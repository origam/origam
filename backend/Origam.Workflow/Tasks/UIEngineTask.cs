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
#pragma warning disable IDE0005
using System.Data;
using Origam.DA;
#pragma warning restore IDE0005
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
#pragma warning disable IDE0005
using Origam.Workbench.Services;
#pragma warning restore IDE0005

namespace Origam.Workflow.Tasks;

/// <summary>
/// Summary description for UIEngineTask.
/// </summary>
public class UIEngineTask : AbstractWorkflowEngineTask
{
    public UIEngineTask()
        : base() { }

    public override void Execute()
    {
        Exception exception = null;
        try
        {
            MeasuredExecution();
        }
        catch (Exception ex)
        {
            exception = ex;
            OnFinished(e: new WorkflowEngineTaskEventArgs(exception: exception));
        }
    }

    protected override void OnExecute()
    {
        UIFormTask task = this.Step as UIFormTask;
        // Cloning the dataset is neccessary, because the form will eventually add new columns
        // into the dataset (because of GUID columns sorting). This is not possible if the DataSet
        // is mapped to an XmlDataDocument (which it is always in the workflow).
        IDataDocument originalData =
            this.Engine.RuleEngine.GetContext(contextStore: task.OutputContextStore)
            as IDataDocument;
        if (originalData == null)
        {
            throw new Exception(message: ResourceUtils.GetString(key: "ErrorContextEmpty"));
        }
        IDataDocument data = originalData;
#if ORIGAM_SERVER
        if (task.OutputMethod != ServiceOutputMethod.FullMerge)
        {
            data =
                this.Engine.CloneContext(context: originalData, returnDataSet: false)
                as IDataDocument;
        }
#else
        DataSet cloned = this.Engine.CloneContext(originalData, true) as DataSet;
        DatasetTools.AddSortColumns(cloned);
        data = DataDocumentFactory.New(cloned);
#endif
        IEndRule validationRule = null;
        // only assign a validation rule, if it is on the context the user will be editing
        if (
            task.ValidationRuleContextStore != null
            && task.OutputContextStore.PrimaryKey.Equals(
                obj: task.ValidationRuleContextStore.PrimaryKey
            )
        )
        {
            validationRule = task.ValidationRule;
        }
        AbstractDataStructure structure;
        if (task.RefreshDataStructure == null)
        {
            structure = task.OutputContextStore.Structure;
        }
        else
        {
            structure = task.RefreshDataStructure;
        }
        Hashtable parameters = new Hashtable();
        // get parameters for the form
        foreach (ISchemaItem param in task.RefreshParameters)
        {
            parameters.Add(key: param.Name, value: this.Evaluate(item: param));
        }
        this.Engine.Host.OnWorkflowForm(
            task: this,
            data: data,
            description: this.Engine.GetTaskDescription(task: task),
            notification: this.Engine.Notification,
            form: task.Screen,
            ruleSet: task.OutputContextStore.RuleSet,
            endRule: validationRule,
            isFinalForm: task.IsFinalForm,
            allowSave: task.AllowSave,
            isAutoNext: task.AutoNext,
            structure: structure,
            refreshMethod: task.RefreshMethod,
            refreshSort: task.RefreshSortSet,
            isRefreshSuppressedBeforeFirstSave: task.IsRefreshSuppressedBeforeFirstSave,
            saveConfirmationRule: task.SaveConfirmationRule,
            saveStructure: task.SaveDataStructure,
            parameters: parameters,
            refreshPortalAfterSave: task.RefreshPortalAfterSave == TrueFalseEnum.True
        );
    }

    public void Finish()
    {
        if (this.Result == null)
        {
            OnFinished(
                e: new WorkflowEngineTaskEventArgs(
                    exception: new NullReferenceException(
                        message: ResourceUtils.GetString(key: "ErrorNoResultData")
                    )
                )
            );
        }

        OnFinished(e: new WorkflowEngineTaskEventArgs());
    }

    public void Abort(bool isDirty)
    {
        bool showAbortError = isDirty || !((UIFormTask)Step).IsFinalForm;
        OnFinished(
            e: showAbortError
                ? new WorkflowEngineTaskEventArgs(
                    exception: new WorkflowCancelledByUserException(
                        message: ResourceUtils.GetString(key: "ErrorUserCanceled")
                    )
                )
                : new WorkflowEngineTaskEventArgs()
        );
    }
}
