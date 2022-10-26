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

using Origam.DA.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.GuiModel;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using Origam.Schema.RuleModel;

namespace Origam.Schema.WorkflowModel
{
    public enum TrueFalseEnum
    {
        False,
        True
    }

	/// <summary>
	/// Summary description for UIFormTask.
	/// </summary>
	[SchemaItemDescription("(Task) User Interface", "Tasks", "task-user-interface.png")]
    [HelpTopic("User+Interface+Task")]
    [ClassMetaVersion("6.0.0")]
	public class UIFormTask : WorkflowTask, ISchemaItemFactory
	{
		public UIFormTask() : base()
		{
			this.OutputMethod = ServiceOutputMethod.FullMerge;
		}

		public UIFormTask(Guid schemaExtensionId) : base(schemaExtensionId) 
		{
			this.OutputMethod = ServiceOutputMethod.FullMerge;
		}

		public UIFormTask(Key primaryKey) : base(primaryKey)
		{
			this.OutputMethod = ServiceOutputMethod.FullMerge;
		}

		#region Override AbstractSchemaItem Members
		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Screen);
			if(this.RefreshDataStructure != null)
				dependencies.Add(this.RefreshDataStructure);
			if(this.RefreshMethod != null)
				dependencies.Add(this.RefreshMethod);
			if(this.RefreshSortSet != null)
				dependencies.Add(this.RefreshSortSet);
			if(this.SaveDataStructure != null)
				dependencies.Add(this.SaveDataStructure);
			if (this.SaveConfirmationRule != null)
				dependencies.Add(this.SaveConfirmationRule);


			base.GetExtraDependencies (dependencies);
		}
		#endregion

		#region Properties
		public Guid ScreenId;

		[TypeConverter(typeof(FormControlSetConverter))]
		[NotNullModelElementRule()]
		[XmlReference("screen", "ScreenId")]
		public FormControlSet Screen
		{
			get
			{
				return (FormControlSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ScreenId));
			}
			set
			{
				this.ScreenId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid RefreshDataStructureId;

		[Category("Data Refresh Parameters")]
		[TypeConverter(typeof(DataStructureConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("refreshDataStructure", "RefreshDataStructureId")]
		public DataStructure RefreshDataStructure
		{
			get
			{
				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.RefreshDataStructureId)) as DataStructure;
			}
			set
			{
				if (value == null)
				{
					this.RefreshDataStructureId = Guid.Empty;
				}
				else
				{
					this.RefreshDataStructureId = (Guid)value.PrimaryKey["Id"];
				}		

				this.RefreshMethod = null;
				this.RefreshSortSet = null;
			}
		}
		
		public Guid RefreshMethodId;

		[TypeConverter(typeof(UIFormTaskMethodConverter))]
		[Category("Data Refresh Parameters")]
		[XmlReference("refreshMethod", "RefreshMethodId")]
		public DataStructureMethod RefreshMethod
		{
			get
			{
				return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(RefreshMethodId));
			}
			set
			{
				this.RefreshMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid RefreshSortSetId;

		[TypeConverter(typeof(UIFormTaskSortSetConverter))]
		[Category("Data Refresh Parameters")]
		[XmlReference("refreshSortSet", "RefreshSortSetId")]
		public DataStructureSortSet RefreshSortSet
		{
			get
			{
				return (DataStructureSortSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.RefreshSortSetId));
			}
			set
			{
				this.RefreshSortSetId = (value == null? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid SaveDataStructureId;

		[Category("Save Parameters")]
		[TypeConverter(typeof(DataStructureConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("saveDataStructure", "SaveDataStructureId")]
		public DataStructure SaveDataStructure
		{
			get
			{
				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.SaveDataStructureId)) as DataStructure;
			}
			set
			{
				this.SaveDataStructureId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		[DefaultValue(false)]
		[XmlAttribute ("isFinalForm")]
		public bool IsFinalForm { get; set; } = false;
		
		[DefaultValue(false)]
		[Category("Save Parameters")]
		[XmlAttribute ("allowSave")]
		public bool AllowSave { get; set; } = false;
		
		public Guid SaveConfirmationRuleId;

		[Category("Save Parameters")]
		[TypeConverter(typeof(EndRuleConverter))]
		[XmlReference("saveConfirmationRule", "SaveConfirmationRuleId")]
		public IEndRule SaveConfirmationRule
		{
			get
			{
				return (IEndRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.SaveConfirmationRuleId));
			}
			set
			{
				this.SaveConfirmationRuleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		[DefaultValue(false)]
		[XmlAttribute ("autoNext")]
		public bool AutoNext { get; set; } = false;
		
		[DefaultValue(true)]
		[XmlAttribute ("isRefreshSuppressedBeforeFirstSave")]
		public bool IsRefreshSuppressedBeforeFirstSave { get; set; } = true;

		[DefaultValue(TrueFalseEnum.False)]
		[Description("If true, the client will refresh its menu after saving data.")]
		[XmlAttribute ("refreshPortalAfterSave")]
        public TrueFalseEnum RefreshPortalAfterSave { get; set; } = TrueFalseEnum.False;

		public ArrayList RefreshParameters
		{
			get
			{
				ArrayList result = new ArrayList();

				foreach(AbstractSchemaItem item in this.ChildItems)
				{
					if(item is ContextReference || item is DataConstantReference
						|| item is SystemFunctionCall)
					{
						result.Add(item);
					}
				}

				return result;
			}
		}
		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {
									  typeof(WorkflowTaskDependency),
									  typeof(ContextReference),
									  typeof(DataConstantReference),
									  typeof(SystemFunctionCall)
								  };
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(ContextReference))
			{
				item = new ContextReference(schemaExtensionId);
				item.Name = "NewContextReference";
			}
			else if(type == typeof(DataConstantReference))
			{
				item = new DataConstantReference(schemaExtensionId);
				item.Name = "NewDataConstantReference";
			}
			else if(type == typeof(SystemFunctionCall))
			{
				item = new SystemFunctionCall(schemaExtensionId);
				item.Name = "NewSystemFunctionCall";
			}
			else if(type == typeof(WorkflowTaskDependency))
			{
				item = new WorkflowTaskDependency(schemaExtensionId);
				item.Name = "NewWorkflowTaskDependency";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorUIFormTaskUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}
		#endregion
	}
}
