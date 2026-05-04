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
using System.Data;
using System.Xml;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using Origam.Workbench.Services;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for ComboBoxBuilder.
/// </summary>
public class ComboBoxBuilder
{
    public static void Build(
        XmlElement propertyElement,
        Guid lookupId,
        bool showUniqueValues,
        string bindingMember,
        DataTable table
    )
    {
        propertyElement.SetAttribute(name: "Entity", value: "String");
        propertyElement.SetAttribute(name: "Column", value: "ComboBox");
        propertyElement.SetAttribute(
            name: "DropDownShowUniqueValues",
            value: XmlConvert.ToString(value: showUniqueValues)
        );
        BuildCommonDropdown(
            propertyElement: propertyElement,
            lookupId: lookupId,
            bindingMember: bindingMember,
            table: table
        );
    }

    public static void BuildCommonDropdown(
        XmlElement propertyElement,
        Guid lookupId,
        string bindingMember,
        DataTable table
    )
    {
        if (lookupId == Guid.Empty)
        {
            throw new Exception(
                message: "Lookup not set for a DropDown widget bound to the field "
                    + table.TableName
                    + "."
                    + bindingMember
            );
        }
        bool useCache = true;
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        DataServiceDataLookup lookup =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(DataServiceDataLookup),
                primaryKey: new ModelElementKey(id: lookupId)
            ) as DataServiceDataLookup;
        DatasetGenerator gen = new DatasetGenerator(userDefinedParameters: true);
        DataSet comboListDataset = gen.CreateDataSet(ds: lookup.ListDataStructure);
        DataTable comboListTable = comboListDataset.Tables[
            name: (
                lookup.ListDataStructure.ChildItemsByType<DataStructureEntity>(
                    itemType: DataStructureEntity.CategoryConst
                )[index: 0]
            ).Name
        ];
        propertyElement.SetAttribute(name: "LookupId", value: lookupId.ToString());
        propertyElement.SetAttribute(name: "Identifier", value: lookup.ListValueMember);
        propertyElement.SetAttribute(
            name: "IdentifierIndex",
            value: comboListTable.Columns[name: lookup.ListValueMember].Ordinal.ToString()
        );
        propertyElement.SetAttribute(
            name: "EntityName",
            value: lookupId.ToString().Replace(oldValue: "-", newValue: "_")
        );
        propertyElement.SetAttribute(
            name: "IsTree",
            value: XmlConvert.ToString(value: lookup.IsTree)
        );
        propertyElement.SetAttribute(
            name: "AllowReturnToForm",
            value: XmlConvert.ToString(
                value: lookup.ListMethod == null || lookup.AlwaysAllowReturnToForm
            )
        );
        propertyElement.SetAttribute(
            name: "SearchByFirstColumnOnly",
            value: XmlConvert.ToString(value: lookup.SearchByFirstColumnOnly)
        );
        propertyElement.SetAttribute(
            name: "AutoSort",
            value: XmlConvert.ToString(value: lookup.ListSortSet == null)
        );
        propertyElement.SetAttribute(
            name: "SupportsServerSideSorting",
            value: XmlConvert.ToString(value: lookup.ValueMethod is DataStructureFilterSet)
        );
        string dropDownType = "EagerlyLoadedGrid";
        if (lookup.IsTree && lookup.IsFilteredServerside)
        {
            dropDownType = "LazilyLoadedTree";
        }
        else if (lookup.IsTree)
        {
            dropDownType = "EagerlyLoadedTree";
        }
        else if (lookup.IsFilteredServerside)
        {
            dropDownType = "LazilyLoadedGrid";
        }
        propertyElement.SetAttribute(name: "DropDownType", value: dropDownType);
        if (lookup.HasTooltip)
        {
            propertyElement.SetAttribute(
                name: "HasTooltip",
                value: XmlConvert.ToString(value: true)
            );
        }
        if (lookup.IsTree)
        {
            propertyElement.SetAttribute(name: "ParentIdProperty", value: lookup.TreeParentMember);
            propertyElement.SetAttribute(name: "NameProperty", value: lookup.ListDisplayMember);
            propertyElement.SetAttribute(
                name: "ParentIdPropertyIndex",
                value: comboListTable.Columns[name: lookup.TreeParentMember].Ordinal.ToString()
            );
            propertyElement.SetAttribute(
                name: "NamePropertyIndex",
                value: comboListTable.Columns[name: lookup.ListDisplayMember].Ordinal.ToString()
            );
        }
        else
        {
            XmlElement comboListColumnsElement = propertyElement.OwnerDocument.CreateElement(
                name: "DropDownColumns"
            );
            propertyElement.AppendChild(newChild: comboListColumnsElement);
            foreach (
                string comboColumn in lookup.ListDisplayMember.Split(separator: ";".ToCharArray())
            )
            {
                DataColumn dataColumn = comboListTable.Columns[name: comboColumn];
                if (dataColumn == null)
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "comboColumn",
                        actualValue: comboColumn,
                        message: "Field not found for the dropdown list column. Lookup: "
                            + lookup.Path
                    );
                }
                OrigamDataType origamType = (OrigamDataType)
                    dataColumn.ExtendedProperties[key: "OrigamDataType"];
                XmlElement comboColumnElement = propertyElement.OwnerDocument.CreateElement(
                    name: "Property"
                );
                comboListColumnsElement.AppendChild(newChild: comboColumnElement);
                string targetColumn = "Text";
                string targetEntity = "String";
                string formatPattern = "";
                switch (origamType)
                {
                    case OrigamDataType.Date:
                    {
                        targetEntity = "Date";
                        targetColumn = "Date";
                        formatPattern = "ddd dd. MM yyyy HH:mm";
                        break;
                    }

                    case OrigamDataType.Integer:
                    case OrigamDataType.Long:
                    case OrigamDataType.Float:
                    case OrigamDataType.Currency:
                    {
                        targetEntity = origamType.ToString();
                        targetColumn = "Number";
                        break;
                    }

                    case OrigamDataType.Boolean:
                    {
                        targetEntity = origamType.ToString();
                        targetColumn = "CheckBox";
                        break;
                    }
                }
                comboColumnElement.SetAttribute(name: "Id", value: comboColumn);
                comboColumnElement.SetAttribute(name: "Name", value: dataColumn.Caption);
                comboColumnElement.SetAttribute(name: "Entity", value: targetEntity);
                comboColumnElement.SetAttribute(name: "Column", value: targetColumn);
                if (formatPattern != "")
                {
                    comboColumnElement.SetAttribute(name: "FormatterPattern", value: formatPattern);
                }

                comboColumnElement.SetAttribute(
                    name: "Index",
                    value: dataColumn.Ordinal.ToString()
                );
            }
        }
        if (table != null)
        {
            // set caching policy
            DataColumn column = table.Columns[name: bindingMember];
            if (
                column.ExtendedProperties.Contains(key: "IsState")
                && (bool)column.ExtendedProperties[key: "IsState"] == true
            )
            {
                useCache = false;
            }
        }

        var dataLookupService = ServiceManager.Services.GetService<DataLookupService>();
        NewRecordScreenBinding newRecordScreenBinding = dataLookupService.GetNewRecordScreenBinding(
            lookup: lookup
        );
        if (newRecordScreenBinding != null)
        {
            XmlElement newRecordElement = propertyElement.OwnerDocument.CreateElement(
                name: "NewRecordScreen"
            );
            newRecordElement.SetAttribute(
                name: "Width",
                value: XmlConvert.ToString(value: newRecordScreenBinding.DialogWidth)
            );
            newRecordElement.SetAttribute(
                name: "Height",
                value: XmlConvert.ToString(value: newRecordScreenBinding.DialogHeight)
            );
            newRecordElement.SetAttribute(
                name: "MenuItemId",
                value: XmlConvert.ToString(value: newRecordScreenBinding.MenuItemId)
            );
            propertyElement.AppendChild(newChild: newRecordElement);
            foreach (var parameterMapping in newRecordScreenBinding.GetParameterMappings())
            {
                XmlElement parameterMappingElement = newRecordElement.OwnerDocument.CreateElement(
                    name: "ParameterMapping"
                );
                parameterMappingElement.SetAttribute(
                    name: "ParameterName",
                    value: parameterMapping.ParameterName
                );
                parameterMappingElement.SetAttribute(
                    name: "TargetRootEntityField",
                    value: parameterMapping.TargetRootEntityField
                );
                newRecordElement.AppendChild(newChild: parameterMappingElement);
            }
        }
        propertyElement.SetAttribute(name: "Cached", value: XmlConvert.ToString(value: useCache));
    }
}
