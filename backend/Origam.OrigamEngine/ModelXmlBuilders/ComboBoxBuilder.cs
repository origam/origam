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
using System.Xml;
using System.Data;
using Origam.Schema;
using Origam.Workbench.Services;
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using Origam.DA.Service;

namespace Origam.OrigamEngine.ModelXmlBuilders
{
	/// <summary>
	/// Summary description for ComboBoxBuilder.
	/// </summary>
	public class ComboBoxBuilder
	{
		public static void Build(XmlElement propertyElement, Guid lookupId, bool showUniqueValues, string bindingMember, DataTable table)
		{
			propertyElement.SetAttribute("Entity", "String");
			propertyElement.SetAttribute("Column", "ComboBox");
			propertyElement.SetAttribute("DropDownShowUniqueValues", XmlConvert.ToString(showUniqueValues));

			BuildCommonDropdown(propertyElement, lookupId, bindingMember, table);
		}

		public static void BuildCommonDropdown(XmlElement propertyElement, Guid lookupId, string bindingMember, DataTable table)
		{
            if (lookupId == Guid.Empty)
            {
                throw new Exception("Lookup not set for a DropDown widget bound to the field " + table.TableName + "." + bindingMember);
            }
			bool useCache = true;
			IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
			DataServiceDataLookup lookup = persistence.SchemaProvider.RetrieveInstance(typeof(DataServiceDataLookup), new ModelElementKey(lookupId)) as DataServiceDataLookup;

			DatasetGenerator gen = new DatasetGenerator(true);
			DataSet comboListDataset = gen.CreateDataSet(lookup.ListDataStructure);
			DataTable comboListTable = comboListDataset.Tables[(lookup.ListDataStructure.ChildItemsByType(DataStructureEntity.CategoryConst)[0] as DataStructureEntity).Name];

			propertyElement.SetAttribute("LookupId", lookupId.ToString());
			propertyElement.SetAttribute("Identifier", lookup.ListValueMember);
			propertyElement.SetAttribute("IdentifierIndex", comboListTable.Columns[lookup.ListValueMember].Ordinal.ToString());
			propertyElement.SetAttribute("EntityName", lookupId.ToString().Replace("-", "_"));
			propertyElement.SetAttribute("IsTree", XmlConvert.ToString(lookup.IsTree));
			propertyElement.SetAttribute("AllowReturnToForm", XmlConvert.ToString(lookup.ListMethod == null || lookup.AlwaysAllowReturnToForm));
			propertyElement.SetAttribute("SearchByFirstColumnOnly", XmlConvert.ToString(lookup.SearchByFirstColumnOnly));
			propertyElement.SetAttribute("AutoSort", XmlConvert.ToString(lookup.ListSortSet == null));
			propertyElement.SetAttribute("SupportsServerSideSorting", 
				XmlConvert.ToString(lookup.ValueMethod is DataStructureFilterSet));

			string dropDownType = "EagerlyLoadedGrid";
			if(lookup.IsTree && lookup.IsFilteredServerside)
			{
				dropDownType = "LazilyLoadedTree";
			}
			else if(lookup.IsTree)
			{
				dropDownType = "EagerlyLoadedTree";
			}
			else if(lookup.IsFilteredServerside)
			{
				dropDownType = "LazilyLoadedGrid";
			}
			propertyElement.SetAttribute("DropDownType", dropDownType);

			if(lookup.HasTooltip)
			{
				propertyElement.SetAttribute("HasTooltip", XmlConvert.ToString(true));
			}

			if(lookup.IsTree)
			{
				propertyElement.SetAttribute("ParentIdProperty", lookup.TreeParentMember);
				propertyElement.SetAttribute("NameProperty", lookup.ListDisplayMember);
				propertyElement.SetAttribute("ParentIdPropertyIndex", 
					comboListTable.Columns[lookup.TreeParentMember].Ordinal.ToString());
				propertyElement.SetAttribute("NamePropertyIndex", 
					comboListTable.Columns[lookup.ListDisplayMember].Ordinal.ToString());
			}
			else
			{
				XmlElement comboListColumnsElement = propertyElement.OwnerDocument.CreateElement("DropDownColumns");
				propertyElement.AppendChild(comboListColumnsElement);

				foreach(string comboColumn in lookup.ListDisplayMember.Split(";".ToCharArray()))
				{
					DataColumn dataColumn = comboListTable.Columns[comboColumn];
					if(dataColumn == null)
					{
						throw new ArgumentOutOfRangeException("comboColumn", comboColumn,
							"Field not found for the dropdown list column. Lookup: " + lookup.Path);
					}

					OrigamDataType origamType = (OrigamDataType)dataColumn.ExtendedProperties["OrigamDataType"];

					XmlElement comboColumnElement = propertyElement.OwnerDocument.CreateElement("Property");
					comboListColumnsElement.AppendChild(comboColumnElement);

					string targetColumn = "Text";
					string targetEntity = "String";
					string formatPattern = "";

					switch(origamType)
					{
						case OrigamDataType.Date:
							targetEntity = "Date";
							targetColumn = "Date";
							formatPattern = "ddd dd. MM yyyy HH:mm";
							break;

						case OrigamDataType.Integer:
						case OrigamDataType.Long:
						case OrigamDataType.Float:
						case OrigamDataType.Currency:
							targetEntity = origamType.ToString();
							targetColumn = "Number";
							break;
                        case OrigamDataType.Boolean:
                            targetEntity = origamType.ToString();
                            targetColumn = "CheckBox";
                            break;
					}

					comboColumnElement.SetAttribute("Id", comboColumn);
					comboColumnElement.SetAttribute("Name", dataColumn.Caption);
					comboColumnElement.SetAttribute("Entity", targetEntity);
					comboColumnElement.SetAttribute("Column", targetColumn);
					if(formatPattern != "")	comboColumnElement.SetAttribute("FormatterPattern", formatPattern);
					comboColumnElement.SetAttribute("Index", dataColumn.Ordinal.ToString());
				}
			}

			if(table != null)
			{
				// set caching policy
				DataColumn column = table.Columns[bindingMember];
				if(column.ExtendedProperties.Contains("IsState") && (bool)column.ExtendedProperties["IsState"] == true)
				{
					useCache = false;
				}
			}
			
			var dataLookupService = ServiceManager.Services.GetService<DataLookupService>();
			NewRecordScreenBinding newRecordScreenBinding = dataLookupService.GetNewRecordScreenBinding(lookup);
			if (newRecordScreenBinding != null)
			{
				XmlElement newRecordElement = propertyElement.OwnerDocument.CreateElement("NewRecordScreen");
				newRecordElement.SetAttribute("Width", XmlConvert.ToString(newRecordScreenBinding.DialogWidth));
				newRecordElement.SetAttribute("Height", XmlConvert.ToString(newRecordScreenBinding.DialogHeight));
				newRecordElement.SetAttribute("MenuItemId", XmlConvert.ToString(newRecordScreenBinding.MenuItemId));
				propertyElement.AppendChild(newRecordElement);
				foreach (var parameterMapping 
				         in newRecordScreenBinding.GetParameterMappings())
				{
					XmlElement parameterMappingElement
						= newRecordElement.OwnerDocument.CreateElement(
							"ParameterMapping");
					parameterMappingElement.SetAttribute(
						"ParameterName", 
						parameterMapping.ParameterName);
					parameterMappingElement.SetAttribute(
						"TargetRootEntityField", 
						parameterMapping.TargetRootEntityField);
					newRecordElement.AppendChild(parameterMappingElement);
				}
			}

			propertyElement.SetAttribute("Cached", XmlConvert.ToString(useCache));
		}
	}
}
