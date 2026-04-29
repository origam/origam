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

public enum WorkflowEntityParameterMappingType
{
    Original,
    Current,
    ChangedFlag,
}

/// <summary>
/// Summary description for ContextStoreLink.
/// </summary>
[SchemaItemDescription(
    name: "Parameter Mapping",
    folderName: "Parameter Mappings",
    iconName: "parameter-blm.png"
)]
[HelpTopic(topic: "Data+Event+Parameter+Mapping")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.1")]
public class StateMachineEventParameterMapping : AbstractSchemaItem
{
    public const string CategoryConst = "WorkflowEntityParameterMapping";

    public StateMachineEventParameterMapping()
        : base() { }

    public StateMachineEventParameterMapping(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public StateMachineEventParameterMapping(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem Members

    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Field);
        dependencies.Add(item: this.ContextStore);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override ISchemaItemCollection ChildItems => SchemaItemCollection.Create();
    #endregion
    #region Properties
    [XmlAttribute(attributeName: "wfParameterType")]
    public WorkflowEntityParameterMappingType Type { get; set; } =
        WorkflowEntityParameterMappingType.Current;

    public Guid FieldId;

    [TypeConverter(type: typeof(StateMachineAllFieldConverter))]
    [XmlReference(attributeName: "field", idField: "FieldId")]
    public IDataEntityColumn Field
    {
        get
        {
            ModelElementKey key = new ModelElementKey(id: this.FieldId);
            return (IDataEntityColumn)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set => this.FieldId = (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid ContextStoreId;

    [TypeConverter(type: typeof(StateMachineEventParameterMappingContextStoreConverter))]
    [XmlReference(attributeName: "contextStore", idField: "ContextStoreId")]
    public IContextStore ContextStore
    {
        get
        {
            ModelElementKey key = new ModelElementKey(id: this.ContextStoreId);
            return (IContextStore)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            this.ContextStoreId = (Guid)value.PrimaryKey[key: "Id"];
            this.Name = (this.ContextStore == null ? "" : this.ContextStore.Name);
        }
    }
    #endregion
}
