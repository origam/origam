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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Schema.WorkflowModel;

public enum TrueFalseEnum
{
    False,
    True,
}

[SchemaItemDescription(
    name: "(Task) User Interface",
    folderName: "Tasks",
    iconName: "task-user-interface.png"
)]
[HelpTopic(topic: "User+Interface+Task")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class UIFormTask : WorkflowTask
{
    public UIFormTask()
    {
        OutputMethod = ServiceOutputMethod.FullMerge;
    }

    public UIFormTask(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId)
    {
        OutputMethod = ServiceOutputMethod.FullMerge;
    }

    public UIFormTask(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        OutputMethod = ServiceOutputMethod.FullMerge;
    }

    #region Override ISchemaItem Members
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: Screen);
        if (RefreshDataStructure != null)
        {
            dependencies.Add(item: RefreshDataStructure);
        }
        if (RefreshMethod != null)
        {
            dependencies.Add(item: RefreshMethod);
        }
        if (RefreshSortSet != null)
        {
            dependencies.Add(item: RefreshSortSet);
        }
        if (SaveDataStructure != null)
        {
            dependencies.Add(item: SaveDataStructure);
        }
        if (SaveConfirmationRule != null)
        {
            dependencies.Add(item: SaveConfirmationRule);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
    #region Properties
    public Guid ScreenId;

    [TypeConverter(type: typeof(FormControlSetConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "screen", idField: "ScreenId")]
    public FormControlSet Screen
    {
        get =>
            (FormControlSet)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ScreenId)
                );
        set => ScreenId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid RefreshDataStructureId;

    [Category(category: "Data Refresh Parameters")]
    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "refreshDataStructure", idField: "RefreshDataStructureId")]
    public DataStructure RefreshDataStructure
    {
        get =>
            (ISchemaItem)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RefreshDataStructureId)
                ) as DataStructure;
        set
        {
            if (value == null)
            {
                RefreshDataStructureId = Guid.Empty;
            }
            else
            {
                RefreshDataStructureId = (Guid)value.PrimaryKey[key: "Id"];
            }
            RefreshMethod = null;
            RefreshSortSet = null;
        }
    }

    public Guid RefreshMethodId;

    [TypeConverter(type: typeof(UIFormTaskMethodConverter))]
    [Category(category: "Data Refresh Parameters")]
    [XmlReference(attributeName: "refreshMethod", idField: "RefreshMethodId")]
    public DataStructureMethod RefreshMethod
    {
        get =>
            (DataStructureMethod)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RefreshMethodId)
                );
        set => RefreshMethodId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid RefreshSortSetId;

    [TypeConverter(type: typeof(UIFormTaskSortSetConverter))]
    [Category(category: "Data Refresh Parameters")]
    [XmlReference(attributeName: "refreshSortSet", idField: "RefreshSortSetId")]
    public DataStructureSortSet RefreshSortSet
    {
        get =>
            (DataStructureSortSet)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RefreshSortSetId)
                );
        set => RefreshSortSetId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid SaveDataStructureId;

    [Category(category: "Save Parameters")]
    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "saveDataStructure", idField: "SaveDataStructureId")]
    public DataStructure SaveDataStructure
    {
        get =>
            (ISchemaItem)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: SaveDataStructureId)
                ) as DataStructure;
        set =>
            SaveDataStructureId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "isFinalForm")]
    public bool IsFinalForm { get; set; } = false;

    [DefaultValue(value: false)]
    [Category(category: "Save Parameters")]
    [XmlAttribute(attributeName: "allowSave")]
    public bool AllowSave { get; set; } = false;

    public Guid SaveConfirmationRuleId;

    [Category(category: "Save Parameters")]
    [TypeConverter(type: typeof(EndRuleConverter))]
    [XmlReference(attributeName: "saveConfirmationRule", idField: "SaveConfirmationRuleId")]
    public IEndRule SaveConfirmationRule
    {
        get =>
            (IEndRule)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: SaveConfirmationRuleId)
                );
        set =>
            SaveConfirmationRuleId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "autoNext")]
    public bool AutoNext { get; set; } = false;

    [DefaultValue(value: true)]
    [XmlAttribute(attributeName: "isRefreshSuppressedBeforeFirstSave")]
    public bool IsRefreshSuppressedBeforeFirstSave { get; set; } = true;

    [DefaultValue(value: TrueFalseEnum.False)]
    [Description(description: "If true, the client will refresh its menu after saving data.")]
    [XmlAttribute(attributeName: "refreshPortalAfterSave")]
    public TrueFalseEnum RefreshPortalAfterSave { get; set; } = TrueFalseEnum.False;
    public List<ISchemaItem> RefreshParameters
    {
        get
        {
            var result = new List<ISchemaItem>();
            foreach (var item in ChildItems)
            {
                if (
                    (item is ContextReference)
                    || (item is DataConstantReference)
                    || (item is SystemFunctionCall)
                )
                {
                    result.Add(item: item);
                }
            }
            return result;
        }
    }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes =>
        new[]
        {
            typeof(WorkflowTaskDependency),
            typeof(ContextReference),
            typeof(DataConstantReference),
            typeof(SystemFunctionCall),
        };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(ContextReference))
        {
            itemName = "NewContextReference";
        }
        else if (typeof(T) == typeof(DataConstantReference))
        {
            itemName = "NewDataConstantReference";
        }
        else if (typeof(T) == typeof(SystemFunctionCall))
        {
            itemName = "NewSystemFunctionCall";
        }
        else if (typeof(T) == typeof(WorkflowTaskDependency))
        {
            itemName = "NewWorkflowTaskDependency";
        }
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: itemName
        );
    }
    #endregion
}
