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
using System.Data.Common;
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
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private static System.Xml.Serialization.XmlSerializer _ruleExceptionSerializer =
        new System.Xml.Serialization.XmlSerializer(
            typeof(RuleExceptionDataCollection),
            new System.Xml.Serialization.XmlRootAttribute("RuleExceptionDataCollection")
        );

    private Color NullColor = Color.FromArgb(0, 0, 0, 0);
    IXsltEngine _transformer;
    private IPersistenceService _persistence;
    private IDataLookupService _lookupService;
    private IParameterService _parameterService;
    private ITracingService _tracingService;
    private IDocumentationService _documentationService;
    private IOrigamAuthorizationProvider _authorizationProvider;
    private Func<UserProfile> _userProfileGetter;
    private readonly ResourceTools resourceTools = new(
        ServiceManager.Services.GetService<IBusinessServicesService>(),
        SecurityManager.CurrentUserProfile
    );

    public static RuleEngine Create(
        Hashtable contextStores,
        string transactionId,
        Guid tracingWorkflowId
    )
    {
        return new RuleEngine(
            contextStores,
            transactionId,
            tracingWorkflowId,
            ServiceManager.Services.GetService<IPersistenceService>(),
            ServiceManager.Services.GetService<IDataLookupService>(),
            ServiceManager.Services.GetService<IParameterService>(),
            ServiceManager.Services.GetService<ITracingService>(),
            ServiceManager.Services.GetService<IDocumentationService>(),
            SecurityManager.GetAuthorizationProvider(),
            SecurityManager.CurrentUserProfile
        );
    }

    public static RuleEngine Create()
    {
        return Create(new Hashtable(), null);
    }

    public static RuleEngine Create(Hashtable contextStores, string transactionId)
    {
        return new RuleEngine(
            contextStores,
            transactionId,
            ServiceManager.Services.GetService<IPersistenceService>(),
            ServiceManager.Services.GetService<IDataLookupService>(),
            ServiceManager.Services.GetService<IParameterService>(),
            ServiceManager.Services.GetService<ITracingService>(),
            ServiceManager.Services.GetService<IDocumentationService>(),
            SecurityManager.GetAuthorizationProvider(),
            SecurityManager.CurrentUserProfile
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
            contextStores,
            transactionId,
            persistence,
            lookupService,
            parameterService,
            tracingService,
            documentationService,
            authorizationProvider,
            userProfileGetter
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
            throw new InvalidOperationException(ResourceUtils.GetString("ErrorInitializeEngine"));
        }
        _transformer = new CompiledXsltEngine(_persistence.SchemaProvider);
    }

    #region Properties
    public static string ValidationNotMetMessage()
    {
        return ResourceUtils.GetString("ErrorOutputRuleFailed");
    }

    public static string ValidationContinueMessage(string message)
    {
        return ResourceUtils.GetString("DoYouWishContinue", message);
    }

    public static string ValidationWarningMessage()
    {
        return ResourceUtils.GetString("Warning");
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
                        contextValue = XmlConvert.ToInt32(inputString);
                        break;
                    }

                    case OrigamDataType.Long:
                    {
                        contextValue = XmlConvert.ToInt64(inputString);
                        break;
                    }

                    case OrigamDataType.UniqueIdentifier:
                    {
                        contextValue = XmlConvert.ToGuid(inputString);
                        break;
                    }

                    case OrigamDataType.Currency:
                    case OrigamDataType.Float:
                    {
                        contextValue = XmlConvert.ToDecimal(inputString);
                        break;
                    }

                    case OrigamDataType.Date:
                    {
                        contextValue = XmlConvert.ToDateTime(
                            inputString,
                            XmlDateTimeSerializationMode.RoundtripKind
                        );
                        break;
                    }

                    case OrigamDataType.Boolean:
                    {
                        contextValue = XmlConvert.ToBoolean(inputString);
                        break;
                    }

                    case OrigamDataType.String:
                    case OrigamDataType.Memo:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            "dataType",
                            origamDataType,
                            "Unsupported data type."
                        );
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
            new Guid(lookupId),
            recordId,
            false,
            false,
            this.TransactionId
        );

        return XmlTools.FormatXmlString(result);
    }
    #endregion
    #region Other Functions
    public object EvaluateRule(IRule rule, object data, XPathNodeIterator contextPosition)
    {
        return EvaluateRule(rule, data, contextPosition, false);
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
            IXmlContainer xmlData = GetXmlDocumentFromData(data);
            bool ruleEvaluationDidRun = false;
            object ruleResult = null;
            switch (rule)
            {
                case XPathRule pathRule:
                {
                    ruleResult = EvaluateRule(pathRule, xmlData, contextPosition);
                    ruleEvaluationDidRun = true;
                    break;
                }

                case XslRule xslRule:
                {
                    ruleResult = EvaluateRule(xslRule, xmlData);
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
            string errorMessage = ResourceUtils.GetString("ErrorRuleFailed", rule.Name);
            if (_documentationService != null)
            {
                string doc = _documentationService.GetDocumentation(
                    (Guid)rule.PrimaryKey["Id"],
                    DocumentationType.RULE_EXCEPTION_MESSAGE
                );
                if (doc != "")
                {
                    errorMessage += Environment.NewLine + doc;
                }
            }
            throw new Exception(errorMessage, ex);
        }
    }

    public RuleExceptionDataCollection EvaluateEndRule(IEndRule rule, object data)
    {
        return EvaluateEndRule(rule, data, new Hashtable(), false);
    }

    public RuleExceptionDataCollection EvaluateEndRule(
        IEndRule rule,
        object data,
        bool parentIsTracing
    )
    {
        return EvaluateEndRule(rule, data, new Hashtable(), parentIsTracing);
    }

    public RuleExceptionDataCollection EvaluateEndRule(
        IEndRule rule,
        object data,
        Hashtable parameters
    )
    {
        return EvaluateEndRule(rule, data, parameters, false);
    }

    public RuleExceptionDataCollection EvaluateEndRule(
        IEndRule rule,
        object data,
        Hashtable parameters,
        bool parentIsTracing
    )
    {
        IXmlContainer context = GetXmlDocumentFromData(data);
        IXmlContainer result = null;
        try
        {
            if (rule is XslRule)
            {
                XslRule xslRule = rule as XslRule;
                result = _transformer.Transform(
                    context,
                    xslRule.Id,
                    parameters,
                    _transactionId,
                    new XsdDataStructure(),
                    false
                );
            }
            else if (rule is XPathRule)
            {
                string ruleText = (string)this.EvaluateRule(rule, context, null);
                result = _transformer.Transform(
                    context,
                    ruleText,
                    parameters,
                    TransactionId,
                    new XsdDataStructure(),
                    false
                );
            }
            else
            {
                throw new Exception(ResourceUtils.GetString("ErrorOnlyXslRuleSupported"));
            }

            RuleExceptionDataCollection exceptions = DeserializeRuleExceptions(result.Xml);

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
            throw new Exception(ResourceUtils.GetString("ErrorRuleFailed1", rule.Name), ex);
        }
    }

    private RuleExceptionDataCollection DeserializeRuleExceptions(XmlDocument xmlDoc)
    {
        if (xmlDoc == null)
        {
            return null;
        }
        XmlNodeReader reader = new XmlNodeReader(xmlDoc);
        RuleExceptionDataCollection exceptions = null;
        try
        {
            if (reader.ReadToFollowing("RuleExceptionDataCollection"))
            {
                exceptions = (RuleExceptionDataCollection)
                    _ruleExceptionSerializer.Deserialize(reader);
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
            return Evaluate(item as DataStructureReference);
        }
        else if (item is SystemFunctionCall)
        {
            return Evaluate(item as SystemFunctionCall);
        }
        else if (item is TransformationReference)
        {
            return Evaluate(item as TransformationReference);
        }
        else if (item is ReportReference)
        {
            return (item as ReportReference).ReportId;
        }
        else if (item is DataConstantReference)
        {
            return _parameterService.GetParameterValue(
                (item as DataConstantReference).DataConstant.Id
            );
        }
        else if (item is WorkflowReference)
        {
            return (item as WorkflowReference).WorkflowId;
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                "item",
                item,
                ResourceUtils.GetString("ErrorRuleInvalidType")
            );
        }
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
        DatasetTools.BeginLoadData(inout_dsTarget);
        try
        {
            MergeParams mergeParams = new MergeParams();
            mergeParams.TrueDelete = in_bTrueDelete;
            mergeParams.PreserveChanges = in_bPreserveChanges;
            mergeParams.SourceIsFragment = in_bSourceIsFragment;
            mergeParams.PreserveNewRowState = preserveNewRowState;
            mergeParams.ProfileId = _userProfileGetter().Id;
            result = DatasetTools.MergeDataSet(inout_dsTarget, in_dsSource, null, mergeParams);
        }
        finally
        {
            DatasetTools.EndLoadData(inout_dsTarget);
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
            context = GetXmlDocumentFromData(context).Xml;
        }

        if (context is XmlDocument)
        {
            if (dataType == OrigamDataType.Xml && xpath.Trim() == "/")
            {
                return context;
            }
            OrigamXsltContext ctx = OrigamXsltContext.Create(new NameTable(), _transactionId);
            XPathNavigator nav = ((XmlDocument)context).CreateNavigator();
            XPathExpression expr = nav.Compile(xpath);
            expr.SetContext(ctx);

            if (dataType == OrigamDataType.Array)
            {
                object expressionResult = nav.Evaluate(expr);
                result = new ArrayList();
                if (expressionResult is XPathNodeIterator)
                {
                    XPathNodeIterator iterator = expressionResult as XPathNodeIterator;
                    while (iterator.MoveNext())
                    {
                        ((ArrayList)result).Add(iterator.Current.Value);
                    }
                }
            }
            else if (dataType != OrigamDataType.Xml)
            {
                // Result is other than XML

                result = nav.Evaluate(expr);
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
                            throw new InvalidCastException("Only string can be converted to blob.");
                        }
                        result = Convert.FromBase64String((string)result);
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
                                result = new Guid(result.ToString());
                            }
                        }
                        break;
                    }

                    case OrigamDataType.Integer:
                    {
                        if (!(result is Int32) & result != null)
                        {
                            result = Convert.ToInt32(result);
                        }
                        break;
                    }

                    case OrigamDataType.Float:
                    case OrigamDataType.Currency:
                    {
                        if (!(result is Decimal) && result != null)
                        {
                            result = XmlConvert.ToDecimal(result.ToString());
                        }
                        break;
                    }

                    case OrigamDataType.Date:
                    {
                        if (!(result is DateTime) && result != null)
                        {
                            result = XmlConvert.ToDateTime(
                                result.ToString(),
                                XmlDateTimeSerializationMode.RoundtripKind
                            );
                        }
                        break;
                    }
                }
            }
            else
            {
                // result is XML
                XmlNodeList results = new XPathNodeList(nav.Select(expr));
                XmlDocument resultDoc = new XmlDocument();
                XmlNode docElement = resultDoc.ImportNode(
                    ((XmlDocument)context).DocumentElement,
                    false
                );
                resultDoc.AppendChild(docElement);

                foreach (XmlNode node in results)
                {
                    if (node is XmlDocument)
                    {
                        resultDoc = node as XmlDocument;
                    }
                    else
                    {
                        docElement.AppendChild(resultDoc.ImportNode(node, true));
                    }
                }
                if (targetStructure is DataStructure)
                {
                    // we clone the dataset (no data, just the structure)
                    DataSet dataset = new DatasetGenerator(true).CreateDataSet(
                        targetStructure as DataStructure
                    );

                    dataset.EnforceConstraints = false;
                    // we load the iteration data into the dataset
                    try
                    {
                        dataset.ReadXml(new XmlNodeReader(resultDoc), XmlReadMode.IgnoreSchema);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            ResourceUtils.GetString("ErrorEvaluateContextFailed", ex.Message),
                            ex
                        );
                    }
                    // we add the context into the called engine
                    result = DataDocumentFactory.New(dataset);
                }
                else
                {
                    result = new XmlContainer(resultDoc);
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
                "data",
                data,
                ResourceUtils.GetString("ErrorTypeNotProcessable")
            );
        }
        _ruleSet = ruleSet;
        /********************************************************************
         * Column bound rules
         ********************************************************************/
        if (contextRow == null) // whole dataset
        {
            Hashtable cols = new Hashtable();
            CompleteChildColumnReferences(data.DataSet, cols);

            EnqueueAllRows(data, ruleSet, cols);
        }
        else // current row
        {
            Hashtable cols = new Hashtable();
            CompleteChildColumnReferences(contextRow.Table, cols);

            EnqueueAllRows(contextRow, data, ruleSet, cols);
        }
        /********************************************************************
         * Row bound rules
         ********************************************************************/
        List<DataTable> sortedTables = GetSortedTables(data.DataSet);
        try
        {
            foreach (DataTable table in sortedTables)
            {
                RegisterTableEvents(table);
            }
            ProcessRuleQueue();
        }
        finally
        {
            foreach (DataTable table in sortedTables)
            {
                UnregisterTableEvents(table);
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
            if (col.ExtendedProperties.Contains("Id"))
            {
                cols[ColumnKey(col)] = col;
            }
        }
        foreach (DataRelation rel in table.ChildRelations)
        {
            CompleteChildColumnReferences(rel.ChildTable, cols);
        }
    }

    private void CompleteChildColumnReferences(DataSet data, Hashtable cols)
    {
        foreach (DataTable table in data.Tables)
        {
            foreach (DataColumn col in table.Columns)
            {
                if (col.ExtendedProperties.Contains("Id"))
                {
                    cols[ColumnKey(col)] = col;
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
                GetChildTables(table, result);
            }
        }
        return result;
    }

    private void GetChildTables(DataTable table, List<DataTable> list)
    {
        foreach (DataRelation childRelation in table.ChildRelations)
        {
            GetChildTables(childRelation.ChildTable, list);
        }
        list.Add(table);
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
        ProcessRulesInternal(rowChanged, data, columnChanged, ruleSet, null, false);
    }

    internal void ProcessRules(
        DataRow rowChanged,
        IDataDocument data,
        ICollection columnsChanged,
        DataStructureRuleSet ruleSet
    )
    {
        ProcessRulesInternal(rowChanged, data, null, ruleSet, columnsChanged, false);
    }

    private bool ProcessRulesFromQueue(
        DataRow rowChanged,
        IDataDocument data,
        DataStructureRuleSet ruleSet,
        ICollection columnsChanged
    )
    {
        return ProcessRulesInternal(rowChanged, data, null, ruleSet, columnsChanged, true);
    }

    private IndexedRuleQueue _ruleQueue = new();
    private Hashtable _ruleColumnChanges = new Hashtable();

    private bool IsEntryInQueue(DataRow rowChanged, DataStructureRuleSet ruleSet)
    {
        return _ruleQueue.Contains(rowChanged, ruleSet);
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
                !queueEntry[0].Equals(rowChanged)
                && (
                    (queueEntry[1] != null && queueEntry[1].Equals(ruleSet))
                    || (queueEntry[1] == null && ruleSet == null)
                )
            )
            {
                Hashtable h = queueEntry[2] as Hashtable;
                h[ColumnKey(column)] = column;
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
                if (!col.Table.TableName.Equals(rowChanged.Table.TableName))
                {
                    columns.Add(ColumnKey(col), col);
                }
            }
        }
        object[] queueEntry = new object[4] { rowChanged, ruleSet, columns, data };
        _ruleQueue.Enqueue(queueEntry);
    }

    private static string ColumnKey(DataColumn col)
    {
        return col.Table.TableName + "_" + col.ExtendedProperties["Id"].ToString();
    }

    private void EnqueueAllRows(
        DataRow currentRow,
        IDataDocument data,
        DataStructureRuleSet ruleSet,
        Hashtable columns
    )
    {
        if (!IsEntryInQueue(currentRow, ruleSet))
        {
            EnqueueEntry(currentRow, data, ruleSet, columns);
        }
        EnqueueChildRows(currentRow, data, ruleSet, columns);
        EnqueueParentRows(currentRow, data, ruleSet, columns, null);
    }

    private void EnqueueAllRows(IDataDocument data, DataStructureRuleSet ruleSet, Hashtable columns)
    {
        List<DataTable> tables = GetSortedTables(data.DataSet);
        for (int i = tables.Count - 1; i >= 0; i--)
        {
            foreach (DataRow row in tables[i].Rows)
            {
                EnqueueEntry(row, data, ruleSet, columns);
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
                foreach (DataRow row in parentRow.GetChildRows(childRelation))
                {
                    if (!IsEntryInQueue(row, ruleSet))
                    {
                        EnqueueEntry(row, data, ruleSet, columns);
                    }
                    EnqueueChildRows(row, data, ruleSet, columns);
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
                foreach (DataRow row in childRow.GetParentRows(parentRelation))
                {
                    rows.Add(row);
                }
            }
        }
        else
        {
            rows.AddRange(parentRows);
        }
        foreach (DataRow row in rows)
        {
            if (!IsEntryInQueue(row, ruleSet))
            {
                EnqueueEntry(row, data, ruleSet, columns);
            }
            EnqueueParentRows(row, data, ruleSet, columns, null);
        }
    }

    public void ProcessRules(DataRow rowChanged, IDataDocument data, DataStructureRuleSet ruleSet)
    {
        ProcessRules(rowChanged, data, ruleSet, null);
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
        if (IsEntryInQueue(rowChanged, ruleSet))
        {
            wasQueued = true;
        }
        else
        {
            EnqueueEntry(rowChanged, data, ruleSet, null);
        }
        EnqueueChildRows(rowChanged, data, ruleSet, null);
        Hashtable columns = null;
        if (rowChanged.RowState == DataRowState.Deleted)
        {
            columns = new Hashtable();
            foreach (DataColumn col in rowChanged.Table.Columns)
            {
                columns[ColumnKey(col)] = col;
            }
        }
        EnqueueParentRows(rowChanged, data, ruleSet, columns, parentRows);
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
                    if (ProcessRulesFromQueue(row, data, rs, changedColumns.Values))
                    {
                        try
                        {
                            row.EndEdit();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                ex.Message
                                    + Environment.NewLine
                                    + ResourceUtils.GetString("RowState")
                                    + row.RowState.ToString(),
                                ex
                            );
                        }
                    }
                }
                catch (Exception e)
                {
                    row.CancelEdit();
                    if (log.IsErrorEnabled)
                    {
                        log.Error("Exception ocurred during evaluation of rule queue", e);
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

        if (!DatasetTools.HasRowValidParent(rowChanged))
        {
            return false;
        }

        bool result = false;
        bool resultRules = false;
        if (ruleSet != null)
        {
            List<DataStructureRule> rules;
            if (columnChanged == null)
            {
                rules = ruleSet.Rules(rowChanged.Table.TableName);
                foreach (DataColumn col in columnsChanged)
                {
                    // get all the rules
                    if (col.ExtendedProperties.Contains("Id"))
                    {
                        Guid fieldId = (Guid)col.ExtendedProperties["Id"];
                        List<DataStructureRule> r = ruleSet.Rules(
                            col.Table.TableName,
                            fieldId,
                            isFromRuleQueue
                        );
                        foreach (DataStructureRule rule in r)
                        {
                            if (rule.Entity.Name.Equals(rowChanged.Table.TableName))
                            {
                                if (!rules.Contains(rule))
                                {
                                    rules.Add(rule);
                                }
                            }
                        }
                    }
                    if (!isFromRuleQueue)
                    {
                        UpdateQueueEntries(rowChanged, ruleSet, col);
                        _ruleColumnChanges[ColumnKey(col)] = col;
                    }
                }
            }
            else
            {
                // columns we cannot recognize will not fire any events
                if (!columnChanged.ExtendedProperties.Contains("Id"))
                {
                    return false;
                }

                Guid fieldId = (Guid)columnChanged.ExtendedProperties["Id"];
                rules = ruleSet.Rules(rowChanged.Table.TableName, fieldId, false);
                UpdateQueueEntries(rowChanged, ruleSet, columnChanged);
                _ruleColumnChanges[ColumnKey(columnChanged)] = columnChanged;
            }
            rules.Sort(new ProcessRuleComparer());
            resultRules = ProcessRulesInternalFinish(rules, data, rowChanged, ruleSet);
        }
        // check for lookup fields changes
        if (columnChanged == null)
        {
            var copy = columnsChanged.ToArray<DataColumn>();
            foreach (DataColumn col in copy)
            {
                if (col.Table.TableName == rowChanged.Table.TableName)
                {
                    result = ProcessRulesLookupFields(rowChanged, col.ColumnName);
                }
            }
        }
        else
        {
            result = ProcessRulesLookupFields(rowChanged, columnChanged.ColumnName);
        }
        return result || resultRules;
    }

    public bool ProcessRulesLookupFields(DataRow row, string columnName)
    {
        bool changed = false;
        DataTable t = row.Table;
        Guid columnFieldId = (Guid)t.Columns[columnName].ExtendedProperties["Id"];
        foreach (DataColumn column in t.Columns)
        {
            if (column.ExtendedProperties.Contains(Const.OriginalFieldId))
            {
                Guid originalFieldId = (Guid)column.ExtendedProperties[Const.OriginalFieldId];
                // we find all columns that depend on the changed one
                if (originalFieldId.Equals(columnFieldId))
                {
                    if (column.ExtendedProperties.Contains(Const.OriginalLookupIdAttribute))
                    {
                        // and we reload the value by the original lookup
                        Guid originalLookupId = (Guid)
                            column.ExtendedProperties[Const.OriginalLookupIdAttribute];
                        object newValue = DBNull.Value;

                        if (!row.IsNull(columnName))
                        {
                            newValue = _lookupService.GetDisplayText(
                                originalLookupId,
                                row[columnName],
                                false,
                                false,
                                this.TransactionId
                            );
                        }

                        if (newValue == null)
                        {
                            newValue = DBNull.Value;
                        }
                        if (row[column] != newValue)
                        {
                            row[column] = newValue;
                            changed = true;
                        }
                    }
                    else
                    {
                        // or we just copy the original value (copied fields)
                        if (row[column] != row[columnName])
                        {
                            row[column] = row[columnName];
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
        DataStructureRuleSet ruleSet
    )
    {
        bool changed = false;
        var myRules = new List<DataStructureRule>(rules);
        foreach (DataStructureRule rule in myRules)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    "Evaluating Rule: "
                        + rule?.Name
                        + ", Target Field: "
                        + (rule?.TargetField == null ? "<none>" : rule?.TargetField.Name)
                );
            }
            // columns which don't allow nulls will not get processed when empty
            foreach (DataStructureRuleDependency dependency in rule.RuleDependencies)
            {
                if (dependency.Entity.Name.Equals(rowChanged.Table.TableName))
                {
                    if (
                        !dependency.Field.AllowNulls
                        && rowChanged[dependency.Field.Name] == DBNull.Value
                    )
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(
                                "   "
                                    + ResourceUtils.GetString(
                                        "PadAllowNulls",
                                        dependency.Entity.Name,
                                        dependency.Field.Name
                                    )
                            );
                        }
                        goto nextRule;
                    }
                }
            }
            XPathNodeIterator iterator = null;
            // we do a fresh slice after evaluating each rule, because data could have changed
            DataSet dataSlice = DatasetTools.CloneDataSet(rowChanged.Table.DataSet, false);
            DatasetTools.GetDataSlice(dataSlice, new List<DataRow> { rowChanged });
            IDataDocument xmlSlice = DataDocumentFactory.New(dataSlice);
            if (rule.ValueRule == null)
            {
                throw new Exception(
                    $"{nameof(DataStructureRule.ValueRule)} in {nameof(DataStructureRule)} {rule.Id} is null"
                );
            }
            if (rule.ValueRule.IsPathRelative)
            {
                if (data == null)
                {
                    throw new NullReferenceException(
                        "Rule has IsPathRelative set but no XmlDataDocument has been provided. Cannot evaluate rule."
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
                    if (t.ParentRelations[0].Nested)
                    {
                        t = t.ParentRelations[0].ParentTable;
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
                iterator = nav.Select(path);
                iterator.MoveNext();
            }
            // if exists, check condition, if the rule will be actually evaluated
            if (rule.ConditionRule != null)
            {
                if (rule.ConditionRule.IsPathRelative != rule.ValueRule.IsPathRelative)
                {
                    throw new ArgumentOutOfRangeException(
                        "IsPathRelative",
                        rule.ConditionRule.IsPathRelative,
                        ResourceUtils.GetString("ErrorRuleConditionEqual", rule.Path)
                    );
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug("   " + ResourceUtils.GetString("PadEvaluatingCondition"));
                }
                object shouldEvaluate = this.EvaluateRule(
                    rule.ConditionRule,
                    xmlSlice,
                    iterator == null ? null : iterator.Clone()
                );
                if (shouldEvaluate is bool)
                {
                    if (!(bool)shouldEvaluate)
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug("   " + ResourceUtils.GetString("PadConditionFalse"));
                        }
                        goto nextRule;
                    }
                }
                else
                {
                    throw new InvalidCastException(
                        ResourceUtils.GetString("ErrorNotBool", rule.Path)
                    );
                }
            }

            object result;
            try
            {
                result = this.EvaluateRule(rule.ValueRule, xmlSlice, iterator);
            }
            catch
            {
                throw;
            }
            #region Processing Result
            #region TRACE
            if (log.IsDebugEnabled)
            {
                log.RunHandled(() =>
                {
                    if (rule.TargetField != null)
                    {
                        string columnName = rule.TargetField.Name;
                        DataColumn col = rowChanged.Table.Columns[columnName];
                        object oldValue = rowChanged[col];
                        string newLookupValue = null;
                        string oldLookupValue = null;
                        if (col.ExtendedProperties.Contains(Const.DefaultLookupIdAttribute))
                        {
                            if (result != DBNull.Value && !(result is XmlDocument))
                            {
                                try
                                {
                                    newLookupValue = LookupValue(
                                        col.ExtendedProperties[Const.DefaultLookupIdAttribute]
                                            .ToString(),
                                        result.ToString()
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
                                        col.ExtendedProperties[Const.DefaultLookupIdAttribute]
                                            .ToString(),
                                        oldValue.ToString()
                                    );
                                }
                                catch (Exception ex)
                                {
                                    oldLookupValue = ex.Message;
                                }
                            }
                        }
                        log.Debug(
                            "   "
                                + ResourceUtils.GetString("PadRuleResult0")
                                + result.ToString()
                                + (newLookupValue == null ? "" : " (" + newLookupValue + ")")
                                + ResourceUtils.GetString("PadRuleResult1")
                                + columnName
                                + ResourceUtils.GetString("PadRuleResult2")
                                + oldValue.ToString()
                                + (oldLookupValue == null ? "" : " (" + oldLookupValue + ")")
                        );
                    }
                    else
                    {
                        log.Debug(
                            "   " + ResourceUtils.GetString("PadRuleResult0") + result.ToString()
                        );
                    }
                });
            }
            #endregion
            if (result is IDataDocument)
            {
                // RESULT IS DATASET
                DataTable resultTable = (result as IDataDocument).DataSet.Tables[
                    rowChanged.Table.TableName
                ];
                if (resultTable == null)
                {
                    string message = ResourceUtils.GetString(
                        "PadRuleInvalidStructure",
                        rowChanged.Table.TableName
                    );
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(message);
                    }
                    throw new Exception(message);
                }
                // find the record in the transformed document
                DataRow resultRow = resultTable.Rows.Find(DatasetTools.PrimaryKey(rowChanged));
                if (resultRow == null)
                {
                    // row was not generated by the rule, this is a problem, the row must always be returned
                    string message = ResourceUtils.GetString("PadRuleInvalidNoData");
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(message);
                    }
                    throw new Exception(message);
                }
                else
                {
                    var changedColumns = new List<DataColumn>();
                    var changedTargetColumns = new List<DataColumn>(changedColumns.Count);
                    foreach (DataColumn col in resultRow.Table.Columns)
                    {
                        if (
                            rowChanged.Table.Columns.Contains(col.ColumnName)
                            && !(resultRow[col].Equals(rowChanged[col.ColumnName]))
                        )
                        {
                            changedColumns.Add(col);
                            changedTargetColumns.Add(rowChanged.Table.Columns[col.ColumnName]);
                        }
                    }
                    #region TRACE
                    if (log.IsDebugEnabled)
                    {
                        log.RunHandled(() =>
                        {
                            foreach (DataColumn col in changedColumns)
                            {
                                string newLookupValue = null;
                                string oldLookupValue = null;
                                object resultValue = resultRow[col];
                                object oldValue = rowChanged[col.ColumnName];
                                string columnName = col.ColumnName;
                                if (
                                    col.ExtendedProperties.Contains(Const.DefaultLookupIdAttribute)
                                    && col.ExtendedProperties.Contains(Const.OrigamDataType)
                                    && !OrigamDataType.Array.Equals(
                                        col.ExtendedProperties[Const.OrigamDataType]
                                    )
                                )
                                {
                                    if (resultValue != DBNull.Value)
                                    {
                                        newLookupValue = LookupValue(
                                            col.ExtendedProperties[Const.DefaultLookupIdAttribute]
                                                .ToString(),
                                            resultValue.ToString()
                                        );
                                    }
                                    if (oldValue != DBNull.Value)
                                    {
                                        oldLookupValue = LookupValue(
                                            col.ExtendedProperties[Const.DefaultLookupIdAttribute]
                                                .ToString(),
                                            oldValue.ToString()
                                        );
                                    }
                                }
                                log.Debug(
                                    "   "
                                        + columnName
                                        + ": "
                                        + resultValue.ToString()
                                        + (
                                            newLookupValue == null
                                                ? ""
                                                : " (" + newLookupValue + ")"
                                        )
                                        + ResourceUtils.GetString("PadRuleResult1")
                                        + ResourceUtils.GetString("PadRuleResult2")
                                        + oldValue.ToString()
                                        + (
                                            oldLookupValue == null
                                                ? ""
                                                : " (" + oldLookupValue + ")"
                                        )
                                );
                            }
                        });
                    }
                    #endregion
                    // copy the values into the source row
                    PauseRuleProcessing();
                    bool localChanged = DatasetTools.CopyRecordValues(
                        resultRow,
                        DataRowVersion.Current,
                        rowChanged,
                        true
                    );
                    ResumeRuleProcessing();
                    if (!changed)
                    {
                        changed = localChanged;
                    }

                    ProcessRules(rowChanged, data, changedTargetColumns, ruleSet);
                }
            }
            else if (result is XmlDocument)
            {
                // XML IS NOT SUPPORTED
                string message = ResourceUtils.GetString("PadXmlDocument");
                if (rule.ValueRule != null && rule.ValueRule is Origam.Schema.RuleModel.XslRule)
                {
                    message += ResourceUtils.GetString(
                        "FixXslRuleWithDestinationDataStructure",
                        rule.ValueRule.ToString()
                    );
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug(message);
                }
                throw new NotSupportedException(message);
            }
            else
            {
                // SIMPLE DATA TYE (e.g. XPath Rule). TargetField must be used to return the result to a specific column.
                if (rule.TargetField == null)
                {
                    string message = ResourceUtils.GetString("PadTargetField");
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(message);
                    }
                    throw new Exception(message);
                }
                foreach (DataColumn column in rowChanged.Table.Columns)
                {
                    if (column.ExtendedProperties["Id"].Equals(rule.TargetField.PrimaryKey["Id"]))
                    {
                        if (!rowChanged[column].Equals(result))
                        {
                            rowChanged[column] = result;
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
            log.Debug(ResourceUtils.GetString("PadRuleFinished", DateTime.Now, changed.ToString()));
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
        EntityFormatting formatting = new EntityFormatting(NullColor, NullColor);
        var entityRules = new List<EntityConditionalFormatting>();
        IDataEntity entity =
            _persistence.SchemaProvider.RetrieveInstance(
                typeof(ISchemaItem),
                new ModelElementKey(entityId)
            ) as IDataEntity;

        if (fieldId == Guid.Empty)
        {
            entityRules.AddRange(entity.ConditionalFormattingRules);
        }
        else
        {
            // we retrieve the column from the child-items list
            // this is very cost efficient, because when retrieving abstract columns (i.e. Id, RecordCreated, RecordUpdated), they are never cached
            IDataEntityColumn field = entity.GetChildById(fieldId) as IDataEntityColumn;
            if (field != null)
            {
                entityRules.AddRange(field.ConditionalFormattingRules);
            }
        }
        if (entityRules.Count > 0)
        {
            entityRules.Sort();

            foreach (EntityConditionalFormatting rule in entityRules)
            {
                if (IsRuleMatching(data, rule.Rule, rule.Roles, contextPosition))
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
                        lookupParam = EvaluateRule(xpr, data, contextPosition);
                    }
                    if (lookupParam != DBNull.Value)
                    {
                        if (rule.ForegroundColorLookup != null)
                        {
                            if (rule.DynamicColorLookupField == null)
                            {
                                throw new Exception(
                                    ResourceUtils.GetString("ErrorNoForegroundDynamicColorLookup")
                                );
                            }

                            object color = _lookupService.GetDisplayText(
                                rule.ForeColorLookupId,
                                lookupParam,
                                false,
                                false,
                                null
                            );

                            if (color is int)
                            {
                                foreColor = System.Drawing.Color.FromArgb((int)color);
                            }
                        }
                        if (rule.BackgroundColorLookup != null)
                        {
                            if (rule.DynamicColorLookupField == null)
                            {
                                throw new Exception(
                                    ResourceUtils.GetString("ErrorNoBackgroundDynamicColorLookup")
                                );
                            }

                            object color = _lookupService.GetDisplayText(
                                rule.BackColorLookupId,
                                lookupParam,
                                false,
                                false,
                                null
                            );

                            if (color is int)
                            {
                                backColor = System.Drawing.Color.FromArgb((int)color);
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
                typeof(ISchemaItem),
                new ModelElementKey(entityId)
            ) as IDataEntity;
        IDataEntityColumn field = entity.GetChildById(fieldId) as IDataEntityColumn;
        if (field == null)
        {
            return null; // lookup fields in a data structure
        }

        rules.AddRange(field.DynamicLabels);
        if (rules.Count > 0)
        {
            rules.Sort();
            foreach (EntityFieldDynamicLabel rule in rules)
            {
                if (IsRuleMatching(data, rule.Rule, rule.Roles, contextPosition))
                {
                    string result = (string)
                        _parameterService.GetParameterValue(
                            rule.LabelConstantId,
                            OrigamDataType.String
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
        if (!DatasetTools.HasRowValidParent(row))
        {
            return true;
        }

        Guid entityId = Guid.Empty;
        Guid fieldId = Guid.Empty;
        if (row.Table.ExtendedProperties.Contains("EntityId"))
        {
            XmlContainer originalData = DatasetTools.GetRowXml(row, DataRowVersion.Original);
            XmlContainer actualData = DatasetTools.GetRowXml(
                row,
                row.HasVersion(DataRowVersion.Proposed)
                    ? DataRowVersion.Proposed
                    : DataRowVersion.Default
            );
            fieldId = (Guid)row.Table.Columns[field].ExtendedProperties["Id"];
            entityId = (Guid)row.Table.ExtendedProperties["EntityId"];
            return EvaluateRowLevelSecurityState(
                originalData,
                actualData,
                field,
                type,
                entityId,
                fieldId,
                row.RowState == DataRowState.Added || row.RowState == DataRowState.Detached
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
        IDataEntity entity = _persistence.SchemaProvider.RetrieveInstance<IDataEntity>(entityId);
        foreach (
            EntityUIAction action in entity.ChildItemsByTypeRecursive(EntityUIAction.CategoryConst)
        )
        {
            // Performance sensitive! RuleDisablesAction method should not
            // be invoked unless it is really necessary.
            if (
                IsFeatureOff(action)
                || IsDisabledByMode(actualData, action)
                || IsDisabledByScreenCondition(formId, action)
                || IsDisabledByScreenSectionCondition(formId, action)
                || IsDisabledByRoles(action)
                || RuleDisablesAction(originalData, actualData, action)
            )
            {
                result.Add(action.Id.ToString());
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
        return !_authorizationProvider.Authorize(SecurityManager.CurrentPrincipal, action.Roles);
    }

    private bool IsDisabledByScreenSectionCondition(Guid formId, EntityUIAction action)
    {
        if (formId == Guid.Empty)
        {
            return false;
        }
        var panelIds = _persistence
            .SchemaProvider.RetrieveInstance<FormControlSet>(formId)
            .ChildrenRecursive.OfType<ControlSetItem>()
            .Select(controlSet => controlSet.ControlItem.PanelControlSetId)
            .Where(panelId => panelId != Guid.Empty)
            .ToList();
        return action.ScreenSectionIds.Any()
            && panelIds.Count > 0
            && !panelIds.Any(panelId => action.ScreenSectionIds.Contains(panelId));
    }

    private static bool IsDisabledByScreenCondition(Guid formId, EntityUIAction action)
    {
        return formId != Guid.Empty && action.ScreenIds.Any() && !action.ScreenIds.Contains(formId);
    }

    private static bool IsDisabledByMode(XmlContainer actualData, EntityUIAction action)
    {
        return action.Mode == PanelActionMode.ActiveRecord && actualData == null;
    }

    private bool IsFeatureOff(EntityUIAction action)
    {
        return !_parameterService.IsFeatureOn(action.Features);
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
            && !IsRuleMatching(dataToUseForRule, action.Rule, action.Roles, null);
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
                typeof(ISchemaItem),
                new ModelElementKey(entityId)
            ) as IDataEntity;
        // field-level rules
        IDataEntityColumn column = null;
        if (field != null)
        {
            // we retrieve the column from the child-items list
            // this is very cost efficient, because when retrieving
            // abstract columns (i.e. Id, RecordCreated, RecordUpdated), they are never cached
            column = entity.GetChildById(fieldId) as IDataEntityColumn;
            // field not found, this would be e.g. a looked up column,
            // which does not point to a real entity field id
            if (column != null)
            {
                if (!column.RowLevelSecurityRules.Any())
                {
                    // shortcircuit processing of row level security rules
                    // for a column without it's own rules
                    Boolean? result = ruleEvaluationCache?.GetRulelessFieldResult(entityId, type);
                    if (result != null)
                    {
                        return result.Value;
                    }
                }
                else
                {
                    rules.AddRange(column.RowLevelSecurityRules);
                }
            }
        }
        // entity-level rules
        List<AbstractEntitySecurityRule> entityRules = entity.RowLevelSecurityRules;
        if (entityRules.Count > 0)
        {
            rules.AddRange(entityRules);
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
                    return PutToRulelessCache(type, entityId, ruleEvaluationCache, column, true);
                }
                else if (
                    (entityRule.UpdateCredential && type == CredentialType.Update)
                    || (entityRule.CreateCredential && type == CredentialType.Update && isNewRow)
                    || (entityRule.CreateCredential && type == CredentialType.Create)
                    || (entityRule.DeleteCredential && type == CredentialType.Delete)
                )
                {
                    bool? result = ruleEvaluationCache?.Get(entityRule, entityId);
                    if (result == null)
                    {
                        result = IsRowLevelSecurityRuleMatching(
                            entityRule,
                            entityRule.ValueType == CredentialValueType.ActualValue
                                ? actualData
                                : originalData
                        );
                        ruleEvaluationCache?.Put(entityRule, entityId, result.Value);
                    }
                    if (result.Value)
                    {
                        return PutToRulelessCache(
                            type,
                            entityId,
                            ruleEvaluationCache,
                            column,
                            entityRule.Type == PermissionType.Permit
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
                    Boolean? result = ruleEvaluationCache?.Get(fieldRule, entityId);
                    if (result == null)
                    {
                        result = IsRowLevelSecurityRuleMatching(
                            fieldRule,
                            fieldRule.ValueType == CredentialValueType.ActualValue
                                ? actualData
                                : originalData
                        );
                        ruleEvaluationCache?.Put(fieldRule, entityId, result.Value);
                    }
                    if (result.Value)
                    {
                        return PutToRulelessCache(
                            type,
                            entityId,
                            ruleEvaluationCache,
                            column,
                            fieldRule.Type == PermissionType.Permit
                        );
                    }
                }
            }
        }
        // no match
        if (type == CredentialType.Read)
        {
            return PutToRulelessCache(type, entityId, ruleEvaluationCache, column, true);
        }

        return PutToRulelessCache(type, entityId, ruleEvaluationCache, column, false);
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
            ruleEvaluationCache.PutRulelessFieldResult(entityId, type, value);
        }
        return value;
    }

    private bool IsRowLevelSecurityRuleMatching(AbstractEntitySecurityRule rule, XmlContainer data)
    {
        return IsRuleMatching(data, rule.Rule, rule.Roles, null);
    }

    private bool IsRuleMatching(
        XmlContainer data,
        IRule rule,
        string roles,
        XPathNodeIterator contextPosition
    )
    {
        // check roles
        if (!_authorizationProvider.Authorize(SecurityManager.CurrentPrincipal, roles))
        {
            return false;
        }
        // check business rule
        if (rule != null)
        {
            object result = this.EvaluateRule(rule, data, contextPosition);
            if (result is bool)
            {
                return (bool)result;
            }

            throw new ArgumentException(
                "Rule resulted in a result which is not boolean. Cannot evaluate non-boolean rules. Rule: "
                    + ((ISchemaItem)rule).Path
            );
        }
        return true;
    }

    public bool IsExportAllowed(Guid entityId)
    {
        var rules = new List<AbstractEntitySecurityRule>();
        var entity = _persistence.SchemaProvider.RetrieveInstance<IDataEntity>(entityId);
        List<AbstractEntitySecurityRule> entityRules = entity.RowLevelSecurityRules;
        if (entityRules.Count > 0)
        {
            rules.AddRange(entityRules);
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
        return _contextStores[key];
    }

    public void SetContext(Key key, object value)
    {
        _contextStores[key] = value;
    }

    public object GetContext(IContextStore contextStore)
    {
        return GetContext(contextStore.PrimaryKey);
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
            data = GetContext(contextStore);
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
                "GetXmlDocumentFromData called with System.Xml.XmlDataDocuement."
                    + "This isn't expected. Refactor code to be called with IXmlContainer. (documentElement:{0})",
                xmlDoc.DocumentElement.Name
            );
            return new XmlContainer(xmlDoc);
        }
        if (data is int)
        {
            data = XmlConvert.ToString((int)data);
        }
        else if (data is Guid)
        {
            data = XmlConvert.ToString((Guid)data);
        }
        else if (data is long)
        {
            data = XmlConvert.ToString((long)data);
        }
        else if (data is decimal)
        {
            data = XmlConvert.ToString((decimal)data);
        }
        else if (data is bool)
        {
            data = XmlConvert.ToString((bool)data);
        }
        else if (data is DateTime)
        {
            data = XmlConvert.ToString((DateTime)data, XmlDateTimeSerializationMode.RoundtripKind);
        }
        else if (data == null)
        {
            return new XmlContainer("<ROOT/>");
        }
        else if (data is IList)
        {
            doc = new XmlContainer();
            XmlElement root = (XmlElement)doc.Xml.AppendChild(doc.Xml.CreateElement("ROOT"));
            foreach (object item in data as IList)
            {
                root.AppendChild(doc.Xml.CreateElement("value")).InnerText = item.ToString();
            }
            return doc;
        }
        else
        {
            data = data.ToString();
        }
        doc = new XmlContainer();
        doc.Xml.LoadXml("<ROOT><value /></ROOT>");
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
                return this.ActiveProfileId();
            case SystemFunction.ResourceIdByActiveProfile:
                return resourceTools.ResourceIdByActiveProfile();
            default:
                throw new ArgumentOutOfRangeException(
                    "Function",
                    functionCall.Function,
                    ResourceUtils.GetString("ErrorUnsupportedFunction")
                );
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
            throw new NullReferenceException(ResourceUtils.GetString("ErrorEvaluateContextNull"));
        }
        if (log.IsDebugEnabled)
        {
            log.Debug("Evaluating XPath Rule: " + rule?.Name);
            if (contextPosition != null)
            {
                log.Debug("Current Position: " + contextPosition?.Current?.Name);
            }
            log.Debug("  Input data: " + xmlDocument.OuterXml);
        }
        XPathNavigator nav = xmlDocument.CreateNavigator();
        using (MiniProfiler.Current.CustomTiming("rule", rule.Name, "XPathRuleEvaluation"))
        {
            return XpathEvaluator.Instance.Evaluate(
                rule.XPath,
                rule.IsPathRelative,
                rule.DataType,
                nav,
                contextPosition,
                _transactionId
            );
        }
    }

    private object EvaluateRule(XslRule rule, IXmlContainer context)
    {
        try
        {
            using (MiniProfiler.Current.CustomTiming("rule", rule.Name, "XslRuleEvaluation"))
            {
                IXmlContainer result = _transformer.Transform(
                    context,
                    rule.Id,
                    null,
                    _transactionId,
                    rule.Structure,
                    false
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
            throw new Exception(ResourceUtils.GetString("ErrorRuleFailed2"), ex);
        }
    }
    #endregion
    private void table_RowChanged(object sender, DataRowChangeEventArgs e)
    {
        ProcessRules(e.Row, _currentRuleDocument, _ruleSet);
    }

    private void table_ColumnChanged(object sender, DataColumnChangeEventArgs e)
    {
        ProcessRules(e.Row, _currentRuleDocument, e.Column, _ruleSet);
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
        return new XmlNodeListEnumerator(this);
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
            this.ReadUntil(index);
        }
        if ((index < this.list.Count) && (index >= 0))
        {
            return (XmlNode)this.list[index];
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
                XmlNode node1 = this.GetNode(this.iterator.Current);
                if (node1 != null)
                {
                    this.list.Add(node1);
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
                this.ReadUntil(0x7fffffff);
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
        int num1 = this.list.ReadUntil(this.index + 1);
        if (this.index > (num1 - 1))
        {
            return false;
        }
        this.valid = this.list[this.index] != null;
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
                return this.list[this.index];
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
            return x.Priority.CompareTo(y.Priority);
        }
        // rulesets are always an top, so rules are greater
        return 1;
    }
}
#endregion
