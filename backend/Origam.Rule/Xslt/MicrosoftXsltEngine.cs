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
        : base(persistence)
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
        XsltArgumentList xslArg = BuildArgumentListWithFunctions(transactionId);
        // If source xml is completely empty (not even a root element), we add one
        // with a name of dataset.datasetname (that's how root element looks like when
        // data come from a dataset.
        // It does not do anything with a non-dataset xml source.
        IDataDocument dataDocument = data as IDataDocument;
        if (data.Xml.DocumentElement == null && dataDocument != null)
        {
            bool oldEnforceConstraints = dataDocument.DataSet.EnforceConstraints;
            dataDocument.DataSet.EnforceConstraints = false;
            dataDocument.AppendChild(XmlNodeType.Element, dataDocument.DataSet.DataSetName, "");
            dataDocument.DataSet.EnforceConstraints = oldEnforceConstraints;
        }
        IXmlContainer resultDoc;
        // Generate empty dataset of output structure
        if (outputStructure is DataStructure)
        {
            DataSet result = this.GetEmptyData(outputStructure.PrimaryKey);
            // result dataset will not have enabled constraints, because it can load fragments
            // constraints will be checked after merging into original context
            result.EnforceConstraints = false;
            resultDoc = DataDocumentFactory.New(result);
        }
        else if (outputStructure == null | outputStructure is XsdDataStructure)
        {
            resultDoc = new XmlContainer();
        }
        else
        {
            throw new InvalidOperationException(
                ResourceUtils.GetString("ErrorTransformationSupport")
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
                            new XmlNodeReader(xmlContainer.Xml)
                        );
                        XPathNavigator nav = paramXpathDoc.CreateNavigator();
                        XPathNodeIterator iterator = nav.Select("/");
                        val = iterator;
                    }
                    else if (param.Value is bool)
                    {
                        val = param.Value;
                    }
                    else
                    {
                        val = XmlTools.ConvertToString(param.Value);
                    }
                    if (val != null)
                    {
                        xslArg.AddParam(param.Key.ToString(), "", val);
                    }
                    if (this.Trace)
                    {
                        string traceValue;
                        if (param.Value is IXmlContainer traceXmlContainer)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("AS:ToXml(&apos;");
                            StringWriter sw = new StringWriter(sb);
                            XmlTextWriter xtw = new XmlTextWriter(sw);
                            traceXmlContainer.Xml.Save(xtw);
                            sb.Append("&apos;)");
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
                            traceValue = XmlTools.ConvertToString(val);
                        }
                        if (traceParameters.Length > 0)
                        {
                            traceParameters.Append(Environment.NewLine);
                        }

                        if (traceValue == "")
                        {
                            traceParameters.AppendFormat(
                                "<xsl:param name=\"{0}\" />",
                                param.Key.ToString()
                            );
                        }
                        else
                        {
                            traceParameters.AppendFormat(
                                "<xsl:param name=\"{0}\" select=\"{1}\"/>",
                                param.Key.ToString(),
                                traceValue
                            );
                        }
                    }
                }
            }
            if (this.Trace)
            {
                StringBuilder b = new StringBuilder();
                StringWriter swr = new StringWriter(b);
                XmlTextWriter xwr = new XmlTextWriter(swr);
                xwr.Formatting = Formatting.Indented;
                data.Xml.WriteTo(xwr);
                xwr.Close();
                swr.Close();
                TracingService.TraceStep(
                    this.TraceWorkflowId,
                    this.TraceStepName,
                    this.TraceStepId,
                    "Transformation Service",
                    "Input",
                    null,
                    b.ToString(),
                    traceParameters.ToString(),
                    null
                );
            }
            XPathDocument sourceXpathDoc = new XPathDocument(new XmlNodeReader(data.Xml));
            try
            {
                if (this.Trace && resultDoc is IDataDocument)
                {
                    IXmlContainer traceDocument = new XmlContainer();
                    // first transform to a temporary xml document so we see the clean transformation output
                    Transform(xsltEngine, xslArg, sourceXpathDoc, traceDocument);
                    // trace
                    TraceResult(traceDocument);
                    // after writing the trace we will load the results into the final dataset
                    resultDoc.Load(new XmlNodeReader(traceDocument.Xml));
                }
                else
                {
                    // without tracing we transform directly to the target dataset
                    Transform(xsltEngine, xslArg, sourceXpathDoc, resultDoc);
                    if (this.Trace)
                    {
                        TraceResult(resultDoc);
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
                string terminateString = ResourceUtils.GetString("XsltTransformTerminated");
                if (
                    ex.Message.Length >= terminateString.Length
                    && ex.Message.Substring(0, terminateString.Length) == terminateString
                )
                {
                    throw new OrigamRuleException(
                        ex.Message.Substring(
                            terminateString.Length + 1,
                            ex.Message.Length - 2 - terminateString.Length
                        ),
                        ex,
                        null
                    );
                }
                if (
                    ex.Message.Length >= terminateStringEnglish.Length
                    && ex.Message.Substring(0, terminateStringEnglish.Length)
                        == terminateStringEnglish
                )
                {
                    throw new OrigamRuleException(
                        ex.Message.Substring(
                            terminateStringEnglish.Length + 1,
                            ex.Message.Length - 2 - terminateStringEnglish.Length
                        ),
                        ex,
                        null
                    );
                }
                throw new Exception(ResourceUtils.GetString("ErrorResultInvalid"), ex);
            }
            catch (OrigamRuleException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(ResourceUtils.GetString("ErrorResultInvalid"), ex);
            }
        }
        catch (OrigamRuleException ex)
        {
            if (this.Trace)
            {
                TracingService.TraceStep(
                    this.TraceWorkflowId,
                    this.TraceStepName,
                    this.TraceStepId,
                    "Transformation Service",
                    "Error",
                    null,
                    null,
                    null,
                    ex.Message
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
                    this.TraceWorkflowId,
                    this.TraceStepName,
                    this.TraceStepId,
                    "Transformation Service",
                    "Error",
                    null,
                    null,
                    null,
                    ex.Message
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
            throw new Exception(innerMessage, ex);
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
        XsltArgumentList xslArg = BuildArgumentListWithFunctions(transactionId);
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
                        val = Convert.ToBase64String((byte[])param.Value);
                    }
                    else if (param.Value is DateTime)
                    {
                        val = XmlConvert.ToString(
                            (DateTime)param.Value,
                            XmlDateTimeSerializationMode.RoundtripKind
                        );
                    }
                    else if (param.Value is XmlDocument)
                    {
                        XPathDocument paramXpathDoc = new XPathDocument(
                            new XmlNodeReader(param.Value as XmlDocument)
                        );
                        XPathNavigator nav = paramXpathDoc.CreateNavigator();
                        XPathNodeIterator iterator = nav.Select("/");
                        val = iterator;
                    }
                    if (val != null)
                    {
                        //throw new NullReferenceException("Transformation input parameter '" + param.Key.ToString() + "' cannot be null.");
                        xslArg.AddParam(param.Key.ToString(), "", val);
                    }
                    if (this.Trace)
                    {
                        string traceValue;
                        if (param.Value is XmlDocument)
                        {
                            XmlDocument xmlDoc = param.Value as XmlDocument;
                            StringBuilder sb = new StringBuilder();
                            sb.Append("AS:ToXml(&apos;");
                            StringWriter sw = new StringWriter(sb);
                            XmlTextWriter xtw = new XmlTextWriter(sw);
                            xmlDoc.Save(xtw);
                            sb.Append("&apos;)");
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
                            traceValue = XmlTools.ConvertToString(val);
                        }
                        if (traceParameters.Length > 0)
                        {
                            traceParameters.Append(Environment.NewLine);
                        }
                        if (traceValue == "")
                        {
                            traceParameters.AppendFormat(
                                "<xsl:param name=\"{0}\" />",
                                param.Key.ToString()
                            );
                        }
                        else
                        {
                            traceParameters.AppendFormat(
                                "<xsl:param name=\"{0}\" select=\"{1}\"/>",
                                param.Key.ToString(),
                                traceValue
                            );
                        }
                    }
                }
            }
            try
            {
                Transform(xsltEngine, xslArg, input, output);
            }
            catch (XsltException ex)
            {
                string terminateStringEnglish = "Transform terminated:";
                string terminateString = ResourceUtils.GetString("XsltTransformTerminated");
                if (
                    ex.Message.Length >= terminateString.Length
                    && ex.Message.Substring(0, terminateString.Length) == terminateString
                )
                {
                    throw new OrigamRuleException(
                        ex.Message.Substring(
                            terminateString.Length + 1,
                            ex.Message.Length - 2 - terminateString.Length
                        ),
                        ex,
                        null
                    );
                }

                if (
                    ex.Message.Length >= terminateStringEnglish.Length
                    && ex.Message.Substring(0, terminateStringEnglish.Length)
                        == terminateStringEnglish
                )
                {
                    throw new OrigamRuleException(
                        ex.Message.Substring(
                            terminateStringEnglish.Length + 1,
                            ex.Message.Length - 2 - terminateStringEnglish.Length
                        ),
                        ex,
                        null
                    );
                }

                throw new Exception(ResourceUtils.GetString("ErrorResultInvalid"), ex);
            }
            catch (OrigamRuleException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(ResourceUtils.GetString("ErrorResultInvalid"), ex);
            }
        }
        catch (OrigamRuleException ex)
        {
            if (this.Trace)
            {
                TracingService.TraceStep(
                    this.TraceWorkflowId,
                    this.TraceStepName,
                    this.TraceStepId,
                    "Transformation Service",
                    "Error",
                    null,
                    null,
                    null,
                    ex.Message
                );
            }
            throw;
        }
        catch (Exception ex)
        {
            if (this.Trace)
            {
                TracingService.TraceStep(
                    this.TraceWorkflowId,
                    this.TraceStepName,
                    this.TraceStepId,
                    "Transformation Service",
                    "Error",
                    null,
                    null,
                    null,
                    ex.Message
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
            throw new Exception(innerMessage, ex);
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
                functionsDefinition.NameSpaceUri,
                functionsDefinition.Container
            );
        }
        xslArg.AddExtensionObject(ExsltNamespaces.DatesAndTimes, new ExsltDatesAndTimes());
        xslArg.AddExtensionObject(ExsltNamespaces.Strings, new ExsltStrings());
        xslArg.AddExtensionObject(
            ExsltNamespaces.RegularExpressions,
            new ExsltRegularExpressions()
        );
        xslArg.AddExtensionObject(ExsltNamespaces.Math, new ExsltMath());
        xslArg.AddExtensionObject(ExsltNamespaces.Random, new ExsltRandom());
        xslArg.AddExtensionObject(ExsltNamespaces.Sets, new ExsltSets());
        xslArg.AddExtensionObject(ExsltNamespaces.GdnDatesAndTimes, new GdnDatesAndTimes());
        xslArg.AddExtensionObject(ExsltNamespaces.GdnMath, new GdnMath());
        xslArg.AddExtensionObject(
            ExsltNamespaces.GdnRegularExpressions,
            new GdnRegularExpressions()
        );
        xslArg.AddExtensionObject(ExsltNamespaces.GdnSets, new GdnSets());
        xslArg.AddExtensionObject(ExsltNamespaces.GdnStrings, new GdnStrings());
        xslArg.AddExtensionObject(ExsltNamespaces.GdnDynamic, new GdnDynamic());
        return xslArg;
    }

    private void TraceResult(IXmlContainer traceDocument)
    {
        StringBuilder b = new StringBuilder();
        StringWriter swr = new StringWriter(b);
        XmlTextWriter xwr = new XmlTextWriter(swr);
        xwr.Formatting = Formatting.Indented;
        traceDocument.Xml.WriteTo(xwr);
        xwr.Close();
        swr.Close();
        TracingService.TraceStep(
            this.TraceWorkflowId,
            this.TraceStepName,
            this.TraceStepId,
            "Transformation Service",
            "Output",
            null,
            b.ToString(),
            null,
            null
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
