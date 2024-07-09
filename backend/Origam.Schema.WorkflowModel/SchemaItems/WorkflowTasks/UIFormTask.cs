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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.GuiModel;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Workbench.Services;
using Origam.Schema.RuleModel;

namespace Origam.Schema.WorkflowModel;
public enum TrueFalseEnum
{
    False,
    True
}
[SchemaItemDescription("(Task) User Interface", "Tasks", "task-user-interface.png")]
[HelpTopic("User+Interface+Task")]
[ClassMetaVersion("6.0.0")]
public class UIFormTask : WorkflowTask
{
	public UIFormTask() 
	{
		OutputMethod = ServiceOutputMethod.FullMerge;
	}
	public UIFormTask(Guid schemaExtensionId) : base(schemaExtensionId) 
	{
		OutputMethod = ServiceOutputMethod.FullMerge;
	}
	public UIFormTask(Key primaryKey) : base(primaryKey)
	{
		OutputMethod = ServiceOutputMethod.FullMerge;
	}
	#region Override ISchemaItem Members
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		dependencies.Add(Screen);
		if(RefreshDataStructure != null)
		{
			dependencies.Add(RefreshDataStructure);
		}
		if(RefreshMethod != null)
		{
			dependencies.Add(RefreshMethod);
		}
		if(RefreshSortSet != null)
		{
			dependencies.Add(RefreshSortSet);
		}
		if(SaveDataStructure != null)
		{
			dependencies.Add(SaveDataStructure);
		}
		if(SaveConfirmationRule != null)
		{
			dependencies.Add(SaveConfirmationRule);
		}
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
		get => (FormControlSet)PersistenceProvider.RetrieveInstance(
			typeof(ISchemaItem), new ModelElementKey(ScreenId));
		set => ScreenId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
	public Guid RefreshDataStructureId;
	[Category("Data Refresh Parameters")]
	[TypeConverter(typeof(DataStructureConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[XmlReference("refreshDataStructure", "RefreshDataStructureId")]
	public DataStructure RefreshDataStructure
	{
		get => (ISchemaItem)PersistenceProvider.RetrieveInstance(
			typeof(ISchemaItem), 
			new ModelElementKey(RefreshDataStructureId)) as DataStructure;
		set
		{
			if (value == null)
			{
				RefreshDataStructureId = Guid.Empty;
			}
			else
			{
				RefreshDataStructureId = (Guid)value.PrimaryKey["Id"];
			}		
			RefreshMethod = null;
			RefreshSortSet = null;
		}
	}
	
	public Guid RefreshMethodId;
	[TypeConverter(typeof(UIFormTaskMethodConverter))]
	[Category("Data Refresh Parameters")]
	[XmlReference("refreshMethod", "RefreshMethodId")]
	public DataStructureMethod RefreshMethod
	{
		get => (DataStructureMethod)PersistenceProvider.RetrieveInstance(
			typeof(ISchemaItem), 
			new ModelElementKey(RefreshMethodId));
		set => RefreshMethodId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
	
	public Guid RefreshSortSetId;
	[TypeConverter(typeof(UIFormTaskSortSetConverter))]
	[Category("Data Refresh Parameters")]
	[XmlReference("refreshSortSet", "RefreshSortSetId")]
	public DataStructureSortSet RefreshSortSet
	{
		get => (DataStructureSortSet)PersistenceProvider.RetrieveInstance(
			typeof(ISchemaItem), 
			new ModelElementKey(RefreshSortSetId));
		set => RefreshSortSetId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}
	
	public Guid SaveDataStructureId;
	[Category("Save Parameters")]
	[TypeConverter(typeof(DataStructureConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[XmlReference("saveDataStructure", "SaveDataStructureId")]
	public DataStructure SaveDataStructure
	{
		get => (ISchemaItem)PersistenceProvider.RetrieveInstance(
			typeof(ISchemaItem), 
			new ModelElementKey(SaveDataStructureId)) as DataStructure;
		set => SaveDataStructureId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
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
		get => (IEndRule)PersistenceProvider.RetrieveInstance(
			typeof(ISchemaItem), 
			new ModelElementKey(SaveConfirmationRuleId));
		set => SaveConfirmationRuleId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
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
    public TrueFalseEnum RefreshPortalAfterSave { get; set; } 
        = TrueFalseEnum.False;
	public ArrayList RefreshParameters
	{
		get
		{
			var result = new ArrayList();
			foreach(var item in ChildItems)
			{
				if((item is ContextReference) 
				|| (item is DataConstantReference)
				|| (item is SystemFunctionCall))
				{
					result.Add(item);
				}
			}
			return result;
		}
	}
	#endregion
	#region ISchemaItemFactory Members
	public override Type[] NewItemTypes => new[] 
	{
		typeof(WorkflowTaskDependency),
		typeof(ContextReference),
		typeof(DataConstantReference),
		typeof(SystemFunctionCall)
	};
	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
		string itemName = null;
		if(typeof(T) == typeof(ContextReference))
		{
			itemName = "NewContextReference";
		}
		else if(typeof(T) == typeof(DataConstantReference))
		{
			itemName = "NewDataConstantReference";
		}
		else if(typeof(T) == typeof(SystemFunctionCall))
		{
			itemName = "NewSystemFunctionCall";
		}
		else if(typeof(T) == typeof(WorkflowTaskDependency))
		{
			itemName = "NewWorkflowTaskDependency";
		}
		return base.NewItem<T>(schemaExtensionId, group, itemName);
	}
	#endregion
}
