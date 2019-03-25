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
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.RuleModel;
using Origam.Schema.EntityModel;

namespace Origam.Schema.WorkflowModel
{
    /// <summary>
    /// Summary description for AbstractWorkflowStep.
    /// </summary>
    [XmlModelRoot(ItemTypeConst)]
    public abstract class AbstractWorkflowStep : AbstractSchemaItem, IWorkflowStep
	{															
		public const string ItemTypeConst = "WorkflowTask";

		public AbstractWorkflowStep() : base() {Init();}

		public AbstractWorkflowStep(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}

		public AbstractWorkflowStep(Key primaryKey) : base(primaryKey)	{Init();}

		private void Init()
		{
			this.ChildItemTypes.Add(typeof(WorkflowTaskDependency));
		}

		#region Overriden AbstractSchemaItem Members
		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ItemTypeConst;
			}
		}

		[Browsable(false)] 
		public override abstract string Icon	{get;}

		public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
		{
			// can move inside the same workflow and we can move it under any block
			if(this.RootItem == (newNode as ISchemaItem).RootItem && 
				newNode is IWorkflowBlock)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			if(this.StartConditionRule != null) dependencies.Add(this.StartConditionRule);
			if(this.StartConditionRuleContextStore != null) dependencies.Add(this.StartConditionRuleContextStore);
			if(this.ValidationRule != null) dependencies.Add(this.ValidationRule);
			if(this.ValidationRuleContextStore != null) dependencies.Add(this.ValidationRuleContextStore);

			base.GetExtraDependencies (dependencies);
		}

		public override void UpdateReferences()
		{
			foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
			{
				if(item.OldPrimaryKey != null)
				{
					if(this.StartConditionRuleContextStore != null && item.OldPrimaryKey.Equals(this.StartConditionRuleContextStore.PrimaryKey))
					{
						this.StartConditionRuleContextStore = item as IContextStore;
					}
					if(this.ValidationRuleContextStore != null && item.OldPrimaryKey.Equals(this.ValidationRuleContextStore.PrimaryKey))
					{
						this.ValidationRuleContextStore = item as IContextStore;
					}
				}
			}

			base.UpdateReferences ();
		}

		#endregion

		#region IWorkflowStep Members
        [Browsable(false)]
		public ArrayList Dependencies
		{
			get
			{
				return this.ChildItemsByType(WorkflowTaskDependency.ItemTypeConst);
			}
		}


		[DefaultValue(WorkflowStepTraceLevel.InheritFromParent)]
		[Category("Tracing"), RefreshProperties(RefreshProperties.Repaint)]
		[EntityColumn("I01")]  
		[XmlAttribute ("traceLevel")]
		public WorkflowStepTraceLevel TraceLevel { get; set; } = WorkflowStepTraceLevel.InheritFromParent;

		[Category("Tracing")]

        public string ShowTrace
        {
            get
            {
                bool? trace = Trace;
                if (trace == null)
                {
                    return "Parent";
                }
                return trace.ToString();
            }
        }

        [Browsable(false)]
        public bool? Trace
		{
			get
			{
				#if ORIGAM_CLIENT
					if(this.TraceLevel == WorkflowStepTraceLevel.TraceClientAndArchitect) return true;
				#else
					if(this.TraceLevel == WorkflowStepTraceLevel.TraceClientAndArchitect |
						this.TraceLevel == WorkflowStepTraceLevel.TraceArchitect) return true;
				#endif

				if(this.TraceLevel == WorkflowStepTraceLevel.InheritFromParent)
				{
					// we check the first non-inherited parent

					IWorkflowStep parentStep = this.ParentItem as IWorkflowStep;
					while(parentStep != null)
					{
						if(parentStep.TraceLevel != WorkflowStepTraceLevel.InheritFromParent)
						{
							break;
						}
						if(parentStep.ParentItem as IWorkflowStep == null)
                        {
                            return null;
                        }
						parentStep = parentStep.ParentItem as IWorkflowStep;
					}

					if(parentStep != null)
					{
						#if ORIGAM_CLIENT
							if(parentStep.TraceLevel == WorkflowStepTraceLevel.TraceClientAndArchitect) return true;
						#else
							if(parentStep.TraceLevel == WorkflowStepTraceLevel.TraceClientAndArchitect |
								parentStep.TraceLevel == WorkflowStepTraceLevel.TraceArchitect) return true;
						#endif
					}
				}

				return false;
			}
		}

        [EntityColumn("G02")]  
		public Guid StartRuleId;

		[Category("Rules")]
		[TypeConverter(typeof(StartRuleConverter))]
        [NotNullModelElementRule("StartConditionRuleContextStore")]
		[XmlReference("startConditionRule", "StartRuleId")]
		public StartRule StartConditionRule
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.StartRuleId;

				return (StartRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.StartRuleId = Guid.Empty;
				}
				else
				{
					this.StartRuleId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}

		[EntityColumn("G01")]  
		public Guid StartRuleContextStoreId;

		[Category("Rules")]
		[TypeConverter(typeof(ContextStoreConverter))]
        [NotNullModelElementRule("StartConditionRule")]
		[XmlReference("startConditionRuleContextStore", "StartRuleContextStoreId")]
		public IContextStore StartConditionRuleContextStore
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.StartRuleContextStoreId;

				return (IContextStore)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.StartRuleContextStoreId = Guid.Empty;
				}
				else
				{
					this.StartRuleContextStoreId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}

		[EntityColumn("G04")]  
		public Guid ValidationRuleId;

		[Category("Rules")]
		[TypeConverter(typeof(EndRuleConverter))]
        [NotNullModelElementRule("ValidationRuleContextStore")]
		[XmlReference("validationRule", "ValidationRuleId")]
		public IEndRule ValidationRule
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.ValidationRuleId;

				return (IEndRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.ValidationRuleId = Guid.Empty;
				}
				else
				{
					this.ValidationRuleId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}

		[EntityColumn("G03")]  
		public Guid ValidationRuleContextStoreId;

		[Category("Rules")]
		[TypeConverter(typeof(ContextStoreConverter))]
        [NotNullModelElementRule("ValidationRule")]
		[XmlReference("validationRuleContextStore", "ValidationRuleContextStoreId")]
        public IContextStore ValidationRuleContextStore
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.ValidationRuleContextStoreId;

				return (IContextStore)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.ValidationRuleContextStoreId = Guid.Empty;
				}
				else
				{
					this.ValidationRuleContextStoreId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}

		[EntityColumn("SS02")]
		[Category("Rules")]
		[XmlAttribute ("roles")]
		public string Roles { get; set; } = "*";

		[EntityColumn("SS01")]
		[Category("Rules")]
		[XmlAttribute ("features")]
		public string Features { get; set; }
		#endregion

	}
}
