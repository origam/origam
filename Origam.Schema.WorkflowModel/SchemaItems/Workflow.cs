#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.RuleModel;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for Workflow.
	/// </summary>
	[SchemaItemDescription("Sequential Workflow", "sequential-workflow.png")]
    [HelpTopic("Sequential+Workflows")]
	[XmlModelRoot(ItemTypeConst)]
	public class Workflow : AbstractSchemaItem, IWorkflow
	{
		public const string ItemTypeConst = "Workflow";

		public Workflow() : base() {Init();}

		public Workflow(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}

		public Workflow(Key primaryKey) : base(primaryKey)	{Init();}

		private void Init()
		{
			this.ChildItemTypes.Add(typeof(ServiceMethodCallTask));
			this.ChildItemTypes.Add(typeof(UIFormTask));
			this.ChildItemTypes.Add(typeof(WorkflowCallTask));
			this.ChildItemTypes.Add(typeof(SetWorkflowPropertyTask));
			this.ChildItemTypes.Add(typeof(UpdateContextTask));
			this.ChildItemTypes.Add(typeof(TransactionWorkflowBlock));
			this.ChildItemTypes.Add(typeof(ForeachWorkflowBlock));
			this.ChildItemTypes.Add(typeof(LoopWorkflowBlock));
			this.ChildItemTypes.Add(typeof(ContextStore));
			this.ChildItemTypes.Add(typeof(CheckRuleStep));
			this.ChildItemTypes.Add(typeof(WaitTask));
		}

		/* @Short returns a name of context store that is used for returning values.
		 *        If there isn't such a context, null is returned.
		 * 
		 * */
		public ContextStore GetReturnContext ()
		{
			foreach (ContextStore context in ChildItemsByType(ContextStore.ItemTypeConst))
			{
				if (context.IsReturnValue) return context;
			}
			return null;
		}

        #region IWorkflowStep Members
		[DefaultValue(WorkflowTransactionBehavior.InheritExisting)]
		[Category("Transactions"), RefreshProperties(RefreshProperties.Repaint)]
		[EntityColumn("I02")] 
        [DisplayName("Transaction Behavior")]
		[XmlAttribute ("transactionBehavior")]
        [Description("Controls how will workflow interact with incoming transactions. The default behavior is to inherit them.")]
        public WorkflowTransactionBehavior TransactionBehavior { get; set; } 
			= WorkflowTransactionBehavior.InheritExisting;
		#endregion

        #region Overriden AbstractSchemaItem Members

        [EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ItemTypeConst;
			}
		}
		#endregion

		#region IWorkflowStep Members
        [Browsable(false)]
        public ArrayList Dependencies
		{
			get
			{
				return new ArrayList();
			}
		}

		[DefaultValue(WorkflowStepTraceLevel.None)]
		[Category("Tracing"), RefreshProperties(RefreshProperties.Repaint)]
		[EntityColumn("I01")] 
		[XmlAttribute ("traceLevel")]
        [DisplayName("Trace Level")]
		public WorkflowStepTraceLevel TraceLevel { get; set; } = WorkflowStepTraceLevel.None;

		[Category("Tracing")]
   
        public Trace Trace
		{
			get
			{
				foreach(object s in this.ChildItemsRecursive)
				{
					if(s is IWorkflowStep)
					{
						if((s as IWorkflowStep).Trace == Trace.Yes)
						{
							return Trace.Yes;
						}
						else if(s is WorkflowCallTask)
						{
							// skip any direct recursion
							if(!(s as WorkflowCallTask).Workflow.PrimaryKey.Equals(this.PrimaryKey))
							{
								Trace result = (s as WorkflowCallTask).Workflow.Trace;

								if(result == Trace.Yes) return Trace.Yes;
							}
						}
					}
				}

				return Trace.No;
			}
		}

		[Browsable(false)]
		public StartRule StartConditionRule
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException("Cannot set start rule to Workflow");
			}
		}

		[Browsable(false)]
		public IContextStore StartConditionRuleContextStore
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException("Cannot set start rule to Workflow");
			}
		}
		
		[Browsable(false)]
		public IEndRule ValidationRule
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException("Cannot set end rule to Workflow");
			}
		}

		[Browsable(false)]
		public IContextStore ValidationRuleContextStore
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException("Cannot set end rule to Workflow");
			}
		}

		[Browsable(false)]
		public string Roles
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException("Cannot set Roles to Workflow");
			}
		}		

		[Browsable(false)]
		public string Features
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException("Cannot set Features to Workflow");
			}
		}
        #endregion
    }
}
