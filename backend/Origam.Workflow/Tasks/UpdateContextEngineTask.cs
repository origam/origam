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
using System.Data;
using System.Xml;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Workflow.Tasks;

/// <summary>
/// Summary description for UpdateContextEngineTask.
/// </summary>
public class UpdateContextEngineTask : AbstractWorkflowEngineTask
{
    public UpdateContextEngineTask()
        : base() { }

    protected override void OnExecute()
    {
        bool changed = false;
        ITracingService tracingService =
            ServiceManager.Services.GetService(typeof(ITracingService)) as ITracingService;
        UpdateContextTask updateTask = this.Step as UpdateContextTask;
        Key outputCtxtKey = updateTask.OutputContextStore.PrimaryKey;
        if (Engine.IsTrace(Step))
        {
            tracingService.TraceStep(
                this.Engine.WorkflowInstanceId,
                Step.Path,
                (Guid)this.Step.PrimaryKey["Id"],
                "Update Context Store",
                "Input",
                updateTask.OutputContextStore.Name,
                WorkflowEngine.ContextData(this.Engine.RuleEngine.GetContext(outputCtxtKey)),
                null,
                null
            );
        }
        // 1. get a value from XPath and XPathContextStore
        IXmlContainer xPathXMLDoc = this.Engine.RuleEngine.GetXmlDocumentFromData(
            updateTask.XPathContextStore
        );
        string val = (string)
            this.Engine.RuleEngine.EvaluateContext(
                updateTask.ValueXPath,
                xPathXMLDoc,
                OrigamDataType.String,
                null
            );

        DataStructureRuleSet ruleSet = null;
        if (updateTask.OutputContextStore.Structure != null)
        {
            object res = this.Engine.RuleEngine.GetXmlDocumentFromData(
                updateTask.OutputContextStore
            );
            if (res is IDataDocument)
            {
                // update dataset
                DataSet outputDS = ((IDataDocument)res).DataSet;
                // find the target table in the dataset
                DataTable table = outputDS.Tables[updateTask.Entity.Name];
                // find out the data type of target field
                OrigamDataType origamDataType = updateTask.GetFieldSchemaItem().DataType;
                object valueToContext = val;
                RuleEngine.ConvertStringValueToContextValue(
                    origamDataType,
                    val,
                    ref valueToContext
                );
                if (valueToContext == null)
                {
                    valueToContext = DBNull.Value;
                }
                // update fields in dataset table's rows
                foreach (DataRow row in table.Rows)
                {
                    changed = true;
                    row[updateTask.FieldName] = valueToContext;
                }
                ruleSet = this.Engine.ContextStoreRuleSet(outputCtxtKey);
            }
            else
            {
                // not-typed xml document
                throw new WorkflowException(
                    ResourceUtils.GetString(
                        "ErrorWrongContextStoreForUpdate",
                        updateTask.OutputContextStore.Name
                    )
                );
            }
        }
        else
        {
            // simple context value
            // get that context store directly
            //object simpleContextData = this.Engine.RuleEngine.ContextStores[outputCtxtKey];
            // get type of this context store
            OrigamDataType contextType = this.Engine.ContextStoreType(outputCtxtKey);
            // convert value to update to proper context type and set to context store
            object contextValue = val;
            RuleEngine.ConvertStringValueToContextValue(contextType, val, ref contextValue);
            this.Engine.RuleEngine.SetContext(outputCtxtKey, contextValue);
            changed = true;
        }
        if (Engine.IsTrace(Step))
        {
            tracingService.TraceStep(
                this.Engine.WorkflowInstanceId,
                this.Step.Path,
                (Guid)this.Step.PrimaryKey["Id"],
                "Update Context Store",
                "Result",
                updateTask.OutputContextStore.Name,
                changed
                    ? WorkflowEngine.ContextData(this.Engine.RuleEngine.GetContext(outputCtxtKey))
                    : "-- no change --",
                null,
                null
            );
        }
        if (changed && ruleSet != null)
        {
            this.Engine.RuleEngine.ProcessRules(
                this.Engine.RuleEngine.GetContext(outputCtxtKey) as IDataDocument,
                ruleSet,
                null
            );
            if (Engine.IsTrace(Step))
            {
                tracingService.TraceStep(
                    this.Engine.WorkflowInstanceId,
                    Step.Path,
                    (Guid)this.Step.PrimaryKey["Id"],
                    "Rule Processing",
                    "Result",
                    updateTask.OutputContextStore.Name,
                    WorkflowEngine.ContextData(this.Engine.RuleEngine.GetContext(outputCtxtKey)),
                    null,
                    null
                );
            }
        }
    }
}
