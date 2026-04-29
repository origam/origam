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
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Rule.Xslt;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.GuiModel;
using Origam.Schema.RuleModel;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.UI.Common;
#pragma warning disable IDE0005
using Origam.Workbench;
#pragma warning restore IDE0005
using Origam.Workbench.Services;
using StackExchange.Profiling;

namespace Origam.Rule;

/// <summary>
/// Summary description for Functions.
/// </summary>
public class RuleEngine
{
    private readonly Guid _tracingWorkflowId;
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private static System.Xml.Serialization.XmlSerializer _ruleExceptionSerializer =
        new System.Xml.Serialization.XmlSerializer(
            type: typeof(RuleExceptionDataCollection),
            root: new System.Xml.Serialization.XmlRootAttribute(
                elementName: "RuleExceptionDataCollection"
            )
        );

    private Color NullColor = Color.FromArgb(alpha: 0, red: 0, green: 0, blue: 0);
    IXsltEngine _transformer;
    private IPersistenceService _persistence;
    private IDataLookupService _lookupService;
    private IParameterService _parameterService;
    private ITracingService _tracingService;
    private IDocumentationService _documentationService;
    private IOrigamAuthorizationProvider _authorizationProvider;
    private Func<UserProfile> _userProfileGetter;
    private readonly ResourceTools resourceTools = new(
        businessService: ServiceManager.Services.GetService<IBusinessServicesService>(),
        userProfileGetter: SecurityManager.CurrentUserProfile
    );

    public static RuleEngine Create(
        Hashtable contextStores,
        string transactionId,
        Guid tracingWorkflowId
    )
    {
        return new RuleEngine(
            contextStores: contextStores,
            transactionId: transactionId,
            tracingWorkflowId: tracingWorkflowId,
            persistence: ServiceManager.Services.GetService<IPersistenceService>(),
            lookupService: ServiceManager.Services.GetService<IDataLookupService>(),
            parameterService: ServiceManager.Services.GetService<IParameterService>(),
            tracingService: ServiceManager.Services.GetService<ITracingService>(),
            documentationService: ServiceManager.Services.GetService<IDocumentationService>(),
            authorizationProvider: SecurityManager.GetAuthorizationProvider(),
            userProfileGetter: SecurityManager.CurrentUserProfile
        );
    }

    public static RuleEngine Create()
    {
        return Create(contextStores: new Hashtable(), transactionId: null);
    }

    public static RuleEngine Create(Hashtable contextStores, string transactionId)
    {
        return new RuleEngine(
            contextStores: contextStores,
            transactionId: transactionId,
            persistence: ServiceManager.Services.GetService<IPersistenceService>(),
            lookupService: ServiceManager.Services.GetService<IDataLookupService>(),
            parameterService: ServiceManager.Services.GetService<IParameterService>(),
            tracingService: ServiceManager.Services.GetService<ITracingService>(),
            documentationService: ServiceManager.Services.GetService<IDocumentationService>(),
            authorizationProvider: SecurityManager.GetAuthorizationProvider(),
            userProfileGetter: SecurityManager.CurrentUserProfile
        );
    }

    private RuleEngine(
        Hashtable contextStores,
        string transactionId,
        Guid tracingWorkflowId,
        IPersistenceService persistence,
        IDataLookupService lookupService,
        IParameterService parameterService,
        ITracingService tracingService,
        IDocumentationService documentationService,
        IOrigamAuthorizationProvider authorizationProvider,
        Func<UserProfile> userProfileGetter
    )
        : this(
            contextStores: contextStores,
            transactionId: transactionId,
            persistence: persistence,
            lookupService: lookupService,
            parameterService: parameterService,
            tracingService: tracingService,
            documentationService: documentationService,
            authorizationProvider: authorizationProvider,
            userProfileGetter: userProfileGetter
        )
    {
        _tracingWorkflowId = tracingWorkflowId;
    }

    public RuleEngine(
        Hashtable contextStores,
        string transactionId,
        IPersistenceService persistence,
        IDataLookupService lookupService,
        IParameterService parameterService,
        ITracingService tracingService,
        IDocumentationService documentationService,
        IOrigamAuthorizationProvider authorizationProvider,
        Func<UserProfile> userProfileGetter
    )
    {
        _persistence = persistence;
        _lookupService = lookupService;
        _parameterService = parameterService;
        _tracingService = tracingService;
        _documentationService = documentationService;
        _authorizationProvider = authorizationProvider;
        TransactionId = transactionId;
        _contextStores = contextStores;
        _userProfileGetter = userProfileGetter;
        if (_persistence == null)
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(key: "ErrorInitializeEngine")
            );
        }
        _transformer = new CompiledXsltEngine(persistence: _persistence.SchemaProvider);
    }

    #region Properties
    public static string ValidationNotMetMessage()
    {
        return ResourceUtils.GetString(key: "ErrorOutputRuleFailed");
    }

    public static string ValidationContinueMessage(string message)
    {
        return ResourceUtils.GetString(key: "DoYouWishContinue", args: message);
    }

    public static string ValidationWarningMessage()
    {
        return ResourceUtils.GetString(key: "Warning");
    }

    public static void ConvertStringValueToContextValue(
        OrigamDataType origamDataType,
        string inputString,
        ref object contextValue
    )
    {
        if (inputString != null)
        {
            if (
                inputString == ""
                && origamDataType != OrigamDataType.String
                && origamDataType != OrigamDataType.Memo
            )
            {
                contextValue = null;
            }
            else
            {
                switch (origamDataType)
                {
                    case OrigamDataType.Integer:
                    {
                        contextValue = XmlConvert.ToInt32(s: inputString);
                        break;
                    }

                    case OrigamDataType.Long:
                    {
                        contextValue = XmlConvert.ToInt64(s: inputString);
                        break;
                    }

                    case OrigamDataType.UniqueIdentifier:
                    {
                        contextValue = XmlConvert.ToGuid(s: inputString);
                        break;
                    }

                    case OrigamDataType.Currency:
                    case OrigamDataType.Float:
                    {
                        contextValue = XmlConvert.ToDecimal(s: inputString);
                        break;
                    }

                    case OrigamDataType.Date:
                    {
                        contextValue = XmlConvert.ToDateTime(
                            s: inputString,
                            dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
                        );
                        break;
                    }

                    case OrigamDataType.Boolean:
                    {
                        contextValue = XmlConvert.ToBoolean(s: inputString);
                        break;
                    }

                    case OrigamDataType.String:
                    case OrigamDataType.Memo:
                    {
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "dataType",
                            actualValue: origamDataType,
                            message: "Unsupported data type."
                        );
                    }
                }
            }
        }
    }

    Hashtable _contextStores;
    private string _transactionId = null;

    public string TransactionId
    {
        get { return _transactionId; }
        set { _transactionId = value; }
    }
    #endregion
    #region Public Functions
    #region XSL Functions

    private string ActiveProfileId()
    {
        UserProfile profile = _userProfileGetter();
        return profile.Id.ToString();
    }

    private string LookupValue(string lookupId, string recordId)
    {
        object result = _lookupService.GetDisplayText(
            lookupId: new Guid(g: lookupId),
            lookupValue: recordId,
            useCache: false,
            returnMessageIfNull: false,
            transactionId: this.TransactionId
        );

        return XmlTools.FormatXmlString(value: result);
    }
    #endregion
    #region Other Functions
    public object EvaluateRule(IRule rule, object data, XPathNodeIterator contextPosition)
    {
        return EvaluateRule(
            rule: rule,
            data: data,
            contextPosition: contextPosition,
            parentIsTracing: false
        );
    }

    public object EvaluateRule(
        IRule rule,
        object data,
        XPathNodeIterator contextPosition,
        bool parentIsTracing
    )
    {
        try
        {
            IXmlContainer xmlData = GetXmlDocumentFromData(inputData: data);
            bool ruleEvaluationDidRun = false;
            object ruleResult = null;
            switch (rule)
            {
                case XPathRule pathRule:
                {
                    ruleResult = EvaluateRule(
                        rule: pathRule,
                        context: xmlData,
                        contextPosition: contextPosition
                    );
                    ruleEvaluationDidRun = true;
                    break;
                }

                case XslRule xslRule:
                {
                    ruleResult = EvaluateRule(rule: xslRule, context: xmlData);
                    ruleEvaluationDidRun = true;
                    break;
                }
            }

            if (
                (
                    rule.Trace == Origam.Trace.Yes
                    || (rule.Trace == Origam.Trace.InheritFromParent && parentIsTracing)
                ) && ruleEvaluationDidRun
            )
            {
                _tracingService.TraceRule(
                    ruleId: rule.Id,
                    ruleName: rule.Name,
                    ruleInput: xmlData?.Xml?.OuterXml,
                    ruleResult: ruleResult?.ToString(),
                    workflowInstanceId: _tracingWorkflowId
                );
            }
            return ruleResult;
        }
        catch (OrigamRuleException)
        {
            throw;
        }
        catch (Exception ex)
        {
            string errorMessage = ResourceUtils.GetString(key: "ErrorRuleFailed", args: rule.Name);
            if (_documentationService != null)
            {
                string doc = _documentationService.GetDocumentation(
                    schemaItemId: (Guid)rule.PrimaryKey[key: "Id"],
                    docType: DocumentationType.RULE_EXCEPTION_MESSAGE
                );
                if (doc != "")
                {
                    errorMessage += Environment.NewLine + doc;
                }
            }
            throw new Exception(message: errorMessage, innerException: ex);
        }
    }

    public RuleExceptionDataCollection EvaluateEndRule(IEndRule rule, object data)
    {
        return EvaluateEndRule(
            rule: rule,
            data: data,
            parameters: new Hashtable(),
            parentIsTracing: false
        );
    }

    public RuleExceptionDataCollection EvaluateEndRule(
        IEndRule rule,
        object data,
        bool parentIsTracing
    )
    {
        return EvaluateEndRule(
            rule: rule,
            data: data,
            parameters: new Hashtable(),
            parentIsTracing: parentIsTracing
        );
    }

    public RuleExceptionDataCollection EvaluateEndRule(
        IEndRule rule,
        object data,
        Hashtable parameters
    )
    {
        return EvaluateEndRule(
            rule: rule,
            data: data,
            parameters: parameters,
            parentIsTracing: false
        );
    }

    public RuleExceptionDataCollection EvaluateEndRule(
        IEndRule rule,
        object data,
        Hashtable parameters,
        bool parentIsTracing
    )
    {
        IXmlContainer context = GetXmlDocumentFromData(inputData: data);
        IXmlContainer result = null;
        try
        {
            if (rule is XslRule)
            {
                XslRule xslRule = rule as XslRule;
                result = _transformer.Transform(
                    data: context,
                    transformationId: xslRule.Id,
                    parameters: parameters,
                    transactionId: _transactionId,
                    outputStructure: new XsdDataStructure(),
                    validateOnly: false
                );
            }
            else if (rule is XPathRule)
            {
                string ruleText = (string)
                    this.EvaluateRule(rule: rule, data: context, contextPosition: null);
                result = _transformer.Transform(
                    data: context,
                    xsl: ruleText,
                    parameters: parameters,
                    transactionId: TransactionId,
                    outputStructure: new XsdDataStructure(),
                    validateOnly: false
                );
            }
            else
            {
                throw new Exception(
                    message: ResourceUtils.GetString(key: "ErrorOnlyXslRuleSupported")
                );
            }

            RuleExceptionDataCollection exceptions = DeserializeRuleExceptions(xmlDoc: result.Xml);

            if (
                rule.Trace == Origam.Trace.Yes
                || (rule.Trace == Origam.Trace.InheritFromParent && parentIsTracing)
            )
            {
                _tracingService.TraceRule(
                    ruleId: rule.Id,
                    ruleName: rule.Name,
                    ruleInput: context?.Xml?.OuterXml,
                    ruleResult: result.Xml.OuterXml,
                    workflowInstanceId: _tracingWorkflowId
                );
            }

            return exceptions;
        }
        // due to a complex situation, a rule exception can be raised inside
        // a workflow used to retrieve a look-up value by rule
        catch (RuleException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception(
                message: ResourceUtils.GetString(key: "ErrorRuleFailed1", args: rule.Name),
                innerException: ex
            );
        }
    }

    private RuleExceptionDataCollection DeserializeRuleExceptions(XmlDocument xmlDoc)
    {
        if (xmlDoc == null)
        {
            return null;
        }
        XmlNodeReader reader = new XmlNodeReader(node: xmlDoc);
        RuleExceptionDataCollection exceptions = null;
        try
        {
            if (reader.ReadToFollowing(name: "RuleExceptionDataCollection"))
            {
                exceptions = (RuleExceptionDataCollection)
                    _ruleExceptionSerializer.Deserialize(xmlReader: reader);
            }
        }
        finally
        {
            reader.Close();
        }
        return exceptions;
    }

    public object Evaluate(ISchemaItem item)
    {
        if (item is DataStructureReference)
        {
            return Evaluate(reference: item as DataStructureReference);
        }

        if (item is SystemFunctionCall)
        {
            return Evaluate(functionCall: item as SystemFunctionCall);
        }

        if (item is TransformationReference)
        {
            return Evaluate(reference: item as TransformationReference);
        }

        if (item is ReportReference)
        {
            return (item as ReportReference).ReportId;
        }

        if (item is DataConstantReference)
        {
            return _parameterService.GetParameterValue(
                id: (item as DataConstantReference).DataConstant.Id
            );
        }

        if (item is WorkflowReference)
        {
            return (item as WorkflowReference).WorkflowId;
        }

        throw new ArgumentOutOfRangeException(
            paramName: "item",
            actualValue: item,
            message: ResourceUtils.GetString(key: "ErrorRuleInvalidType")
        );
    }

    public bool Merge(
        DataSet inout_dsTarget,
        DataSet in_dsSource,
        bool in_bTrueDelete,
        bool in_bPreserveChanges,
        bool in_bSourceIsFragment,
        bool preserveNewRowState
    )
    {
        bool result;
        bool constraintsWereEnforced = inout_dsTarget.EnforceConstraints;
        DatasetTools.BeginLoadData(dataset: inout_dsTarget);
        try
        {
            MergeParams mergeParams = new MergeParams();
            mergeParams.TrueDelete = in_bTrueDelete;
            mergeParams.PreserveChanges = in_bPreserveChanges;
            mergeParams.SourceIsFragment = in_bSourceIsFragment;
            mergeParams.PreserveNewRowState = preserveNewRowState;
            mergeParams.ProfileId = _userProfileGetter().Id;
            result = DatasetTools.MergeDataSet(
                inout_dsTarget: inout_dsTarget,
                in_dsSource: in_dsSource,
                changeList: null,
                mergeParams: mergeParams
            );
        }
        finally
        {
            DatasetTools.EndLoadData(dataset: inout_dsTarget);
            inout_dsTarget.EnforceConstraints = constraintsWereEnforced;
        }
        return result;
    }

    public object EvaluateContext(
        string xpath,
        object context,
        OrigamDataType dataType,
        AbstractDataStructure targetStructure
    )
    {
        object result = null;
        if (!(context is XmlDocument))
        {
            if (xpath == "/")
            {
                return context;
            }
            // convert value to XML
            context = GetXmlDocumentFromData(inputData: context).Xml;
        }

        if (context is XmlDocument)
        {
            if (dataType == OrigamDataType.Xml && xpath.Trim() == "/")
            {
                return context;
            }
            OrigamXsltContext ctx = OrigamXsltContext.Create(
                nameTable: new NameTable(),
                transactionId: _transactionId
            );
            XPathNavigator nav = ((XmlDocument)context).CreateNavigator();
            XPathExpression expr = nav.Compile(xpath: xpath);
            expr.SetContext(nsManager: ctx);

            if (dataType == OrigamDataType.Array)
            {
                object expressionResult = nav.Evaluate(expr: expr);
                result = new ArrayList();
                if (expressionResult is XPathNodeIterator)
                {
                    XPathNodeIterator iterator = expressionResult as XPathNodeIterator;
                    while (iterator.MoveNext())
                    {
                        ((ArrayList)result).Add(value: iterator.Current.Value);
                    }
                }
            }
            else if (dataType != OrigamDataType.Xml)
            {
                // Result is other than XML

                result = nav.Evaluate(expr: expr);
                if (result is XPathNodeIterator)
                {
                    XPathNodeIterator iterator = result as XPathNodeIterator;
                    if (iterator.Count == 0)
                    {
                        result = null;
                    }
                    else
                    {
                        iterator.MoveNext();
                        result = iterator.Current.Value;
                    }
                }
                switch (dataType)
                {
                    case OrigamDataType.Blob:
                    {
                        if (!(result is String))
                        {
                            throw new InvalidCastException(
                                message: "Only string can be converted to blob."
                            );
                        }
                        result = Convert.FromBase64String(s: (string)result);
                        break;
                    }

                    case OrigamDataType.Boolean:
                    {
                        if (!(result is bool))
                        {
                            if (result == null)
                            {
                                result = false;
                            }
                            else if (
                                result is string
                                && ((string)result == "0" || (string)result == "false")
                            )
                            {
                                result = false;
                            }
                            else
                            {
                                result = true;
                            }
                        }
                        break;
                    }

                    case OrigamDataType.UniqueIdentifier:
                    {
                        if (result != null)
                        {
                            if ((string)result == "")
                            {
                                result = null;
                            }
                            else
                            {
                                result = new Guid(g: result.ToString());
                            }
                        }
                        break;
                    }

                    case OrigamDataType.Integer:
                    {
                        if (!(result is Int32) & result != null)
                        {
                            result = Convert.ToInt32(value: result);
                        }
                        break;
                    }

                    case OrigamDataType.Float:
                    case OrigamDataType.Currency:
                    {
                        if (!(result is Decimal) && result != null)
                        {
                            result = XmlConvert.ToDecimal(s: result.ToString());
                        }
                        break;
                    }

                    case OrigamDataType.Date:
                    {
                        if (!(result is DateTime) && result != null)
                        {
                            result = XmlConvert.ToDateTime(
                                s: result.ToString(),
                                dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
                            );
                        }
                        break;
                    }
                }
            }
            else
            {
                // result is XML
                XmlNodeList results = new XPathNodeList(iterator: nav.Select(expr: expr));
                XmlDocument resultDoc = new XmlDocument();
                XmlNode docElement = resultDoc.ImportNode(
                    node: ((XmlDocument)context).DocumentElement,
                    deep: false
                );
                resultDoc.AppendChild(newChild: docElement);

                foreach (XmlNode node in results)
                {
                    if (node is XmlDocument)
                    {
                        resultDoc = node as XmlDocument;
                    }
                    else
                    {
                        docElement.AppendChild(
                            newChild: resultDoc.ImportNode(node: node, deep: true)
                        );
                    }
                }
                if (targetStructure is DataStructure)
                {
                    // we clone the dataset (no data, just the structure)
                    DataSet dataset = new DatasetGenerator(
                        userDefinedParameters: true
                    ).CreateDataSet(ds: targetStructure as DataStructure);

                    dataset.EnforceConstraints = false;
                    // we load the iteration data into the dataset
                    try
                    {
                        dataset.ReadXml(
                            reader: new XmlNodeReader(node: resultDoc),
                            mode: XmlReadMode.IgnoreSchema
                        );
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            message: ResourceUtils.GetString(
                                key: "ErrorEvaluateContextFailed",
                                args: ex.Message
                            ),
                            innerException: ex
                        );
                    }
                    // we add the context into the called engine
                    result = DataDocumentFactory.New(dataSet: dataset);
                }
                else
                {
                    result = new XmlContainer(xmlDocument: resultDoc);
                }
            }
        }
        else
        {
            // context is not xml document
            result = context;
        }
        return result;
    }

    private DataStructureRuleSet _ruleSet = null;
    private IDataDocument _currentRuleDocument = null;

    public void ProcessRules(IDataDocument data, DataStructureRuleSet ruleSet, DataRow contextRow)
    {
        if (data != null && data.DataSet != null)
        {
            _currentRuleDocument = data;
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                paramName: "data",
                actualValue: data,
                message: ResourceUtils.GetString(key: "ErrorTypeNotProcessable")
            );
        }
        _ruleSet = ruleSet;
        /********************************************************************
         * Column bound rules
         ********************************************************************/
        if (contextRow == null) // whole dataset
        {
            Hashtable cols = new Hashtable();
            CompleteChildColumnReferences(data: data.DataSet, cols: cols);

            EnqueueAllRows(data: data, ruleSet: ruleSet, columns: cols);
        }
        else // current row
        {
            Hashtable cols = new Hashtable();
            CompleteChildColumnReferences(table: contextRow.Table, cols: cols);

            EnqueueAllRows(currentRow: contextRow, data: data, ruleSet: ruleSet, columns: cols);
        }
        /********************************************************************
         * Row bound rules
         ********************************************************************/
        List<DataTable> sortedTables = GetSortedTables(dataset: data.DataSet);
        try
        {
            foreach (DataTable table in sortedTables)
            {
                RegisterTableEvents(table: table);
            }
            ProcessRuleQueue();
        }
        finally
        {
            foreach (DataTable table in sortedTables)
            {
                UnregisterTableEvents(table: table);
            }

            _ruleSet = null;
        }
    }

    private void RegisterTableEvents(DataTable table)
    {
        table.RowChanged += new DataRowChangeEventHandler(table_RowChanged);
        table.ColumnChanged += new DataColumnChangeEventHandler(table_ColumnChanged);
    }

    private void UnregisterTableEvents(DataTable table)
    {
        table.RowChanged -= new DataRowChangeEventHandler(table_RowChanged);
        table.ColumnChanged -= new DataColumnChangeEventHandler(table_ColumnChanged);
    }

    private void CompleteChildColumnReferences(DataTable table, Hashtable cols)
    {
        foreach (DataColumn col in table.Columns)
        {
            if (col.ExtendedProperties.Contains(key: "Id"))
            {
                cols[key: ColumnKey(col: col)] = col;
            }
        }
        foreach (DataRelation rel in table.ChildRelations)
        {
            CompleteChildColumnReferences(table: rel.ChildTable, cols: cols);
        }
    }

    private void CompleteChildColumnReferences(DataSet data, Hashtable cols)
    {
        foreach (DataTable table in data.Tables)
        {
            foreach (DataColumn col in table.Columns)
            {
                if (col.ExtendedProperties.Contains(key: "Id"))
                {
                    cols[key: ColumnKey(col: col)] = col;
                }
            }
        }
    }

    private List<DataTable> GetSortedTables(DataSet dataset)
    {
        var result = new List<DataTable>();
        foreach (DataTable table in dataset.Tables)
        {
            if (table.ParentRelations.Count == 0)
            {
                GetChildTables(table: table, list: result);
            }
        }
        return result;
    }

    private void GetChildTables(DataTable table, List<DataTable> list)
    {
        foreach (DataRelation childRelation in table.ChildRelations)
        {
            GetChildTables(table: childRelation.ChildTable, list: list);
        }
        list.Add(item: table);
    }

    /// <summary>
    /// Processes rules after the column value has changed.
    /// </summary>
    /// <param name="rowChanged"></param>
    /// <param name="columnChanged"></param>
    /// <param name="ruleSet"></param>
    public void ProcessRules(
        DataRow rowChanged,
        IDataDocument data,
        DataColumn columnChanged,
        DataStructureRuleSet ruleSet
    )
    {
        ProcessRulesInternal(
            rowChanged: rowChanged,
            data: data,
            columnChanged: columnChanged,
            ruleSet: ruleSet,
            columnsChanged: null,
            isFromRuleQueue: false
        );
    }

    internal void ProcessRules(
        DataRow rowChanged,
        IDataDocument data,
        ICollection columnsChanged,
        DataStructureRuleSet ruleSet
    )
    {
        ProcessRulesInternal(
            rowChanged: rowChanged,
            data: data,
            columnChanged: null,
            ruleSet: ruleSet,
            columnsChanged: columnsChanged,
            isFromRuleQueue: false
        );
    }

    private bool ProcessRulesFromQueue(
        DataRow rowChanged,
        IDataDocument data,
        DataStructureRuleSet ruleSet,
        ICollection columnsChanged
    )
    {
        return ProcessRulesInternal(
            rowChanged: rowChanged,
            data: data,
            columnChanged: null,
            ruleSet: ruleSet,
            columnsChanged: columnsChanged,
            isFromRuleQueue: true
        );
    }

    private IndexedRuleQueue _ruleQueue = new();
    private Hashtable _ruleColumnChanges = new Hashtable();

    private bool IsEntryInQueue(DataRow rowChanged, DataStructureRuleSet ruleSet)
    {
        return _ruleQueue.Contains(row: rowChanged, ruleSet: ruleSet);
    }

    private void UpdateQueueEntries(
        DataRow rowChanged,
        DataStructureRuleSet ruleSet,
        DataColumn column
    )
    {
        foreach (object[] queueEntry in _ruleQueue)
        {
            if (
                !queueEntry[0].Equals(obj: rowChanged)
                && (
                    (queueEntry[1] != null && queueEntry[1].Equals(obj: ruleSet))
                    || (queueEntry[1] == null && ruleSet == null)
                )
            )
            {
                Hashtable h = queueEntry[2] as Hashtable;
                h[key: ColumnKey(col: column)] = column;
            }
        }
    }

    private void EnqueueEntry(
        DataRow rowChanged,
        IDataDocument data,
        DataStructureRuleSet ruleSet,
        Hashtable columns
    )
    {
        if (rowChanged.RowState == DataRowState.Deleted)
        {
            return;
        }

        if (columns == null)
        {
            // only pass other entities's column changes (e.g. from children to parents)
            columns = new Hashtable();
            foreach (DataColumn col in _ruleColumnChanges.Values)
            {
                if (!col.Table.TableName.Equals(value: rowChanged.Table.TableName))
                {
                    columns.Add(key: ColumnKey(col: col), value: col);
                }
            }
        }
        object[] queueEntry = new object[4] { rowChanged, ruleSet, columns, data };
        _ruleQueue.Enqueue(entry: queueEntry);
    }

    private static string ColumnKey(DataColumn col)
    {
        return col.Table.TableName + "_" + col.ExtendedProperties[key: "Id"].ToString();
    }

    private void EnqueueAllRows(
        DataRow currentRow,
        IDataDocument data,
        DataStructureRuleSet ruleSet,
        Hashtable columns
    )
    {
        if (!IsEntryInQueue(rowChanged: currentRow, ruleSet: ruleSet))
        {
            EnqueueEntry(rowChanged: currentRow, data: data, ruleSet: ruleSet, columns: columns);
        }
        EnqueueChildRows(parentRow: currentRow, data: data, ruleSet: ruleSet, columns: columns);
        EnqueueParentRows(
            childRow: currentRow,
            data: data,
            ruleSet: ruleSet,
            columns: columns,
            parentRows: null
        );
    }

    private void EnqueueAllRows(IDataDocument data, DataStructureRuleSet ruleSet, Hashtable columns)
    {
        List<DataTable> tables = GetSortedTables(dataset: data.DataSet);
        for (int i = tables.Count - 1; i >= 0; i--)
        {
            foreach (DataRow row in tables[index: i].Rows)
            {
                EnqueueEntry(rowChanged: row, data: data, ruleSet: ruleSet, columns: columns);
            }
        }
    }

    private void EnqueueChildRows(
        DataRow parentRow,
        IDataDocument data,
        DataStructureRuleSet ruleSet,
        Hashtable columns
    )
    {
        if (
            parentRow.RowState != DataRowState.Detached
            && parentRow.RowState != DataRowState.Deleted
        )
        {
            foreach (DataRelation childRelation in parentRow.Table.ChildRelations)
            {
                foreach (DataRow row in parentRow.GetChildRows(relation: childRelation))
                {
                    if (!IsEntryInQueue(rowChanged: row, ruleSet: ruleSet))
                    {
                        EnqueueEntry(
                            rowChanged: row,
                            data: data,
                            ruleSet: ruleSet,
                            columns: columns
                        );
                    }
                    EnqueueChildRows(
                        parentRow: row,
                        data: data,
                        ruleSet: ruleSet,
                        columns: columns
                    );
                }
            }
        }
    }

    private void EnqueueParentRows(
        DataRow childRow,
        IDataDocument data,
        DataStructureRuleSet ruleSet,
        Hashtable columns,
        DataRow[] parentRows
    )
    {
        var rows = new List<DataRow>();
        if (parentRows == null)
        {
            foreach (DataRelation parentRelation in childRow.Table.ParentRelations)
            {
                foreach (DataRow row in childRow.GetParentRows(relation: parentRelation))
                {
                    rows.Add(item: row);
                }
            }
        }
        else
        {
            rows.AddRange(collection: parentRows);
        }
        foreach (DataRow row in rows)
        {
            if (!IsEntryInQueue(rowChanged: row, ruleSet: ruleSet))
            {
                EnqueueEntry(rowChanged: row, data: data, ruleSet: ruleSet, columns: columns);
            }
            EnqueueParentRows(
                childRow: row,
                data: data,
                ruleSet: ruleSet,
                columns: columns,
                parentRows: null
            );
        }
    }

    public void ProcessRules(DataRow rowChanged, IDataDocument data, DataStructureRuleSet ruleSet)
    {
        ProcessRules(rowChanged: rowChanged, data: data, ruleSet: ruleSet, parentRows: null);
    }

    /// <summary>
    /// Processes rules after the row was commited for change.
    /// </summary>
    /// <param name="rowChanged"></param>
    /// <param name="ruleSet"></param>
    public void ProcessRules(
        DataRow rowChanged,
        IDataDocument data,
        DataStructureRuleSet ruleSet,
        DataRow[] parentRows
    )
    {
        bool wasQueued = false;
        if (IsEntryInQueue(rowChanged: rowChanged, ruleSet: ruleSet))
        {
            wasQueued = true;
        }
        else
        {
            EnqueueEntry(rowChanged: rowChanged, data: data, ruleSet: ruleSet, columns: null);
        }
        EnqueueChildRows(parentRow: rowChanged, data: data, ruleSet: ruleSet, columns: null);
        Hashtable columns = null;
        if (rowChanged.RowState == DataRowState.Deleted)
        {
            columns = new Hashtable();
            foreach (DataColumn col in rowChanged.Table.Columns)
            {
                columns[key: ColumnKey(col: col)] = col;
            }
        }
        EnqueueParentRows(
            childRow: rowChanged,
            data: data,
            ruleSet: ruleSet,
            columns: columns,
            parentRows: parentRows
        );
        if (wasQueued)
        {
            return;
        }

        _ruleColumnChanges.Clear();
        ProcessRuleQueue();
    }

    private bool _ruleProcessingPaused = false;

    public void PauseRuleProcessing()
    {
        _ruleProcessingPaused = true;
    }

    public void ResumeRuleProcessing()
    {
        _ruleProcessingPaused = false;
    }

    public void ProcessRuleQueue()
    {
        if (!_ruleProcessingPaused)
        {
            while (_ruleQueue.Count != 0)
            {
                object[] queueEntry = (object[])_ruleQueue.Peek();
                DataRow row = queueEntry[0] as DataRow;
                DataStructureRuleSet rs = queueEntry[1] as DataStructureRuleSet;
                Hashtable changedColumns = queueEntry[2] as Hashtable;
                IDataDocument data = queueEntry[3] as IDataDocument;
                row.BeginEdit();
                try
                {
                    // Process rules on the changed row.
                    if (
                        ProcessRulesFromQueue(
                            rowChanged: row,
                            data: data,
                            ruleSet: rs,
                            columnsChanged: changedColumns.Values
                        )
                    )
                    {
                        try
                        {
                            row.EndEdit();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                message: ex.Message
                                    + Environment.NewLine
                                    + ResourceUtils.GetString(key: "RowState")
                                    + row.RowState.ToString(),
                                innerException: ex
                            );
                        }
                    }
                }
                catch (Exception e)
                {
                    row.CancelEdit();
                    if (log.IsErrorEnabled)
                    {
                        log.Error(
                            message: "Exception ocurred during evaluation of rule queue",
                            exception: e
                        );
                    }
                    _ruleQueue.Clear();
                    throw;
                }
                row.CancelEdit();
                _ruleQueue.Dequeue();
            }
        }
    }

    private bool ProcessRulesInternal(
        DataRow rowChanged,
        IDataDocument data,
        DataColumn columnChanged,
        DataStructureRuleSet ruleSet,
        ICollection columnsChanged,
        bool isFromRuleQueue
    )
    {
        if (_ruleProcessingPaused)
        {
            return false;
        }

        if (columnChanged == null && columnsChanged.Count == 0)
        {
            return false;
        }

        if (!DatasetTools.HasRowValidParent(row: rowChanged))
        {
            return false;
        }

        bool result = false;
        bool resultRules = false;
        var outputPad = GetOutputPad();
        if (ruleSet != null)
        {
            List<DataStructureRule> rules;
            if (columnChanged == null)
            {
                rules = ruleSet.Rules(entityName: rowChanged.Table.TableName);
                foreach (DataColumn col in columnsChanged)
                {
                    // get all the rules
                    if (col.ExtendedProperties.Contains(key: "Id"))
                    {
                        Guid fieldId = (Guid)col.ExtendedProperties[key: "Id"];
                        List<DataStructureRule> r = ruleSet.Rules(
                            entityName: col.Table.TableName,
                            fieldId: fieldId,
                            includeOtherEntities: isFromRuleQueue
                        );
                        foreach (DataStructureRule rule in r)
                        {
                            if (rule.Entity.Name.Equals(value: rowChanged.Table.TableName))
                            {
                                if (!rules.Contains(item: rule))
                                {
                                    rules.Add(item: rule);
                                }
                            }
                        }
                    }
                    if (!isFromRuleQueue)
                    {
                        UpdateQueueEntries(rowChanged: rowChanged, ruleSet: ruleSet, column: col);
                        _ruleColumnChanges[key: ColumnKey(col: col)] = col;
                    }
                }
            }
            else
            {
                // columns we cannot recognize will not fire any events
                if (!columnChanged.ExtendedProperties.Contains(key: "Id"))
                {
                    return false;
                }

                Guid fieldId = (Guid)columnChanged.ExtendedProperties[key: "Id"];
                rules = ruleSet.Rules(
                    entityName: rowChanged.Table.TableName,
                    fieldId: fieldId,
                    includeOtherEntities: false
                );
                UpdateQueueEntries(rowChanged: rowChanged, ruleSet: ruleSet, column: columnChanged);
                _ruleColumnChanges[key: ColumnKey(col: columnChanged)] = columnChanged;
            }
            rules.Sort(comparer: new ProcessRuleComparer());
            if (rules.Count > 0)
            {
                if (outputPad != null)
                {
                    string pk = "";
                    foreach (DataColumn column in rowChanged.Table.PrimaryKey)
                    {
                        if (pk != "")
                        {
                            pk += ", ";
                        }

                        pk += column.ColumnName + ": " + rowChanged[column: column].ToString();
                    }
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                            message: ResourceUtils.GetString(
                                key: "PadProcessingRules",
                                args:
                                [
                                    DateTime.Now.ToString(),
                                    ruleSet.Name,
                                    rowChanged.Table.TableName,
                                    pk,
                                    (columnChanged == null ? "<none>" : columnChanged?.ColumnName),
                                ]
                            )
                        );
                    }
                }
            }
            resultRules = ProcessRulesInternalFinish(
                rules: rules,
                data: data,
                rowChanged: rowChanged,
                outputPad: outputPad,
                ruleSet: ruleSet
            );
        }
        // check for lookup fields changes
        if (columnChanged == null)
        {
            var copy = columnsChanged.ToArray<DataColumn>();
            foreach (DataColumn col in copy)
            {
                if (col.Table.TableName == rowChanged.Table.TableName)
                {
                    result = ProcessRulesLookupFields(row: rowChanged, columnName: col.ColumnName);
                }
            }
        }
        else
        {
            result = ProcessRulesLookupFields(
                row: rowChanged,
                columnName: columnChanged.ColumnName
            );
        }
        return result || resultRules;
    }

    private static IOutputPad GetOutputPad()
    {
        IOutputPad outputPad = null;
#if !NETSTANDARD
        if (WorkbenchSingleton.Workbench != null)
        {
            outputPad = WorkbenchSingleton.Workbench.GetPad(type: typeof(IOutputPad)) as IOutputPad;
        }
#endif
        return outputPad;
    }

    public bool ProcessRulesLookupFields(DataRow row, string columnName)
    {
        bool changed = false;
        DataTable t = row.Table;
        Guid columnFieldId = (Guid)t.Columns[name: columnName].ExtendedProperties[key: "Id"];
        foreach (DataColumn column in t.Columns)
        {
            if (column.ExtendedProperties.Contains(key: Const.OriginalFieldId))
            {
                Guid originalFieldId = (Guid)column.ExtendedProperties[key: Const.OriginalFieldId];
                // we find all columns that depend on the changed one
                if (originalFieldId.Equals(g: columnFieldId))
                {
                    if (column.ExtendedProperties.Contains(key: Const.OriginalLookupIdAttribute))
                    {
                        // and we reload the value by the original lookup
                        Guid originalLookupId = (Guid)
                            column.ExtendedProperties[key: Const.OriginalLookupIdAttribute];
                        object newValue = DBNull.Value;

                        if (!row.IsNull(columnName: columnName))
                        {
                            newValue = _lookupService.GetDisplayText(
                                lookupId: originalLookupId,
                                lookupValue: row[columnName: columnName],
                                useCache: false,
                                returnMessageIfNull: false,
                                transactionId: this.TransactionId
                            );
                        }

                        if (newValue == null)
                        {
                            newValue = DBNull.Value;
                        }
                        if (row[column: column] != newValue)
                        {
                            row[column: column] = newValue;
                            changed = true;
                        }
                    }
                    else
                    {
                        // or we just copy the original value (copied fields)
                        if (row[column: column] != row[columnName: columnName])
                        {
                            row[column: column] = row[columnName: columnName];
                        }
                    }
                }
            }
        }

        return changed;
    }

    private bool ProcessRulesInternalFinish(
        List<DataStructureRule> rules,
        IDataDocument data,
        DataRow rowChanged,
        IOutputPad outputPad,
        DataStructureRuleSet ruleSet
    )
    {
        bool changed = false;
        var myRules = new List<DataStructureRule>(collection: rules);
        foreach (DataStructureRule rule in myRules)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    message: "Evaluating Rule: "
                        + rule?.Name
                        + ", Target Field: "
                        + (rule?.TargetField == null ? "<none>" : rule?.TargetField.Name)
                );
            }
            // columns which don't allow nulls will not get processed when empty
            foreach (DataStructureRuleDependency dependency in rule.RuleDependencies)
            {
                if (dependency.Entity.Name.Equals(value: rowChanged.Table.TableName))
                {
                    if (
                        !dependency.Field.AllowNulls
                        && rowChanged[columnName: dependency.Field.Name] == DBNull.Value
                    )
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(
                                message: "   "
                                    + ResourceUtils.GetString(
                                        key: "PadAllowNulls",
                                        args: [dependency.Entity.Name, dependency.Field.Name]
                                    )
                            );
                        }
                        goto nextRule;
                    }
                }
            }
            XPathNodeIterator iterator = null;
            // we do a fresh slice after evaluating each rule, because data could have changed
            DataSet dataSlice = DatasetTools.CloneDataSet(
                dataset: rowChanged.Table.DataSet,
                cloneExpressions: false
            );
            DatasetTools.GetDataSlice(target: dataSlice, rows: new List<DataRow> { rowChanged });
            IDataDocument xmlSlice = DataDocumentFactory.New(dataSet: dataSlice);
            if (rule.ValueRule == null)
            {
                throw new Exception(
                    message: $"{nameof(DataStructureRule.ValueRule)} in {nameof(DataStructureRule)} {rule.Id} is null"
                );
            }
            if (rule.ValueRule.IsPathRelative)
            {
                if (data == null)
                {
                    throw new NullReferenceException(
                        message: "Rule has IsPathRelative set but no XmlDataDocument has been provided. Cannot evaluate rule."
                    );
                }
                // HERE WE HAVE TO USE THE SLICE, BECAUSE IF WE USED THE ORIGINA XML DOCUMENT (E.G. FROM THE FORM)
                // WE WOULD NOT GET THE ACTUAL VALUES, SINCE THEY WERE NOT COMMITED TO THE XML, YET
                // if the xml propagation would work, we would use
                //XPathNavigator nav = data.GetElementFromRow(rowChanged).CreateNavigator();
                //iterator = nav.Select(".");
                //xmlSlice = data;
                // get the path to the current row
                // DOES NOT WORK FOR NEW ROWS NOT APPENDED TO THE DATATABLE
                //					XPathNavigator nav = data.GetElementFromRow(rowChanged).CreateNavigator();
                //					string path = nav.Name;
                //
                //					while(nav.MoveToParent())
                //					{
                //						path = nav.Name + "/" + path;
                //					}
                string path = rowChanged.Table.TableName;
                DataTable t = rowChanged.Table;
                while (t.ParentRelations.Count > 0)
                {
                    if (t.ParentRelations[index: 0].Nested)
                    {
                        t = t.ParentRelations[index: 0].ParentTable;
                        path = t.TableName + "/" + path;
                    }
                    else
                    {
                        break;
                    }
                }
                path = "/" + data.DataSet.DataSetName + "/" + path;

                // move to the same position in the xml slice
                XPathNavigator nav = xmlSlice.Xml.CreateNavigator();
                iterator = nav.Select(xpath: path);
                iterator.MoveNext();
            }
            // if exists, check condition, if the rule will be actually evaluated
            if (rule.ConditionRule != null)
            {
                if (rule.ConditionRule.IsPathRelative != rule.ValueRule.IsPathRelative)
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "IsPathRelative",
                        actualValue: rule.ConditionRule.IsPathRelative,
                        message: ResourceUtils.GetString(
                            key: "ErrorRuleConditionEqual",
                            args: rule.Path
                        )
                    );
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        message: "   " + ResourceUtils.GetString(key: "PadEvaluatingCondition")
                    );
                }
                object shouldEvaluate = this.EvaluateRule(
                    rule: rule.ConditionRule,
                    data: xmlSlice,
                    contextPosition: iterator == null ? null : iterator.Clone()
                );
                if (shouldEvaluate is bool)
                {
                    if (!(bool)shouldEvaluate)
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(
                                message: "   " + ResourceUtils.GetString(key: "PadConditionFalse")
                            );
                        }
                        goto nextRule;
                    }
                }
                else
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorNotBool", args: rule.Path)
                    );
                }
            }

            //				if(outputPad != null)
            //				{
            //					outputPad.AddText("Rule source data:");
            //					outputPad.AddText(dataSlice.GetXml());
            //				}
            object result;
            try
            {
                result = this.EvaluateRule(
                    rule: rule.ValueRule,
                    data: xmlSlice,
                    contextPosition: iterator
                );
            }
            catch
            {
                throw;
            }
            #region Processing Result
            #region TRACE
            if (log.IsDebugEnabled)
            {
                log.RunHandled(loggingAction: () =>
                {
                    if (rule.TargetField != null)
                    {
                        string columnName = rule.TargetField.Name;
                        DataColumn col = rowChanged.Table.Columns[name: columnName];
                        object oldValue = rowChanged[column: col];
                        string newLookupValue = null;
                        string oldLookupValue = null;
                        if (col.ExtendedProperties.Contains(key: Const.DefaultLookupIdAttribute))
                        {
                            if (result != DBNull.Value && !(result is XmlDocument))
                            {
                                try
                                {
                                    newLookupValue = LookupValue(
                                        lookupId: col.ExtendedProperties[
                                                key: Const.DefaultLookupIdAttribute
                                            ]
                                            .ToString(),
                                        recordId: result.ToString()
                                    );
                                }
                                catch (Exception ex)
                                {
                                    newLookupValue = ex.Message;
                                }
                            }
                            if (oldValue != DBNull.Value)
                            {
                                try
                                {
                                    oldLookupValue = LookupValue(
                                        lookupId: col.ExtendedProperties[
                                                key: Const.DefaultLookupIdAttribute
                                            ]
                                            .ToString(),
                                        recordId: oldValue.ToString()
                                    );
                                }
                                catch (Exception ex)
                                {
                                    oldLookupValue = ex.Message;
                                }
                            }
                        }
                        log.Debug(
                            message: "   "
                                + ResourceUtils.GetString(key: "PadRuleResult0")
                                + result.ToString()
                                + (newLookupValue == null ? "" : " (" + newLookupValue + ")")
                                + ResourceUtils.GetString(key: "PadRuleResult1")
                                + columnName
                                + ResourceUtils.GetString(key: "PadRuleResult2")
                                + oldValue.ToString()
                                + (oldLookupValue == null ? "" : " (" + oldLookupValue + ")")
                        );
                    }
                    else
                    {
                        log.Debug(
                            message: "   "
                                + ResourceUtils.GetString(key: "PadRuleResult0")
                                + result.ToString()
                        );
                    }
                });
            }
            #endregion
            if (result is IDataDocument)
            {
                // RESULT IS DATASET
                DataTable resultTable = (result as IDataDocument).DataSet.Tables[
                    name: rowChanged.Table.TableName
                ];
                if (resultTable == null)
                {
                    string message = ResourceUtils.GetString(
                        key: "PadRuleInvalidStructure",
                        args: rowChanged.Table.TableName
                    );
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(message: message);
                    }
                    throw new Exception(message: message);
                }
                // find the record in the transformed document
                DataRow resultRow = resultTable.Rows.Find(
                    keys: DatasetTools.PrimaryKey(row: rowChanged)
                );
                if (resultRow == null)
                {
                    // row was not generated by the rule, this is a problem, the row must always be returned
                    string message = ResourceUtils.GetString(key: "PadRuleInvalidNoData");
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(message: message);
                    }
                    throw new Exception(message: message);
                }

                var changedColumns = new List<DataColumn>();
                var changedTargetColumns = new List<DataColumn>(capacity: changedColumns.Count);
                foreach (DataColumn col in resultRow.Table.Columns)
                {
                    if (
                        rowChanged.Table.Columns.Contains(name: col.ColumnName)
                        && !(
                            resultRow[column: col]
                                .Equals(obj: rowChanged[columnName: col.ColumnName])
                        )
                    )
                    {
                        changedColumns.Add(item: col);
                        changedTargetColumns.Add(
                            item: rowChanged.Table.Columns[name: col.ColumnName]
                        );
                    }
                }
                #region TRACE
                if (log.IsDebugEnabled)
                {
                    log.RunHandled(loggingAction: () =>
                    {
                        foreach (DataColumn col in changedColumns)
                        {
                            string newLookupValue = null;
                            string oldLookupValue = null;
                            object resultValue = resultRow[column: col];
                            object oldValue = rowChanged[columnName: col.ColumnName];
                            string columnName = col.ColumnName;
                            if (
                                col.ExtendedProperties.Contains(key: Const.DefaultLookupIdAttribute)
                                && col.ExtendedProperties.Contains(key: Const.OrigamDataType)
                                && !OrigamDataType.Array.Equals(
                                    obj: col.ExtendedProperties[key: Const.OrigamDataType]
                                )
                            )
                            {
                                if (resultValue != DBNull.Value)
                                {
                                    newLookupValue = LookupValue(
                                        lookupId: col.ExtendedProperties[
                                                key: Const.DefaultLookupIdAttribute
                                            ]
                                            .ToString(),
                                        recordId: resultValue.ToString()
                                    );
                                }
                                if (oldValue != DBNull.Value)
                                {
                                    oldLookupValue = LookupValue(
                                        lookupId: col.ExtendedProperties[
                                                key: Const.DefaultLookupIdAttribute
                                            ]
                                            .ToString(),
                                        recordId: oldValue.ToString()
                                    );
                                }
                            }
                            log.Debug(
                                message: "   "
                                    + columnName
                                    + ": "
                                    + resultValue.ToString()
                                    + (newLookupValue == null ? "" : " (" + newLookupValue + ")")
                                    + ResourceUtils.GetString(key: "PadRuleResult1")
                                    + ResourceUtils.GetString(key: "PadRuleResult2")
                                    + oldValue.ToString()
                                    + (oldLookupValue == null ? "" : " (" + oldLookupValue + ")")
                            );
                        }
                    });
                }
                #endregion
                // copy the values into the source row
                PauseRuleProcessing();
                bool localChanged = DatasetTools.CopyRecordValues(
                    sourceRow: resultRow,
                    sourceVersion: DataRowVersion.Current,
                    destinationRow: rowChanged,
                    enforceNullValues: true
                );
                ResumeRuleProcessing();
                if (!changed)
                {
                    changed = localChanged;
                }

                ProcessRules(
                    rowChanged: rowChanged,
                    data: data,
                    columnsChanged: changedTargetColumns,
                    ruleSet: ruleSet
                );
            }
            else if (result is XmlDocument)
            {
                // XML IS NOT SUPPORTED
                string message = ResourceUtils.GetString(key: "PadXmlDocument");
                if (rule.ValueRule != null && rule.ValueRule is Origam.Schema.RuleModel.XslRule)
                {
                    message += ResourceUtils.GetString(
                        key: "FixXslRuleWithDestinationDataStructure",
                        args: rule.ValueRule.ToString()
                    );
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug(message: message);
                }
                throw new NotSupportedException(message: message);
            }
            else
            {
                // SIMPLE DATA TYE (e.g. XPath Rule). TargetField must be used to return the result to a specific column.
                if (rule.TargetField == null)
                {
                    string message = ResourceUtils.GetString(key: "PadTargetField");
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(message: message);
                    }
                    throw new Exception(message: message);
                }
                foreach (DataColumn column in rowChanged.Table.Columns)
                {
                    if (
                        column
                            .ExtendedProperties[key: "Id"]
                            .Equals(obj: rule.TargetField.PrimaryKey[key: "Id"])
                    )
                    {
                        if (!rowChanged[column: column].Equals(obj: result))
                        {
                            rowChanged[column: column] = result;
                            changed = true;
                        }
                        break;
                    }
                }
            }
            #endregion
            nextRule:
            ;
        }
        if (log.IsDebugEnabled && myRules.Count > 0)
        {
            log.Debug(
                message: ResourceUtils.GetString(
                    key: "PadRuleFinished",
                    args: [DateTime.Now, changed.ToString()]
                )
            );
        }
        return changed;
    }
    #endregion
    #region Conditional Formatting Functions
    public EntityFormatting Formatting(
        XmlContainer data,
        Guid entityId,
        Guid fieldId,
        XPathNodeIterator contextPosition
    )
    {
        EntityFormatting formatting = new EntityFormatting(
            foreColor: NullColor,
            backColor: NullColor
        );
        var entityRules = new List<EntityConditionalFormatting>();
        IDataEntity entity =
            _persistence.SchemaProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(id: entityId)
            ) as IDataEntity;

        if (fieldId == Guid.Empty)
        {
            entityRules.AddRange(collection: entity.ConditionalFormattingRules);
        }
        else
        {
            // we retrieve the column from the child-items list
            // this is very cost efficient, because when retrieving abstract columns (i.e. Id, RecordCreated, RecordUpdated), they are never cached
            IDataEntityColumn field = entity.GetChildById(id: fieldId) as IDataEntityColumn;
            if (field != null)
            {
                entityRules.AddRange(collection: field.ConditionalFormattingRules);
            }
        }
        if (entityRules.Count > 0)
        {
            entityRules.Sort();

            foreach (EntityConditionalFormatting rule in entityRules)
            {
                if (
                    IsRuleMatching(
                        data: data,
                        rule: rule.Rule,
                        roles: rule.Roles,
                        contextPosition: contextPosition
                    )
                )
                {
                    Color foreColor = rule.ForegroundColor;
                    Color backColor = rule.BackgroundColor;
                    object lookupParam = null;
                    if (rule.DynamicColorLookupField != null)
                    {
                        EntityRule xpr = new EntityRule();
                        xpr.DataType = OrigamDataType.String;
                        xpr.XPath =
                            "/row/"
                            + (
                                rule.DynamicColorLookupField.XmlMappingType
                                == EntityColumnXmlMapping.Attribute
                                    ? "@"
                                    : ""
                            )
                            + rule.DynamicColorLookupField.Name;
                        lookupParam = EvaluateRule(
                            rule: xpr,
                            context: data,
                            contextPosition: contextPosition
                        );
                    }
                    if (lookupParam != DBNull.Value)
                    {
                        if (rule.ForegroundColorLookup != null)
                        {
                            if (rule.DynamicColorLookupField == null)
                            {
                                throw new Exception(
                                    message: ResourceUtils.GetString(
                                        key: "ErrorNoForegroundDynamicColorLookup"
                                    )
                                );
                            }

                            object color = _lookupService.GetDisplayText(
                                lookupId: rule.ForeColorLookupId,
                                lookupValue: lookupParam,
                                useCache: false,
                                returnMessageIfNull: false,
                                transactionId: null
                            );

                            if (color is int)
                            {
                                foreColor = System.Drawing.Color.FromArgb(argb: (int)color);
                            }
                        }
                        if (rule.BackgroundColorLookup != null)
                        {
                            if (rule.DynamicColorLookupField == null)
                            {
                                throw new Exception(
                                    message: ResourceUtils.GetString(
                                        key: "ErrorNoBackgroundDynamicColorLookup"
                                    )
                                );
                            }

                            object color = _lookupService.GetDisplayText(
                                lookupId: rule.BackColorLookupId,
                                lookupValue: lookupParam,
                                useCache: false,
                                returnMessageIfNull: false,
                                transactionId: null
                            );

                            if (color is int)
                            {
                                backColor = System.Drawing.Color.FromArgb(argb: (int)color);
                            }
                        }
                    }
                    if (foreColor != NullColor && formatting.ForeColor == NullColor)
                    {
                        formatting.ForeColor = foreColor;
                    }
                    if (backColor != NullColor && formatting.BackColor == NullColor)
                    {
                        formatting.BackColor = backColor;
                    }
                    if (formatting.ForeColor != NullColor && formatting.BackColor != NullColor)
                    {
                        return formatting;
                    }
                }
            }
        }
        return formatting;
    }

    public string DynamicLabel(
        XmlContainer data,
        Guid entityId,
        Guid fieldId,
        XPathNodeIterator contextPosition
    )
    {
        var rules = new List<EntityFieldDynamicLabel>();

        IDataEntity entity =
            _persistence.SchemaProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(id: entityId)
            ) as IDataEntity;
        IDataEntityColumn field = entity.GetChildById(id: fieldId) as IDataEntityColumn;
        if (field == null)
        {
            return null; // lookup fields in a data structure
        }

        rules.AddRange(collection: field.DynamicLabels);
        if (rules.Count > 0)
        {
            rules.Sort();
            foreach (EntityFieldDynamicLabel rule in rules)
            {
                if (
                    IsRuleMatching(
                        data: data,
                        rule: rule.Rule,
                        roles: rule.Roles,
                        contextPosition: contextPosition
                    )
                )
                {
                    string result = (string)
                        _parameterService.GetParameterValue(
                            id: rule.LabelConstantId,
                            targetType: OrigamDataType.String
                        );
                    return result;
                }
            }
        }
        return null;
    }
    #endregion
    #region Row Level Security Functions
    public bool EvaluateRowLevelSecurityState(DataRow row, string field, CredentialType type)
    {
        if (!DatasetTools.HasRowValidParent(row: row))
        {
            return true;
        }

        Guid entityId = Guid.Empty;
        Guid fieldId = Guid.Empty;
        if (row.Table.ExtendedProperties.Contains(key: "EntityId"))
        {
            XmlContainer originalData = DatasetTools.GetRowXml(
                row: row,
                version: DataRowVersion.Original
            );
            XmlContainer actualData = DatasetTools.GetRowXml(
                row: row,
                version: row.HasVersion(version: DataRowVersion.Proposed)
                    ? DataRowVersion.Proposed
                    : DataRowVersion.Default
            );
            fieldId = (Guid)row.Table.Columns[name: field].ExtendedProperties[key: "Id"];
            entityId = (Guid)row.Table.ExtendedProperties[key: "EntityId"];
            return EvaluateRowLevelSecurityState(
                originalData: originalData,
                actualData: actualData,
                field: field,
                type: type,
                entityId: entityId,
                fieldId: fieldId,
                isNewRow: row.RowState == DataRowState.Added
                    || row.RowState == DataRowState.Detached
            );
        }

        return true;
    }

    public List<string> GetDisabledActions(
        XmlContainer originalData,
        XmlContainer actualData,
        Guid entityId,
        Guid formId
    )
    {
        var result = new List<string>();
        IDataEntity entity = _persistence.SchemaProvider.RetrieveInstance<IDataEntity>(
            instanceId: entityId
        );
        foreach (
            EntityUIAction action in entity.ChildItemsByTypeRecursive(
                itemType: EntityUIAction.CategoryConst
            )
        )
        {
            // Performance sensitive! RuleDisablesAction method should not
            // be invoked unless it is really necessary.
            if (
                IsFeatureOff(action: action)
                || IsDisabledByMode(actualData: actualData, action: action)
                || IsDisabledByScreenCondition(formId: formId, action: action)
                || IsDisabledByScreenSectionCondition(formId: formId, action: action)
                || IsDisabledByRoles(action: action)
                || RuleDisablesAction(
                    originalData: originalData,
                    actualData: actualData,
                    action: action
                )
            )
            {
                result.Add(item: action.Id.ToString());
            }
        }
        return result;
    }

    private bool IsDisabledByRoles(EntityUIAction action)
    {
        if (action.Roles != null & action.Roles != String.Empty)
        {
            return false;
        }
        return !_authorizationProvider.Authorize(
            principal: SecurityManager.CurrentPrincipal,
            context: action.Roles
        );
    }

    private bool IsDisabledByScreenSectionCondition(Guid formId, EntityUIAction action)
    {
        if (formId == Guid.Empty)
        {
            return false;
        }
        var panelIds = _persistence
            .SchemaProvider.RetrieveInstance<FormControlSet>(instanceId: formId)
            .ChildrenRecursive.OfType<ControlSetItem>()
            .Select(selector: controlSet => controlSet.ControlItem.PanelControlSetId)
            .Where(predicate: panelId => panelId != Guid.Empty)
            .ToList();
        return action.ScreenSectionIds.Any()
            && panelIds.Count > 0
            && !panelIds.Any(predicate: panelId =>
                action.ScreenSectionIds.Contains(value: panelId)
            );
    }

    private static bool IsDisabledByScreenCondition(Guid formId, EntityUIAction action)
    {
        return formId != Guid.Empty
            && action.ScreenIds.Any()
            && !action.ScreenIds.Contains(value: formId);
    }

    private static bool IsDisabledByMode(XmlContainer actualData, EntityUIAction action)
    {
        return action.Mode == PanelActionMode.ActiveRecord && actualData == null;
    }

    private bool IsFeatureOff(EntityUIAction action)
    {
        return !_parameterService.IsFeatureOn(featureCode: action.Features);
    }

    // Performance sensitive! RuleDisablesAction method should not
    // be invoked unless it is really necessary.
    private bool RuleDisablesAction(
        XmlContainer originalData,
        XmlContainer actualData,
        EntityUIAction action
    )
    {
        XmlContainer dataToUseForRule =
            action.ValueType == CredentialValueType.ActualValue ? actualData : originalData;
        return action.Rule != null
            && !IsRuleMatching(
                data: dataToUseForRule,
                rule: action.Rule,
                roles: action.Roles,
                contextPosition: null
            );
    }

    public bool EvaluateRowLevelSecurityState(
        XmlContainer originalData,
        XmlContainer actualData,
        string field,
        CredentialType type,
        Guid entityId,
        Guid fieldId,
        bool isNewRow,
        RuleEvaluationCache ruleEvaluationCache = null
    )
    {
        var rules = new List<AbstractEntitySecurityRule>();

        IDataEntity entity =
            _persistence.SchemaProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(id: entityId)
            ) as IDataEntity;
        // field-level rules
        IDataEntityColumn column = null;
        if (field != null)
        {
            // we retrieve the column from the child-items list
            // this is very cost efficient, because when retrieving
            // abstract columns (i.e. Id, RecordCreated, RecordUpdated), they are never cached
            column = entity.GetChildById(id: fieldId) as IDataEntityColumn;
            // field not found, this would be e.g. a looked up column,
            // which does not point to a real entity field id
            if (column != null)
            {
                if (!column.RowLevelSecurityRules.Any())
                {
                    // shortcircuit processing of row level security rules
                    // for a column without it's own rules
                    Boolean? result = ruleEvaluationCache?.GetRulelessFieldResult(
                        entityId: entityId,
                        type: type
                    );
                    if (result != null)
                    {
                        return result.Value;
                    }
                }
                else
                {
                    rules.AddRange(collection: column.RowLevelSecurityRules);
                }
            }
        }
        // entity-level rules
        List<AbstractEntitySecurityRule> entityRules = entity.RowLevelSecurityRules;
        if (entityRules.Count > 0)
        {
            rules.AddRange(collection: entityRules);
        }
        // no rules - permit
        if (rules.Count == 0)
        {
            return true;
        }
        rules.Sort();
        foreach (AbstractEntitySecurityRule rule in rules)
        {
            EntitySecurityRule entityRule = rule as EntitySecurityRule;
            if (entityRule != null)
            {
                if (entityRule.DeleteCredential && type == CredentialType.Delete && isNewRow)
                {
                    // always allow to delete new (not saved) records
                    return PutToRulelessCache(
                        type: type,
                        entityId: entityId,
                        ruleEvaluationCache: ruleEvaluationCache,
                        column: column,
                        value: true
                    );
                }

                if (
                    (entityRule.UpdateCredential && type == CredentialType.Update)
                    || (entityRule.CreateCredential && type == CredentialType.Update && isNewRow)
                    || (entityRule.CreateCredential && type == CredentialType.Create)
                    || (entityRule.DeleteCredential && type == CredentialType.Delete)
                )
                {
                    bool? result = ruleEvaluationCache?.Get(rule: entityRule, entityId: entityId);
                    if (result == null)
                    {
                        result = IsRowLevelSecurityRuleMatching(
                            rule: entityRule,
                            data: entityRule.ValueType == CredentialValueType.ActualValue
                                ? actualData
                                : originalData
                        );
                        ruleEvaluationCache?.Put(
                            rule: entityRule,
                            entityId: entityId,
                            value: result.Value
                        );
                    }
                    if (result.Value)
                    {
                        return PutToRulelessCache(
                            type: type,
                            entityId: entityId,
                            ruleEvaluationCache: ruleEvaluationCache,
                            column: column,
                            value: entityRule.Type == PermissionType.Permit
                        );
                    }
                }
            }
            EntityFieldSecurityRule fieldRule = rule as EntityFieldSecurityRule;
            if (fieldRule != null)
            {
                if (
                    (fieldRule.UpdateCredential & type == CredentialType.Update)
                    | (fieldRule.ReadCredential & type == CredentialType.Read)
                )
                {
                    Boolean? result = ruleEvaluationCache?.Get(rule: fieldRule, entityId: entityId);
                    if (result == null)
                    {
                        result = IsRowLevelSecurityRuleMatching(
                            rule: fieldRule,
                            data: fieldRule.ValueType == CredentialValueType.ActualValue
                                ? actualData
                                : originalData
                        );
                        ruleEvaluationCache?.Put(
                            rule: fieldRule,
                            entityId: entityId,
                            value: result.Value
                        );
                    }
                    if (result.Value)
                    {
                        return PutToRulelessCache(
                            type: type,
                            entityId: entityId,
                            ruleEvaluationCache: ruleEvaluationCache,
                            column: column,
                            value: fieldRule.Type == PermissionType.Permit
                        );
                    }
                }
            }
        }
        // no match
        if (type == CredentialType.Read)
        {
            return PutToRulelessCache(
                type: type,
                entityId: entityId,
                ruleEvaluationCache: ruleEvaluationCache,
                column: column,
                value: true
            );
        }

        return PutToRulelessCache(
            type: type,
            entityId: entityId,
            ruleEvaluationCache: ruleEvaluationCache,
            column: column,
            value: false
        );
    }

    private static bool PutToRulelessCache(
        CredentialType type,
        Guid entityId,
        RuleEvaluationCache ruleEvaluationCache,
        IDataEntityColumn column,
        bool value
    )
    {
        if (column?.RowLevelSecurityRules.Count == 0 && ruleEvaluationCache != null)
        {
            ruleEvaluationCache.PutRulelessFieldResult(
                entityId: entityId,
                type: type,
                value: value
            );
        }
        return value;
    }

    private bool IsRowLevelSecurityRuleMatching(AbstractEntitySecurityRule rule, XmlContainer data)
    {
        return IsRuleMatching(
            data: data,
            rule: rule.Rule,
            roles: rule.Roles,
            contextPosition: null
        );
    }

    private bool IsRuleMatching(
        XmlContainer data,
        IRule rule,
        string roles,
        XPathNodeIterator contextPosition
    )
    {
        // check roles
        if (
            !_authorizationProvider.Authorize(
                principal: SecurityManager.CurrentPrincipal,
                context: roles
            )
        )
        {
            return false;
        }
        // check business rule
        if (rule != null)
        {
            object result = this.EvaluateRule(
                rule: rule,
                data: data,
                contextPosition: contextPosition
            );
            if (result is bool)
            {
                return (bool)result;
            }

            throw new ArgumentException(
                message: "Rule resulted in a result which is not boolean. Cannot evaluate non-boolean rules. Rule: "
                    + ((ISchemaItem)rule).Path
            );
        }
        return true;
    }

    public bool IsExportAllowed(Guid entityId)
    {
        var rules = new List<AbstractEntitySecurityRule>();
        var entity = _persistence.SchemaProvider.RetrieveInstance<IDataEntity>(
            instanceId: entityId
        );
        List<AbstractEntitySecurityRule> entityRules = entity.RowLevelSecurityRules;
        if (entityRules.Count > 0)
        {
            rules.AddRange(collection: entityRules);
        }
        // no rules - permit
        if (rules.Count == 0)
        {
            return true;
        }
        rules.Sort();
        foreach (AbstractEntitySecurityRule rule in rules)
        {
            if (rule is not EntitySecurityRule entityRule)
            {
                continue;
            }
            if (!entityRule.ExportCredential)
            {
                continue;
            }
            if (IsRowLevelSecurityRuleMatching(rule: entityRule, data: null))
            {
                return entityRule.Type == PermissionType.Permit;
            }
        }
        return true;
    }
    #endregion
    #endregion
    #region Private Functions

    public object GetContext(Key key)
    {
        return _contextStores[key: key];
    }

    public void SetContext(Key key, object value)
    {
        _contextStores[key: key] = value;
    }

    public object GetContext(IContextStore contextStore)
    {
        return GetContext(key: contextStore.PrimaryKey);
    }

    public ICollection ContextStoreKeys
    {
        get { return _contextStores.Keys; }
    }

    public IXmlContainer GetXmlDocumentFromData(object inputData)
    {
        IXmlContainer doc = inputData as XmlContainer;
        if (doc != null)
        {
            return doc;
        }
        object data = inputData;
        IContextStore contextStore = data as IContextStore;
        if (contextStore != null)
        {
            // Get the rule's context store
            data = GetContext(contextStore: contextStore);
        }
        IXmlContainer xmlDocument = data as IXmlContainer;
        if (xmlDocument != null)
        {
            doc = xmlDocument;
            return doc;
        }
        System.Xml.XmlDocument xmlDoc = data as XmlDocument;
        if (xmlDoc != null)
        {
            // this shouldn't happen. XmlContainer should be as and input all the time.
            // But if it was XmlDocument, we convert it here and log it.
            log.ErrorFormat(
                format: "GetXmlDocumentFromData called with System.Xml.XmlDataDocuement."
                    + "This isn't expected. Refactor code to be called with IXmlContainer. (documentElement:{0})",
                arg0: xmlDoc.DocumentElement.Name
            );
            return new XmlContainer(xmlDocument: xmlDoc);
        }
        if (data is int)
        {
            data = XmlConvert.ToString(value: (int)data);
        }
        else if (data is Guid)
        {
            data = XmlConvert.ToString(value: (Guid)data);
        }
        else if (data is long)
        {
            data = XmlConvert.ToString(value: (long)data);
        }
        else if (data is decimal)
        {
            data = XmlConvert.ToString(value: (decimal)data);
        }
        else if (data is bool)
        {
            data = XmlConvert.ToString(value: (bool)data);
        }
        else if (data is DateTime)
        {
            data = XmlConvert.ToString(
                value: (DateTime)data,
                dateTimeOption: XmlDateTimeSerializationMode.RoundtripKind
            );
        }
        else if (data == null)
        {
            return new XmlContainer(xmlString: "<ROOT/>");
        }
        else if (data is IList)
        {
            doc = new XmlContainer();
            XmlElement root = (XmlElement)
                doc.Xml.AppendChild(newChild: doc.Xml.CreateElement(name: "ROOT"));
            foreach (object item in data as IList)
            {
                root.AppendChild(newChild: doc.Xml.CreateElement(name: "value")).InnerText =
                    item.ToString();
            }
            return doc;
        }
        else
        {
            data = data.ToString();
        }
        doc = new XmlContainer();
        doc.Xml.LoadXml(xml: "<ROOT><value /></ROOT>");
        doc.Xml.FirstChild.FirstChild.InnerText = (string)data;
        return doc;
    }
    #endregion
    #region Evaluators
    private object Evaluate(SystemFunctionCall functionCall)
    {
        switch (functionCall.Function)
        {
            case SystemFunction.ActiveProfileId:
            {
                return this.ActiveProfileId();
            }
            case SystemFunction.ResourceIdByActiveProfile:
            {
                return resourceTools.ResourceIdByActiveProfile();
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "Function",
                    actualValue: functionCall.Function,
                    message: ResourceUtils.GetString(key: "ErrorUnsupportedFunction")
                );
            }
        }
    }

    private object Evaluate(DataStructureReference reference)
    {
        return reference;
    }

    private Guid Evaluate(TransformationReference reference)
    {
        return ((XslTransformation)reference.Transformation).Id;
    }
    #endregion
    #region Rule Evaluators
    private object EvaluateRule(
        XPathRule rule,
        IXmlContainer context,
        XPathNodeIterator contextPosition
    )
    {
        XmlDocument xmlDocument = context?.Xml;
        if (xmlDocument == null)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(key: "ErrorEvaluateContextNull")
            );
        }
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Evaluating XPath Rule: " + rule?.Name);
            if (contextPosition != null)
            {
                log.Debug(message: "Current Position: " + contextPosition?.Current?.Name);
            }
            log.Debug(message: "  Input data: " + xmlDocument.OuterXml);
        }
        XPathNavigator nav = xmlDocument.CreateNavigator();
        using (
            MiniProfiler.Current.CustomTiming(
                category: "rule",
                commandString: rule.Name,
                executeType: "XPathRuleEvaluation"
            )
        )
        {
            return XpathEvaluator.Instance.Evaluate(
                xpath: rule.XPath,
                isPathRelative: rule.IsPathRelative,
                returnDataType: rule.DataType,
                nav: nav,
                contextPosition: contextPosition,
                transactionId: _transactionId
            );
        }
    }

    private object EvaluateRule(XslRule rule, IXmlContainer context)
    {
        try
        {
            using (
                MiniProfiler.Current.CustomTiming(
                    category: "rule",
                    commandString: rule.Name,
                    executeType: "XslRuleEvaluation"
                )
            )
            {
                IXmlContainer result = _transformer.Transform(
                    data: context,
                    transformationId: rule.Id,
                    parameters: null,
                    transactionId: _transactionId,
                    outputStructure: rule.Structure,
                    validateOnly: false
                );
                return result;
            }
        }
        catch (OrigamRuleException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception(
                message: ResourceUtils.GetString(key: "ErrorRuleFailed2"),
                innerException: ex
            );
        }
    }
    #endregion
    private void table_RowChanged(object sender, DataRowChangeEventArgs e)
    {
        ProcessRules(rowChanged: e.Row, data: _currentRuleDocument, ruleSet: _ruleSet);
    }

    private void table_ColumnChanged(object sender, DataColumnChangeEventArgs e)
    {
        ProcessRules(
            rowChanged: e.Row,
            data: _currentRuleDocument,
            columnChanged: e.Column,
            ruleSet: _ruleSet
        );
    }
}

#region XPath Helper Classes

public class XPathNodeList : XmlNodeList
{
    // Methods
    static XPathNodeList()
    {
        XPathNodeList.nullparams = new object[0];
    }

    public XPathNodeList(XPathNodeIterator iterator)
    {
        this.iterator = iterator;
        this.list = new List<XmlNode>();
        this.done = false;
    }

    public override IEnumerator GetEnumerator()
    {
        return new XmlNodeListEnumerator(list: this);
    }

    private XmlNode GetNode(XPathNavigator n)
    {
        IHasXmlNode node1 = (IHasXmlNode)n;
        return node1.GetNode();
    }

    public override XmlNode Item(int index)
    {
        if (index >= this.list.Count)
        {
            this.ReadUntil(index: index);
        }
        if ((index < this.list.Count) && (index >= 0))
        {
            return (XmlNode)this.list[index: index];
        }
        return null;
    }

    internal int ReadUntil(int index)
    {
        int num1 = this.list.Count;
        while (!this.done && (index >= num1))
        {
            if (this.iterator.MoveNext())
            {
                XmlNode node1 = this.GetNode(n: this.iterator.Current);
                if (node1 != null)
                {
                    this.list.Add(item: node1);
                    num1++;
                }
            }
            else
            {
                this.done = true;
                return num1;
            }
        }
        return num1;
    }

    // Properties
    public override int Count
    {
        get
        {
            if (!this.done)
            {
                this.ReadUntil(index: 0x7fffffff);
            }
            return this.list.Count;
        }
    }

    // Fields
    private bool done;
    private XPathNodeIterator iterator;
    private List<XmlNode> list;
    private static readonly object[] nullparams;
}

internal class XmlNodeListEnumerator : IEnumerator
{
    // Methods
    public XmlNodeListEnumerator(XPathNodeList list)
    {
        this.list = list;
        this.index = -1;
        this.valid = false;
    }

    public bool MoveNext()
    {
        this.index++;
        int num1 = this.list.ReadUntil(index: this.index + 1);
        if (this.index > (num1 - 1))
        {
            return false;
        }
        this.valid = this.list[i: this.index] != null;
        return this.valid;
    }

    public void Reset()
    {
        this.index = -1;
    }

    // Properties
    public object Current
    {
        get
        {
            if (this.valid)
            {
                return this.list[i: this.index];
            }
            return null;
        }
    }

    // Fields
    private int index;
    private XPathNodeList list;
    private bool valid;
}
#endregion
#region IComparer Members
public class ProcessRuleComparer : IComparer<DataStructureRule>
{
    public int Compare(DataStructureRule x, DataStructureRule y)
    {
        if (x != null && y != null)
        {
            return x.Priority.CompareTo(value: y.Priority);
        }
        // rulesets are always an top, so rules are greater
        return 1;
    }
}
#endregion
