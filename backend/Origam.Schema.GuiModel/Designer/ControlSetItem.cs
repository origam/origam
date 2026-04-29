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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.GuiModel;

[SchemaItemDescription(
    name: "Alternative",
    folderName: "Alternatives",
    iconName: "icon_alternative.png"
)]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class ControlSetItem : AbstractSchemaItem
{
    public const string CategoryConst = "ControlSetItem";

    public ControlSetItem() { }

    public ControlSetItem(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public ControlSetItem(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public Guid ControlId;

    [XmlReference(attributeName: "widget", idField: "ControlId")]
    public ControlItem ControlItem
    {
        get =>
            (ControlItem)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ControlItem),
                    primaryKey: new ModelElementKey(id: ControlId)
                );
        set => ControlId = (Guid)value.PrimaryKey[key: "Id"];
    }

    private string _roles;

    [XmlAttribute(attributeName: "roles")]
    public string Roles
    {
        get => _roles;
        set => _roles = value;
    }
    private string _features;

    [XmlAttribute(attributeName: "features")]
    public string Features
    {
        get => _features;
        set => _features = value;
    }
    private Guid _multiColumnAdapterFieldCondition;

    [XmlAttribute(attributeName: "multiColumnAdapterFieldCondition")]
    public Guid MultiColumnAdapterFieldCondition
    {
        get => _multiColumnAdapterFieldCondition;
        set => _multiColumnAdapterFieldCondition = value;
    }
    private bool _isAlternative = false;

    [XmlAttribute(attributeName: "isAlternative")]
    public bool IsAlternative
    {
        get => _isAlternative;
        set => _isAlternative = value;
    }
    private bool _requestSaveAfterChange = false;

    [XmlAttribute(attributeName: "requestSaveAfterChange")]
    public bool RequestSaveAfterChange
    {
        get => _requestSaveAfterChange;
        set => _requestSaveAfterChange = value;
    }
    private int _level = 100;

    [XmlAttribute(attributeName: "level")]
    public int Level
    {
        get => _level;
        set => _level = value;
    }
    #endregion

    #region Overriden ISchemaItem Members
    public override string ItemType => CategoryConst;

    public override UI.BrowserNodeCollection ChildNodes()
    {
        // return only the 1st level of items (alternative screen/panels) but not child widgets
        return ParentItem.ParentItem == null ? new UI.BrowserNodeCollection() : base.ChildNodes();
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: ControlItem);
        if (ControlItem.PanelControlSet != null)
        {
            dependencies.Add(item: ControlItem.PanelControlSet);
        }
        var lookupId = Guid.Empty;
        var reportId = Guid.Empty;
        var graphicsId = Guid.Empty;
        var workflowId = Guid.Empty;
        var constantId = Guid.Empty;
        foreach (
            var property in ChildItemsByType<PropertyValueItem>(
                itemType: PropertyValueItem.CategoryConst
            )
        )
        {
            if (property.ControlPropertyItem == null)
            {
                continue;
            }
            if (
                (ControlItem.Name == "AsCombo") && (property.ControlPropertyItem.Name == "LookupId")
            )
            {
                lookupId = property.GuidValue;
            }
            if (
                (ControlItem.Name == "RadioButton")
                && (property.ControlPropertyItem.Name == "DataConstantId")
            )
            {
                constantId = property.GuidValue;
            }
            if (
                (ControlItem.Name == "AsReportPanel")
                && (property.ControlPropertyItem.Name == "ReportId")
            )
            {
                reportId = property.GuidValue;
            }
            if (
                ((ControlItem.Name == "AsPanel") || ControlItem.IsComplexType)
                && (property.ControlPropertyItem.Name == "IconId")
            )
            {
                graphicsId = property.GuidValue;
            }
            if (
                (ControlItem.Name == "ExecuteWorkflowButton")
                && (property.ControlPropertyItem.Name == "IconId")
            )
            {
                graphicsId = property.GuidValue;
            }
            if (
                (ControlItem.Name == "ExecuteWorkflowButton")
                && (property.ControlPropertyItem.Name == "WorkflowId")
            )
            {
                workflowId = property.GuidValue;
            }
        }
        if (lookupId != Guid.Empty)
        {
            try
            {
                var item =
                    PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: lookupId)
                    ) as ISchemaItem;
                dependencies.Add(item: item);
            }
            catch
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "lookupId",
                    actualValue: lookupId,
                    message: ResourceUtils.GetString(
                        key: "ErrorLookupNotFound",
                        args: new object[] { Name, RootItem.ItemType, RootItem.Name }
                    )
                );
            }
        }
        if (constantId != Guid.Empty)
        {
            try
            {
                var item =
                    PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: constantId)
                    ) as ISchemaItem;
                dependencies.Add(item: item);
            }
            catch
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "constantId",
                    actualValue: lookupId,
                    message: ResourceUtils.GetString(
                        key: "ErrorConstantNotFound",
                        args: new object[] { Name, RootItem.ItemType, RootItem.Name }
                    )
                );
            }
        }
        if (reportId != Guid.Empty)
        {
            try
            {
                var item =
                    PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: reportId)
                    ) as ISchemaItem;
                dependencies.Add(item: item);
            }
            catch
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "reportId",
                    actualValue: reportId,
                    message: ResourceUtils.GetString(
                        key: "ErrorReportNotFound",
                        args: new object[] { Name, RootItem.ItemType, RootItem.Name }
                    )
                );
            }
        }
        if (graphicsId != Guid.Empty)
        {
            try
            {
                var item =
                    PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: graphicsId)
                    ) as ISchemaItem;
                dependencies.Add(item: item);
            }
            catch
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "graphicsId",
                    actualValue: graphicsId,
                    message: ResourceUtils.GetString(
                        key: "ErrorGraphicsNotFound",
                        args: new object[] { Name, RootItem.ItemType, RootItem.Name }
                    )
                );
            }
        }
        if (workflowId != Guid.Empty)
        {
            try
            {
                var item =
                    PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: workflowId)
                    ) as ISchemaItem;
                dependencies.Add(item: item);
            }
            catch
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "workflowId",
                    actualValue: workflowId,
                    message: ResourceUtils.GetString(
                        key: "ErrorWorkflowNotFound",
                        args: new object[] { Name, RootItem.ItemType, RootItem.Name }
                    )
                );
            }
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes =>
        new[]
        {
            typeof(PropertyValueItem),
            typeof(ControlSetItem),
            typeof(PropertyBindingInfo),
            typeof(ColumnParameterMapping),
        };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(PropertyValueItem))
        {
            itemName = "NewPropertyValue";
        }
        else if (typeof(T) == typeof(ControlSetItem))
        {
            itemName = "NewControlSetItem";
        }
        else if (typeof(T) == typeof(PropertyBindingInfo))
        {
            itemName = "NewPropertyBindingInfo";
        }
        else if (typeof(T) == typeof(ColumnParameterMapping))
        {
            itemName = "NewColumnParameterMapping";
        }
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: itemName
        );
    }
    #endregion

    public PropertyValueItem GetProperty(string propertyName)
    {
        return ChildItems.OfType<PropertyValueItem>().First(predicate: x => x.Name == propertyName);
    }

    public PropertyValueItem GetPropertyOrNull(string propertyName)
    {
        return ChildItems
            .OfType<PropertyValueItem>()
            .FirstOrDefault(predicate: x => x.Name == propertyName);
    }
}

public class ControlSetItemComparer : IComparer<ISchemaItem>
{
    #region IComparer Members
    public int Compare(ISchemaItem x, ISchemaItem y)
    {
        if (!(x is ControlSetItem xItem))
        {
            throw new ArgumentOutOfRangeException(
                paramName: "x",
                actualValue: x,
                message: "Unsupported type for comparison."
            );
        }
        if (!(y is ControlSetItem yItem))
        {
            throw new ArgumentOutOfRangeException(
                paramName: "y",
                actualValue: y,
                message: "Unsupported type for comparison."
            );
        }
        var tabX = TabIndex(control: xItem);
        var tabY = TabIndex(control: yItem);
        if (tabX == -1 || tabY == -1)
        {
            return xItem.Name.CompareTo(strB: yItem.Name);
        }
        return tabX.CompareTo(value: tabY);
    }
    #endregion
    private int TabIndex(ControlSetItem control)
    {
        foreach (
            var property in control.ChildItemsByType<PropertyValueItem>(
                itemType: PropertyValueItem.CategoryConst
            )
        )
        {
            if (property.ControlPropertyItem.Name == "TabIndex")
            {
                return property.IntValue;
            }
        }
        return -1;
    }
}

public class AlternativeControlSetItemComparer : IComparer<ControlSetItem>
{
    #region IComparer Members
    public int Compare(ControlSetItem x, ControlSetItem y)
    {
        if (x is null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "x",
                actualValue: x,
                message: "Unsupported type for comparison."
            );
        }
        if (y is null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "y",
                actualValue: y,
                message: "Unsupported type for comparison."
            );
        }
        return x.Level.CompareTo(value: y.Level);
    }
    #endregion
}
