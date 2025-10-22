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
using System.Xml;
using Origam.Rule;
using Origam.Rule.Xslt;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Workflow.Tasks;

/// <summary>
/// Summary description for SetWorkflowPropertyEngineTask.
/// </summary>
public class SetWorkflowPropertyEngineTask : AbstractWorkflowEngineTask
{
    public SetWorkflowPropertyEngineTask()
        : base() { }

    protected override void OnExecute()
    {
        SetWorkflowPropertyTask setProperty = this.Step as SetWorkflowPropertyTask;
        IXmlContainer data = this.Engine.RuleEngine.GetXmlDocumentFromData(
            setProperty.ContextStore
        );
        if (setProperty.Transformation != null)
        {
            IPersistenceService persistence =
                ServiceManager.Services.GetService(typeof(IPersistenceService))
                as IPersistenceService;
            IXsltEngine transform = new CompiledXsltEngine(persistence.SchemaProvider);
            data = transform.Transform(
                data,
                setProperty.TransformationId,
                new Hashtable(),
                Engine.TransactionId,
                null,
                false
            );
        }
        string propertyValue = (string)
            this.Engine.RuleEngine.EvaluateContext(
                setProperty.XPath,
                data,
                OrigamDataType.String,
                null
            );
        string delimiter = (
            setProperty.Delimiter == @"\n" ? Environment.NewLine : setProperty.Delimiter
        );

        switch (setProperty.WorkflowProperty)
        {
            case WorkflowProperty.Title:
            {
                if (
                    setProperty.Method == SetWorkflowPropertyMethod.Add
                    && this.Engine.RuntimeDescription != ""
                )
                {
                    this.Engine.RuntimeDescription += delimiter + propertyValue;
                }
                else
                {
                    this.Engine.RuntimeDescription = propertyValue;
                }
                break;
            }

            case WorkflowProperty.Notification:
            {
                if (
                    setProperty.Method == SetWorkflowPropertyMethod.Add
                    && this.Engine.Notification != ""
                )
                {
                    this.Engine.Notification += delimiter + propertyValue;
                }
                else
                {
                    this.Engine.Notification = propertyValue;
                }
                break;
            }

            case WorkflowProperty.ResultMessage:
            {
                if (
                    setProperty.Method == SetWorkflowPropertyMethod.Add
                    && this.Engine.ResultMessage != ""
                )
                {
                    this.Engine.ResultMessage += delimiter + propertyValue;
                }
                else
                {
                    this.Engine.ResultMessage = propertyValue;
                }
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    "WorkflowProperty",
                    setProperty.WorkflowProperty,
                    ResourceUtils.GetString("ErrorUnknownWorkflow")
                );
            }
        }
    }
}
