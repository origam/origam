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
using Origam.Schema.ItemCollection;

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for ContextStore.
/// </summary>
[SchemaItemDescription(
    name: "Context Store",
    folderName: "Context Stores",
    iconName: "context-store.png"
)]
[HelpTopic(topic: "Context+Store")]
[DefaultProperty(name: "Structure")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class ContextStore : AbstractSchemaItem, IContextStore
{
    public const string CategoryConst = "ContextStore";

    public ContextStore()
        : base() { }

    public ContextStore(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public ContextStore(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public bool isScalar()
    {
        return this.Structure == null;
    }

    #region Overriden ISchemaItem Members
    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (this.Structure != null)
        {
            dependencies.Add(item: this.Structure);
        }
        if (this.RuleSet != null)
        {
            dependencies.Add(item: this.RuleSet);
        }
        if (this.DefaultSet != null)
        {
            dependencies.Add(item: this.DefaultSet);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }

    public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
    {
        // can move inside the same workflow and we can move it under any block
        if (this.RootItem == (newNode as ISchemaItem).RootItem && newNode is IWorkflowBlock)
        {
            return true;
        }

        return false;
    }
    #endregion
    #region IContextStore Members
    [DefaultValue(value: OrigamDataType.Xml)]
    [XmlAttribute(attributeName: "dataType")]
    public OrigamDataType DataType { get; set; } = OrigamDataType.Xml;

    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "isReturnValue")]
    // [ContexStoreOutputRuleAttribute()]
    public bool IsReturnValue { get; set; } = false;

    [DefaultValue(value: false)]
    [Description(
        description: "When set to True it will not check for mandatory fields, primary key duplicates or existence of parent records in the in-memory representation of data."
    )]
    [XmlAttribute(attributeName: "disableConstraints")]
    public bool DisableConstraints { get; set; } = false;
    public Guid DataStructureId;

    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule(
        conditionField1: "DataType",
        conditionField2: null,
        conditionValue1: OrigamDataType.Xml,
        conditionValue2: null
    )]
    [XmlReference(attributeName: "structure", idField: "DataStructureId")]
    public AbstractDataStructure Structure
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DataStructureId;
            return (AbstractDataStructure)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            if (this.DataType == OrigamDataType.Xml)
            {
                if (value == null)
                {
                    this.DataStructureId = Guid.Empty;
                    this.Name = "";
                }
                else
                {
                    this.DataStructureId = (Guid)value.PrimaryKey[key: "Id"];
                    this.Name = this.Structure.Name;
                }
            }
            this.RuleSet = null;
            this.DefaultSet = null;
        }
    }
    public Guid RuleSetId;

    [TypeConverter(type: typeof(ContextStoreRuleSetConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "ruleSet", idField: "RuleSetId")]
    public DataStructureRuleSet RuleSet
    {
        get
        {
            return (DataStructureRuleSet)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.RuleSetId)
                );
        }
        set { this.RuleSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]; }
    }
    public Guid DefaultSetId;

    [TypeConverter(type: typeof(ContextStoreDefaultSetConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "defaultSet", idField: "DefaultSetId")]
    public DataStructureDefaultSet DefaultSet
    {
        get
        {
            return (DataStructureDefaultSet)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.DefaultSetId)
                );
        }
        set { this.DefaultSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]; }
    }
    #endregion
}
