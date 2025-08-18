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

using System.Collections;
using System.Collections.Generic;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.RuleModel;

namespace Origam.Schema.WorkflowModel;
public enum WorkflowStepStartEvent
{
	Success,
	Failure,
	Finish
}
public enum WorkflowStepResult
{
	Ready,
	Success,
	Failure,
	NotRun,
	FailureNotRun,
	Running
}
/// <summary>
/// Summary description for IWorkflowStep.
/// </summary>
public interface IWorkflowStep : ISchemaItem, ITraceable
{
	StartRule StartConditionRule{get; set;}
	IContextStore StartConditionRuleContextStore{get; set;}
	IEndRule ValidationRule{get; set;}
	IContextStore ValidationRuleContextStore{get; set;}
	string Roles{get;set;}
	string Features{get;set;}
	List<WorkflowTaskDependency> Dependencies{get;}
	StepFailureMode OnFailure { set; get; }
}
public enum StepFailureMode
{
	WorkflowFails, Suppress
}
