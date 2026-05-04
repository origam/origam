#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mvp.Xml.Exslt;
using Origam.DA.ObjectPersistence;
using Origam.Rule.XsltFunctions;
using Origam.Schema.EntityModel;
using Origam.Service.Core;

namespace Origam.Rule.Xslt;

public abstract class MicrosoftXsltEngine : AbstractXsltEngine
{
    private readonly IEnumerable<XsltFunctionsDefinition> functionsDefinitions;

    #region Constructors
    protected MicrosoftXsltEngine()
    {
        functionsDefinitions = XsltFunctionContainerFactory.Create();
    }

    public MicrosoftXsltEngine(IEnumerable<XsltFunctionsDefinition> functionsDefinitions)
        : base()
    {
        this.functionsDefinitions = functionsDefinitions ?? XsltFunctionContainerFactory.Create();
    }

    public MicrosoftXsltEngine(IPersistenceProvider persistence)
        : base(persistence: persistence)
    {
        functionsDefinitions = XsltFunctionContainerFactory.Create();
    }
    #endregion
    internal override IXmlContainer Transform(
        IXmlContainer data,
        object xsltEngine,
        Hashtable parameters,
        string transactionId,
        IDataStructure outputStructure,
        bool validateOnly
    )
    {
        XsltArgumentList xslArg = BuildArgumentListWithFunctions(transactionId: transactionId);
        // If source xml is completely empty (not even a root element), we add one
        // with a name of dataset.datasetname (that's how root element looks like when
        // data come from a dataset.
        // It does not do anything with a non-dataset xml source.
        IDataDocument dataDocument = data as IDataDocument;
        if (data.Xml.DocumentElement == null && dataDocument != null)
        {
            bool oldEnforceConstraints = dataDocument.DataSet.EnforceConstraints;
            dataDocument.DataSet.EnforceConstraints = false;
            dataDocument.AppendChild(
                element: XmlNodeType.Element,
                prefix: dataDocument.DataSet.DataSetName,
                name: ""
            );
            dataDocument.DataSet.EnforceConstraints = oldEnforceConstraints;
        }
        IXmlContainer resultDoc;
        // Generate empty dataset of output structure
        if (outputStructure is DataStructure)
        {
            DataSet result = this.GetEmptyData(dataStructureKey: outputStructure.PrimaryKey);
            // result dataset will not have enabled constraints, because it can load fragments
            // constraints will be checked after merging into original context
            result.EnforceConstraints = false;
            resultDoc = DataDocumentFactory.New(dataSet: result);
        }
        else if (outputStructure == null | outputStructure is XsdDataStructure)
        {
            resultDoc = new XmlContainer();
        }
        else
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(key: "ErrorTransformationSupport")
            );
        }

        try
        {
            StringBuilder traceParameters = new StringBuilder();
            // Parameters
            if (parameters != null)
            {
                foreach (DictionaryEntry param in parameters)
                {
                    object val = param.Value;
                    if (param.Value is IXmlContainer xmlContainer)
                    {
                        XPathDocument paramXpathDoc = new XPathDocument(
                            reader: new XmlNodeReader(node: xmlContainer.Xml)
                        );
                        XPathNavigator nav = paramXpathDoc.CreateNavigator();
                        XPathNodeIterator iterator = nav.Select(xpath: "/");
                        val = iterator;
                    }
                    else if (param.Value is bool)
                    {
                        val = param.Value;
                    }
                    else
                    {
                        val = XmlTools.ConvertToString(val: param.Value);
                    }
                    if (val != null)
                    {
                        xslArg.AddParam(
                            name: param.Key.ToString(),
                            namespaceUri: "",
                            parameter: val
                        );
                    }
                    if (this.Trace)
                    {
                        string traceValue;
                        if (param.Value is IXmlContainer traceXmlContainer)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(value: "AS:ToXml(&apos;");
                            StringWriter sw = new StringWriter(sb: sb);
                            XmlTextWriter xtw = new XmlTextWriter(w: sw);
                            traceXmlContainer.Xml.Save(w: xtw);
                            sb.Append(value: "&apos;)");
                            traceValue = sb.ToString();
                            if (traceValue == "AS:ToXml(&apos;&apos;)")
                            {
                                traceValue = "";
                            }
                        }
                        else if (val == null)
                        {
                            traceValue = "";
                        }
                        else
                        {
                            traceValue = XmlTools.ConvertToString(val: val);
                        }
                        if (traceParameters.Length > 0)
                        {
                            traceParameters.Append(value: Environment.NewLine);
                        }

                        if (traceValue == "")
                        {
                            traceParameters.AppendFormat(
                                format: "<xsl:param name=\"{0}\" />",
                                arg0: param.Key.ToString()
                            );
                        }
                        else
                        {
                            traceParameters.AppendFormat(
                                format: "<xsl:param name=\"{0}\" select=\"{1}\"/>",
                                arg0: param.Key.ToString(),
                                arg1: traceValue
                            );
                        }
                    }
                }
            }
            if (this.Trace)
            {
                StringBuilder b = new StringBuilder();
                StringWriter swr = new StringWriter(sb: b);
                XmlTextWriter xwr = new XmlTextWriter(w: swr);
                xwr.Formatting = Formatting.Indented;
                data.Xml.WriteTo(w: xwr);
                xwr.Close();
                swr.Close();
                TracingService.TraceStep(
                    workflowInstanceId: this.TraceWorkflowId,
                    stepPath: this.TraceStepName,
                    stepId: this.TraceStepId,
                    category: "Transformation Service",
                    subCategory: "Input",
                    remark: null,
                    data1: b.ToString(),
                    data2: traceParameters.ToString(),
                    message: null
                );
            }
            XPathDocument sourceXpathDoc = new XPathDocument(
                reader: new XmlNodeReader(node: data.Xml)
            );
            try
            {
                if (this.Trace && resultDoc is IDataDocument)
                {
                    IXmlContainer traceDocument = new XmlContainer();
                    // first transform to a temporary xml document so we see the clean transformation output
                    Transform(
                        engine: xsltEngine,
                        xslArg: xslArg,
                        sourceXpathDoc: sourceXpathDoc,
                        resultDoc: traceDocument
                    );
                    // trace
                    TraceResult(traceDocument: traceDocument);
                    // after writing the trace we will load the results into the final dataset
                    resultDoc.Load(xmlReader: new XmlNodeReader(node: traceDocument.Xml));
                }
                else
                {
                    // without tracing we transform directly to the target dataset
                    Transform(
                        engine: xsltEngine,
                        xslArg: xslArg,
                        sourceXpathDoc: sourceXpathDoc,
                        resultDoc: resultDoc
                    );
                    if (this.Trace)
                    {
                        TraceResult(traceDocument: resultDoc);
                    }
                }
            }
            catch (XsltException ex)
            {
                if (ex.InnerException is RuleException)
                {
                    throw ex.InnerException;
                }
                string terminateStringEnglish = "Transform terminated:";
                string terminateString = ResourceUtils.GetString(key: "XsltTransformTerminated");
                if (
                    ex.Message.Length >= terminateString.Length
                    && ex.Message.Substring(startIndex: 0, length: terminateString.Length)
                        == terminateString
                )
                {
                    throw new OrigamRuleException(
                        message: ex.Message.Substring(
                            startIndex: terminateString.Length + 1,
                            length: ex.Message.Length - 2 - terminateString.Length
                        ),
                        innerException: ex,
                        row: null
                    );
                }
                if (
                    ex.Message.Length >= terminateStringEnglish.Length
                    && ex.Message.Substring(startIndex: 0, length: terminateStringEnglish.Length)
                        == terminateStringEnglish
                )
                {
                    throw new OrigamRuleException(
                        message: ex.Message.Substring(
                            startIndex: terminateStringEnglish.Length + 1,
                            length: ex.Message.Length - 2 - terminateStringEnglish.Length
                        ),
                        innerException: ex,
                        row: null
                    );
                }
                throw new Exception(
                    message: ResourceUtils.GetString(key: "ErrorResultInvalid"),
                    innerException: ex
                );
            }
            catch (OrigamRuleException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    message: ResourceUtils.GetString(key: "ErrorResultInvalid"),
                    innerException: ex
                );
            }
        }
        catch (OrigamRuleException ex)
        {
            if (this.Trace)
            {
                TracingService.TraceStep(
                    workflowInstanceId: this.TraceWorkflowId,
                    stepPath: this.TraceStepName,
                    stepId: this.TraceStepId,
                    category: "Transformation Service",
                    subCategory: "Error",
                    remark: null,
                    data1: null,
                    data2: null,
                    message: ex.Message
                );
            }
            throw;
        }
        catch (RuleException ex)
        {
            if (Trace)
            {
                TracingService.TraceStep(
                    workflowInstanceId: TraceWorkflowId,
                    stepPath: TraceStepName,
                    stepId: TraceStepId,
                    category: "Transformation Service",
                    subCategory: "Error",
                    remark: null,
                    data1: null,
                    data2: null,
                    message: ex.Message
                );
            }
            throw;
        }
        catch (Exception ex)
        {
            if (this.Trace)
            {
                TracingService.TraceStep(
                    workflowInstanceId: this.TraceWorkflowId,
                    stepPath: this.TraceStepName,
                    stepId: this.TraceStepId,
                    category: "Transformation Service",
                    subCategory: "Error",
                    remark: null,
                    data1: null,
                    data2: null,
                    message: ex.Message
                );
            }
            string innerMessage = (
                ex.InnerException == null
                    ? ex.Message
                    : (
                        ex.InnerException.InnerException == null
                            ? ex.InnerException.Message
                            : (
                                ex.InnerException.InnerException.InnerException == null
                                    ? ex.InnerException.InnerException.Message
                                    : ex.InnerException.InnerException.InnerException.Message
                            )
                    )
            );
            throw new Exception(message: innerMessage, innerException: ex);
        }
        return resultDoc;
    }

    internal override void Transform(
        IXPathNavigable input,
        object xsltEngine,
        Hashtable parameters,
        string transactionId,
        Stream output
    )
    {
        XsltArgumentList xslArg = BuildArgumentListWithFunctions(transactionId: transactionId);
        try
        {
            StringBuilder traceParameters = new StringBuilder();
            if (parameters != null)
            {
                foreach (DictionaryEntry param in parameters)
                {
                    object val = param.Value;
                    if (param.Value is byte[])
                    {
                        val = Convert.ToBase64String(inArray: (byte[])param.Value);
                    }
                    else if (param.Value is DateTime)
                    {
                        val = XmlConvert.ToString(
                            value: (DateTime)param.Value,
                            dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
                        );
                    }
                    else if (param.Value is XmlDocument)
                    {
                        XPathDocument paramXpathDoc = new XPathDocument(
                            reader: new XmlNodeReader(node: param.Value as XmlDocument)
                        );
                        XPathNavigator nav = paramXpathDoc.CreateNavigator();
                        XPathNodeIterator iterator = nav.Select(xpath: "/");
                        val = iterator;
                    }
                    if (val != null)
                    {
                        //throw new NullReferenceException("Transformation input parameter '" + param.Key.ToString() + "' cannot be null.");
                        xslArg.AddParam(
                            name: param.Key.ToString(),
                            namespaceUri: "",
                            parameter: val
                        );
                    }
                    if (this.Trace)
                    {
                        string traceValue;
                        if (param.Value is XmlDocument)
                        {
                            XmlDocument xmlDoc = param.Value as XmlDocument;
                            StringBuilder sb = new StringBuilder();
                            sb.Append(value: "AS:ToXml(&apos;");
                            StringWriter sw = new StringWriter(sb: sb);
                            XmlTextWriter xtw = new XmlTextWriter(w: sw);
                            xmlDoc.Save(w: xtw);
                            sb.Append(value: "&apos;)");
                            traceValue = sb.ToString();
                            if (traceValue == "AS:ToXml(&apos;&apos;)")
                            {
                                traceValue = "";
                            }
                        }
                        else if (val == null)
                        {
                            traceValue = "";
                        }
                        else
                        {
                            traceValue = XmlTools.ConvertToString(val: val);
                        }
                        if (traceParameters.Length > 0)
                        {
                            traceParameters.Append(value: Environment.NewLine);
                        }
                        if (traceValue == "")
                        {
                            traceParameters.AppendFormat(
                                format: "<xsl:param name=\"{0}\" />",
                                arg0: param.Key.ToString()
                            );
                        }
                        else
                        {
                            traceParameters.AppendFormat(
                                format: "<xsl:param name=\"{0}\" select=\"{1}\"/>",
                                arg0: param.Key.ToString(),
                                arg1: traceValue
                            );
                        }
                    }
                }
            }
            try
            {
                Transform(engine: xsltEngine, xslArg: xslArg, input: input, output: output);
            }
            catch (XsltException ex)
            {
                string terminateStringEnglish = "Transform terminated:";
                string terminateString = ResourceUtils.GetString(key: "XsltTransformTerminated");
                if (
                    ex.Message.Length >= terminateString.Length
                    && ex.Message.Substring(startIndex: 0, length: terminateString.Length)
                        == terminateString
                )
                {
                    throw new OrigamRuleException(
                        message: ex.Message.Substring(
                            startIndex: terminateString.Length + 1,
                            length: ex.Message.Length - 2 - terminateString.Length
                        ),
                        innerException: ex,
                        row: null
                    );
                }

                if (
                    ex.Message.Length >= terminateStringEnglish.Length
                    && ex.Message.Substring(startIndex: 0, length: terminateStringEnglish.Length)
                        == terminateStringEnglish
                )
                {
                    throw new OrigamRuleException(
                        message: ex.Message.Substring(
                            startIndex: terminateStringEnglish.Length + 1,
                            length: ex.Message.Length - 2 - terminateStringEnglish.Length
                        ),
                        innerException: ex,
                        row: null
                    );
                }

                throw new Exception(
                    message: ResourceUtils.GetString(key: "ErrorResultInvalid"),
                    innerException: ex
                );
            }
            catch (OrigamRuleException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    message: ResourceUtils.GetString(key: "ErrorResultInvalid"),
                    innerException: ex
                );
            }
        }
        catch (OrigamRuleException ex)
        {
            if (this.Trace)
            {
                TracingService.TraceStep(
                    workflowInstanceId: this.TraceWorkflowId,
                    stepPath: this.TraceStepName,
                    stepId: this.TraceStepId,
                    category: "Transformation Service",
                    subCategory: "Error",
                    remark: null,
                    data1: null,
                    data2: null,
                    message: ex.Message
                );
            }
            throw;
        }
        catch (Exception ex)
        {
            if (this.Trace)
            {
                TracingService.TraceStep(
                    workflowInstanceId: this.TraceWorkflowId,
                    stepPath: this.TraceStepName,
                    stepId: this.TraceStepId,
                    category: "Transformation Service",
                    subCategory: "Error",
                    remark: null,
                    data1: null,
                    data2: null,
                    message: ex.Message
                );
            }
            string innerMessage = (
                ex.InnerException == null
                    ? ex.Message
                    : (
                        ex.InnerException.InnerException == null
                            ? ex.InnerException.Message
                            : (
                                ex.InnerException.InnerException.InnerException == null
                                    ? ex.InnerException.InnerException.Message
                                    : ex.InnerException.InnerException.InnerException.Message
                            )
                    )
            );
            throw new Exception(message: innerMessage, innerException: ex);
        }
    }

    private XsltArgumentList BuildArgumentListWithFunctions(string transactionId)
    {
        XsltArgumentList xslArg = new XsltArgumentList();
        foreach (var functionsDefinition in functionsDefinitions)
        {
            if (
                functionsDefinition.Container
                is IOrigamDependentXsltFunctionContainer origamContainer
            )
            {
                origamContainer.TransactionId = transactionId;
            }
            xslArg.AddExtensionObject(
                namespaceUri: functionsDefinition.NameSpaceUri,
                extension: functionsDefinition.Container
            );
        }
        xslArg.AddExtensionObject(
            namespaceUri: ExsltNamespaces.DatesAndTimes,
            extension: new ExsltDatesAndTimes()
        );
        xslArg.AddExtensionObject(
            namespaceUri: ExsltNamespaces.Strings,
            extension: new ExsltStrings()
        );
        xslArg.AddExtensionObject(
            namespaceUri: ExsltNamespaces.RegularExpressions,
            extension: new ExsltRegularExpressions()
        );
        xslArg.AddExtensionObject(namespaceUri: ExsltNamespaces.Math, extension: new ExsltMath());
        xslArg.AddExtensionObject(
            namespaceUri: ExsltNamespaces.Random,
            extension: new ExsltRandom()
        );
        xslArg.AddExtensionObject(namespaceUri: ExsltNamespaces.Sets, extension: new ExsltSets());
        xslArg.AddExtensionObject(
            namespaceUri: ExsltNamespaces.GdnDatesAndTimes,
            extension: new GdnDatesAndTimes()
        );
        xslArg.AddExtensionObject(namespaceUri: ExsltNamespaces.GdnMath, extension: new GdnMath());
        xslArg.AddExtensionObject(
            namespaceUri: ExsltNamespaces.GdnRegularExpressions,
            extension: new GdnRegularExpressions()
        );
        xslArg.AddExtensionObject(namespaceUri: ExsltNamespaces.GdnSets, extension: new GdnSets());
        xslArg.AddExtensionObject(
            namespaceUri: ExsltNamespaces.GdnStrings,
            extension: new GdnStrings()
        );
        xslArg.AddExtensionObject(
            namespaceUri: ExsltNamespaces.GdnDynamic,
            extension: new GdnDynamic()
        );
        return xslArg;
    }

    private void TraceResult(IXmlContainer traceDocument)
    {
        StringBuilder b = new StringBuilder();
        StringWriter swr = new StringWriter(sb: b);
        XmlTextWriter xwr = new XmlTextWriter(w: swr);
        xwr.Formatting = Formatting.Indented;
        traceDocument.Xml.WriteTo(w: xwr);
        xwr.Close();
        swr.Close();
        TracingService.TraceStep(
            workflowInstanceId: this.TraceWorkflowId,
            stepPath: this.TraceStepName,
            stepId: this.TraceStepId,
            category: "Transformation Service",
            subCategory: "Output",
            remark: null,
            data1: b.ToString(),
            data2: null,
            message: null
        );
    }

    public abstract void Transform(
        object engine,
        XsltArgumentList xslArg,
        XPathDocument sourceXpathDoc,
        XmlTextWriter xwr
    );
    public abstract void Transform(
        object engine,
        XsltArgumentList xslArg,
        XPathDocument sourceXpathDoc,
        IXmlContainer resultDoc
    );
    public abstract void Transform(
        object engine,
        XsltArgumentList xslArg,
        IXPathNavigable input,
        Stream output
    );
}
