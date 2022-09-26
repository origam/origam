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
using Origam.Services;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.Schema.GuiModel
{
	public class GuiHelper
	{
		public const string CONTROL_NAME_PANEL = "AsPanel";
		public const string CONTROL_NAME_TEXTBOX = "AsTextBox";
		public const string CONTROL_NAME_COMBOBOX = "AsCombo";
		public const string CONTROL_NAME_CHECKBOX = "AsCheckBox";
		public const string CONTROL_NAME_DATEBOX = "AsDateBox";
		public const string CONTROL_NAME_FORM = "AsForm";
		public const string CONTROL_NAME_MULTICOLUMNADAPTERFIELD = "MultiColumnAdapterFieldWrapper";

		public static FormControlSet CreateForm(DataStructure dataSource, string groupName, PanelControlSet defaultPanel)
		{
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			var formSchemaItemProvider 
				= schemaService.GetProvider<FormSchemaItemProvider>();
			var schemaItemGroup = formSchemaItemProvider.GetGroup(groupName);
			var form = formSchemaItemProvider.NewItem<FormControlSet>(
				schemaService.ActiveSchemaExtensionId, null);
			form.Name = dataSource.Name;
			form.Group = schemaItemGroup;
			form.DataStructure = dataSource;
			var rootControl = CreateControl(form, GetFormControl());
			var formProperties = new Hashtable
			{
				["Width"] = 700,
				["Height"] = 400
			};
			PopulateControlProperties(rootControl, formProperties);
			if(defaultPanel != null)
			{
				var panelControl = CreateControl(rootControl, 
					defaultPanel.PanelControl);
				// clone the panel's properties
				foreach(PropertyValueItem originalProperty 
				        in defaultPanel.ChildItems[0].ChildItemsByType(
					        PropertyValueItem.CategoryConst))
				{
					var property = panelControl.NewItem<PropertyValueItem>(
						schemaService.ActiveSchemaExtensionId, null);
					property.ControlPropertyItem 
						= originalProperty.ControlPropertyItem;
					if(property.ControlPropertyItem.Name == "DataMember")
					{
						property.Value = defaultPanel.DataEntity.Name;
					}
					else
					{
						property.IntValue = originalProperty.IntValue;
						property.BoolValue = originalProperty.BoolValue;
						property.Value = originalProperty.Value;
						property.GuidValue = originalProperty.GuidValue;
					}
				}
			}
			form.Persist();
			return form;
		}

        public static PanelControlSet CreatePanel(
	        string groupName, IDataEntity entity, Hashtable fieldsToPopulate)
        {
            return CreatePanel(groupName, entity, fieldsToPopulate,
	            entity.Name);
        }

        public static PanelControlSet CreatePanel(
	        string groupName, 
	        IDataEntity entity, 
	        Hashtable fieldsToPopulate,
	        string name)
		{
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			var panelSchemaItemProvider 
				= schemaService.GetProvider<PanelSchemaItemProvider>();
			var schemaItemGroup = panelSchemaItemProvider.GetGroup(groupName);
			var panel = panelSchemaItemProvider.NewItem<PanelControlSet>(
				schemaService.ActiveSchemaExtensionId, null);
			panel.Name = name;
			panel.Group = schemaItemGroup;
			panel.DataEntity = entity;
			var rootControl = CreateControl(panel, GetPanelControl());
			var panelProperties = new Hashtable
			{
				["PanelTitle"] = entity.Caption,
				["Width"] = 600,
				["Height"] = 300,
				["GridVisible"] = true,
				["ShowNewButton"] = true,
				["ShowDeleteButton"] = true
			};
			PopulateControlProperties(rootControl, panelProperties);
			var x = 108;
			var y = 36;
			var i = 0;
			foreach(IDataEntityColumn column in entity.EntityColumns)
			{
				if(!fieldsToPopulate.Contains(column.Name))
				{
					continue;
				}
				BuildDefaultControl(rootControl, entity, column, x, y, i);
				y+= 18;
				i++;
			}
			panel.Persist();
			CreatePanelControl(panel);
			return panel;
		}

		private static void BuildDefaultControl(
			ControlSetItem parentControl, 
			IDataEntity entity, 
			IDataEntityColumn column, 
			int x, 
			int y, 
			int tabIndex)
		{
			var properties = new Hashtable
			{
				["Left"] = x,
				["Top"] = y,
				["Height"] = 19,
				["Width"] = 400,
				["Caption"] = "",
				["CaptionLength"] = 100,
				["TabIndex"] = tabIndex
			};
			switch(column.DataType)
			{
				case OrigamDataType.Integer:
				case OrigamDataType.Long:
				case OrigamDataType.Float:
				case OrigamDataType.Currency:
				case OrigamDataType.Memo:
				case OrigamDataType.Geography:
				case OrigamDataType.String:
				{
					var textBox = CreateControl(
						parentControl, GetTextBoxControl());
					textBox.Name = textBox.ControlItem.Name + tabIndex;
					PopulateControlProperties(textBox, properties);
					PopulateControlBindings(
						textBox, entity.Name, column.Name, "Value");
					break;
				}
				case OrigamDataType.Boolean:
				{
					properties["Text"] = column.Caption;
					properties["Left"] = x - (int)properties["CaptionLength"]; //checkbox starts on the left
					var checkBox = CreateControl(
						parentControl, GetCheckBoxControl());
					checkBox.Name = checkBox.ControlItem.Name + tabIndex;
					PopulateControlProperties(checkBox, properties);
					PopulateControlBindings(
						checkBox, entity.Name, column.Name, "Value");
					break;
				}
				case OrigamDataType.Date:
				{
					var dateBox = CreateControl(
						parentControl, GetDateBoxControl());
					dateBox.Name = dateBox.ControlItem.Name + tabIndex;
					PopulateControlProperties(dateBox, properties);
					PopulateControlBindings(
						dateBox, entity.Name, column.Name, "DateValue");
					break;
				}
				case OrigamDataType.UniqueIdentifier:
				{
					if(column.DefaultLookup == null)
					{
						throw new Exception(ResourceUtils.GetString(
							"ErrorLookupNotSet", 
							entity.Name + "/" + column.Name));
					}
					properties["LookupId"] 
						= column.DefaultLookup.PrimaryKey["Id"];
					var comboBox = CreateControl(
						parentControl, GetComboBoxControl());
					comboBox.Name = comboBox.ControlItem.Name + tabIndex;
					PopulateControlProperties(comboBox, properties);
					PopulateControlBindings(
						comboBox, entity.Name, column.Name, "LookupValue");
					break;
				}
				case OrigamDataType.Object:
				{
					var multiColumnAdapterFieldWrapper = CreateControl(
						parentControl, 
						GetMultiColumnAdapterFieldWrapperControl());
					multiColumnAdapterFieldWrapper.Name 
						= multiColumnAdapterFieldWrapper.ControlItem.Name 
						  + tabIndex;
					PopulateControlProperties(
						multiColumnAdapterFieldWrapper, properties);
					PopulateControlBindings(multiColumnAdapterFieldWrapper, 
						entity.Name, column.Name, "Value");
					break;
				}
				default:
					throw new ArgumentOutOfRangeException("DataType", 
						column.DataType, 
						"Default control of this data type is not supported by the control builder.");
			}
		}

		public static ControlSetItem CreateControl(
			AbstractSchemaItem parentControl, 
			ControlItem controlType)
		{
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			var control = parentControl.NewItem<ControlSetItem>(
				schemaService.ActiveSchemaExtensionId, null);
			control.ControlItem = controlType;
			return control;
		}

		public static ControlItem CreatePanelControl(PanelControlSet panel)
		{
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			var userControlSchemaItemProvider 
				= schemaService.GetProvider<UserControlSchemaItemProvider>();
			var newControl = userControlSchemaItemProvider.NewItem<ControlItem>(
				schemaService.ActiveSchemaExtensionId, null);
			newControl.Name = panel.Name;
			newControl.IsComplexType = true;
			var panelControlSetType = typeof(PanelControlSet);
			newControl.ControlType = panelControlSetType.ToString();
			newControl.ControlNamespace = panelControlSetType.Namespace;
			newControl.PanelControlSet = panel;
			newControl.ControlToolBoxVisibility 
				= ControlToolBoxVisibility.FormDesigner;
			var ancestor = new SchemaItemAncestor();
			ancestor.SchemaItem = newControl;
			ancestor.Ancestor = GetPanelControl();
			ancestor.PersistenceProvider = newControl.PersistenceProvider;
			newControl.ThrowEventOnPersist = false;
			newControl.Persist();
			ancestor.Persist();
			newControl.ThrowEventOnPersist = true;
			return newControl;
		}

		public static ControlItem GetPanelControl()
		{
			return GetControlByName(CONTROL_NAME_PANEL);
		}

		public static ControlItem GetFormControl()
		{
			return GetControlByName(CONTROL_NAME_FORM);
		}

		public static ControlItem GetTextBoxControl()
		{
			return GetControlByName(CONTROL_NAME_TEXTBOX);
		}

		public static ControlItem GetCheckBoxControl()
		{
			return GetControlByName(CONTROL_NAME_CHECKBOX);
		}

		public static ControlItem GetDateBoxControl()
		{
			return GetControlByName(CONTROL_NAME_DATEBOX);
		}

		public static ControlItem GetMultiColumnAdapterFieldWrapperControl()
		{
			return GetControlByName(CONTROL_NAME_MULTICOLUMNADAPTERFIELD);
		}
		
		public static ControlItem GetComboBoxControl()
		{
			return GetControlByName(CONTROL_NAME_COMBOBOX);
		}
		
		public static ControlItem GetControlByName(string name)
		{
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			var userControlSchemaItemProvider 
				= schemaService.GetProvider<UserControlSchemaItemProvider>();
			return userControlSchemaItemProvider.GetChildByName(
				name, ControlItem.CategoryConst) as ControlItem;
		}

		private static void PopulateControlBindings(ControlSetItem control, string entity, string field, string property)
		{
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			var propertyBindingInfo = control.NewItem<PropertyBindingInfo>(
				schemaService.ActiveSchemaExtensionId, null);
			propertyBindingInfo.ControlPropertyItem 
				= control.ControlItem.GetChildByName(property) 
					as ControlPropertyItem;
			propertyBindingInfo.Value = field;
			propertyBindingInfo.DesignDataSetPath = entity + "." + field;
		}

		private static void PopulateControlProperties(
			ControlSetItem control, Hashtable properties)
		{
			var schema 
				= ServiceManager.Services.GetService<ISchemaService>();
			foreach(ControlPropertyItem propertyDef 
			        in control.ControlItem.ChildItemsByType(
				        ControlPropertyItem.CategoryConst))
			{
				var propertyValueItem = control.NewItem<PropertyValueItem>(
					schema.ActiveSchemaExtensionId, null);
				propertyValueItem.ControlPropertyItem = propertyDef;
				if(properties.Contains(propertyDef.Name))
				{
					propertyValueItem.SetValue(properties[propertyDef.Name]);
				}
			}
		}
	}
}
