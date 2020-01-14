#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
	/// <summary>
	/// Summary description for GuiHelper.
	/// </summary>
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
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
			FormSchemaItemProvider formProvider = schema.GetProvider(typeof(FormSchemaItemProvider)) as FormSchemaItemProvider;

			SchemaItemGroup group = formProvider.GetGroup(groupName);

			FormControlSet form = formProvider.NewItem(typeof(FormControlSet), schema.ActiveSchemaExtensionId, null) as FormControlSet;
			form.Name = dataSource.Name;
			form.Group = group;
			form.DataStructure = dataSource;

			ControlSetItem rootControl = CreateControl(form, GetFormControl());

			Hashtable formProperties = new Hashtable();
			formProperties["Width"] = 700;
			formProperties["Height"] = 400;

			PopulateControlProperties(rootControl, formProperties);

			if(defaultPanel != null)
			{
				ControlSetItem panelControl = CreateControl(rootControl, defaultPanel.PanelControl);

				// clone the panel's properties
				foreach(PropertyValueItem originalProperty in defaultPanel.ChildItems[0].ChildItemsByType(PropertyValueItem.ItemTypeConst))
				{
					PropertyValueItem property = panelControl.NewItem(typeof(PropertyValueItem), schema.ActiveSchemaExtensionId, null) as PropertyValueItem;

					property.ControlPropertyItem = originalProperty.ControlPropertyItem;

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

		public static PanelControlSet CreatePanel(string groupName, IDataEntity entity, Hashtable fieldsToPopulate)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
			PanelSchemaItemProvider panelProvider = schema.GetProvider(typeof(PanelSchemaItemProvider)) as PanelSchemaItemProvider;

			SchemaItemGroup group = panelProvider.GetGroup(groupName);

			PanelControlSet panel = panelProvider.NewItem(typeof(PanelControlSet), schema.ActiveSchemaExtensionId, null) as PanelControlSet;
			panel.Name = entity.Name;
			panel.Group = group;
			panel.DataEntity = entity;

			ControlSetItem rootControl = CreateControl(panel, GetPanelControl());

			Hashtable panelProperties = new Hashtable();
			panelProperties["PanelTitle"] = entity.Caption;
			panelProperties["Width"] = 600;
			panelProperties["Height"] = 300;
			panelProperties["GridVisible"] = true;
			panelProperties["ShowNewButton"] = true;
			panelProperties["ShowDeleteButton"] = true;

			PopulateControlProperties(rootControl, panelProperties);

			int x = 108;
			int y = 36;
			int i = 0;

			foreach(IDataEntityColumn column in entity.EntityColumns)
			{
				if(fieldsToPopulate.Contains(column.Name))
				{
					BuildDefaultControl(rootControl, entity, column, x, y, i);
					y+= 18;
					i++;
				}
			}

			panel.Persist();

			CreatePanelControl(panel);

			return panel;
		}

		private static void BuildDefaultControl(ControlSetItem parentControl, IDataEntity entity, IDataEntityColumn column, int x, int y, int tabIndex)
		{
			Hashtable properties = new Hashtable();
			properties["Left"] = x;
			properties["Top"] = y;
			properties["Height"] = 19;
			properties["Width"] = 400;
			properties["Caption"] = "";
			properties["CaptionLength"] = 100;
			properties["TabIndex"] = tabIndex;

			switch(column.DataType)
			{
				case OrigamDataType.Integer:
				case OrigamDataType.Long:
				case OrigamDataType.Float:
				case OrigamDataType.Currency:
				case OrigamDataType.Memo:
				case OrigamDataType.Geography:
				case OrigamDataType.String:
					ControlSetItem textBox = CreateControl(parentControl, GetTextBoxControl());
					textBox.Name = textBox.ControlItem.Name + tabIndex.ToString();
					PopulateControlProperties(textBox, properties);
					PopulateControlBindings(textBox, entity.Name, column.Name, "Value");
					break;

				case OrigamDataType.Boolean:
					properties["Text"] = column.Caption;
					properties["Left"] = x - (int)properties["CaptionLength"]; //checkbox starts on the left
					ControlSetItem checkBox = CreateControl(parentControl, GetCheckBoxControl());
					checkBox.Name = checkBox.ControlItem.Name + tabIndex.ToString();
					PopulateControlProperties(checkBox, properties);
					PopulateControlBindings(checkBox, entity.Name, column.Name, "Value");
					break;

				case OrigamDataType.Date:
					ControlSetItem dateBox = CreateControl(parentControl, GetDateBoxControl());
					dateBox.Name = dateBox.ControlItem.Name + tabIndex.ToString();
					PopulateControlProperties(dateBox, properties);
					PopulateControlBindings(dateBox, entity.Name, column.Name, "DateValue");
					break;

				case OrigamDataType.UniqueIdentifier:
					if(column.DefaultLookup == null)
					{
						throw new Exception(ResourceUtils.GetString("ErrorLookupNotSet", entity.Name + "/" + column.Name));
					}

					properties["LookupId"] = column.DefaultLookup.PrimaryKey["Id"];
					ControlSetItem comboBox = CreateControl(parentControl, GetComboBoxControl());
					comboBox.Name = comboBox.ControlItem.Name + tabIndex.ToString();
					PopulateControlProperties(comboBox, properties);
					PopulateControlBindings(comboBox, entity.Name, column.Name, "LookupValue");
					break;

				case OrigamDataType.Object:
					ControlSetItem multiColumnAdapterFieldWrapper 
						= CreateControl(parentControl, GetMultiColumnAdapterFieldWrapperControl());
					multiColumnAdapterFieldWrapper.Name 
						= multiColumnAdapterFieldWrapper.ControlItem.Name + tabIndex.ToString();
					PopulateControlProperties(multiColumnAdapterFieldWrapper, properties);
					PopulateControlBindings(multiColumnAdapterFieldWrapper, 
						entity.Name, column.Name, "Value");
					break;


				default:
					throw new ArgumentOutOfRangeException("DataType", column.DataType, "Default control of this data type is not supported by the control builder.");
			}
		}

		public static ControlSetItem CreateControl(AbstractSchemaItem parentControl, ControlItem controlType)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
			ControlSetItem control = ((ControlSetItem)parentControl.NewItem(typeof(ControlSetItem), schema.ActiveSchemaExtensionId, null));
			control.ControlItem = controlType;

			return control;
		}

		public static ControlItem CreatePanelControl(PanelControlSet panel)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
			UserControlSchemaItemProvider controls = schema.GetProvider(typeof(UserControlSchemaItemProvider)) as UserControlSchemaItemProvider;

			ControlItem newControl = controls.NewItem(typeof(ControlItem), schema.ActiveSchemaExtensionId, null) as ControlItem;
			newControl.Name = panel.Name;
			newControl.IsComplexType = true;
			Type t = typeof(PanelControlSet);
			newControl.ControlType = t.ToString();
			newControl.ControlNamespace = t.Namespace;
			newControl.PanelControlSet = panel;
			newControl.ControlToolBoxVisibility = ControlToolBoxVisibility.FormDesigner;
			SchemaItemAncestor ancestor = new SchemaItemAncestor();
			ancestor.SchemaItem = newControl;
			ancestor.Ancestor = GetPanelControl();
			ancestor.PersistenceProvider = newControl.PersistenceProvider;

			newControl.ThrowEventOnPersist = false;
			newControl.Persist();
			ancestor.Persist();
			//				newControl.ClearCacheOnPersist = true;
			//				newControl.Persist();
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
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
			UserControlSchemaItemProvider controls = schema.GetProvider(typeof(UserControlSchemaItemProvider)) as UserControlSchemaItemProvider;

			return controls.GetChildByName(name, ControlItem.ItemTypeConst) as ControlItem;
		}

		private static void PopulateControlBindings(ControlSetItem control, string entity, string field, string property)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			PropertyBindingInfo binding = control.NewItem(typeof(PropertyBindingInfo), schema.ActiveSchemaExtensionId, null) as PropertyBindingInfo;

			binding.ControlPropertyItem = control.ControlItem.GetChildByName(property) as ControlPropertyItem;
			binding.Value = field;
			binding.DesignDataSetPath = entity + "." + field;
		}

		private static void PopulateControlProperties(ControlSetItem control, Hashtable properties)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			foreach(ControlPropertyItem propertyDef in control.ControlItem.ChildItemsByType(ControlPropertyItem.ItemTypeConst))
			{
				PropertyValueItem property = control.NewItem(typeof(PropertyValueItem), schema.ActiveSchemaExtensionId, null) as PropertyValueItem;

				property.ControlPropertyItem = propertyDef;
				
				if(properties.Contains(propertyDef.Name))
				{
					property.SetValue(properties[propertyDef.Name]);
				}
			}
		}
	}
}
