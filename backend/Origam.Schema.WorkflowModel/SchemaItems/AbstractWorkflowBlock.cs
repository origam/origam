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
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel;
/// <summary>
/// Summary description for AbstractWorkflowBlock.
/// </summary>
public abstract class AbstractWorkflowBlock : AbstractWorkflowStep, IWorkflowBlock
{
	public AbstractWorkflowBlock() : base() {Init();}
	public AbstractWorkflowBlock(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public AbstractWorkflowBlock(Key primaryKey) : base(primaryKey)	{Init();}
	private void Init()
	{
		this.ChildItemTypes.Add(typeof(ServiceMethodCallTask));
		this.ChildItemTypes.Add(typeof(UIFormTask));
		this.ChildItemTypes.Add(typeof(WorkflowCallTask));
		this.ChildItemTypes.Add(typeof(SetWorkflowPropertyTask));
		this.ChildItemTypes.Add(typeof(UpdateContextTask));
		this.ChildItemTypes.Add(typeof(AcceptContextStoreChangesTask));
		this.ChildItemTypes.Add(typeof(TransactionWorkflowBlock));
		this.ChildItemTypes.Add(typeof(ForeachWorkflowBlock));
		this.ChildItemTypes.Add(typeof(LoopWorkflowBlock));
		this.ChildItemTypes.Add(typeof(ContextStore));
		this.ChildItemTypes.Add(typeof(CheckRuleStep));
		this.ChildItemTypes.Add(typeof(WaitTask));
	}
	#region Overriden ISchemaItem Members
	
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	#endregion
}
