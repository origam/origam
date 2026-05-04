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
/// Summary description for ContextStoreLink.
/// </summary>
[SchemaItemDescription(name: "Parameter Mapping", folderName: "Parameter Mappings", icon: 17)]
[HelpTopic(topic: "Dynamic+State+Workflow+Parameter")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class StateMachineDynamicLookupParameterMapping : AbstractSchemaItem
{
    public const string CategoryConst = "StateMachineDynamicLookupParameterMapping";

    public StateMachineDynamicLookupParameterMapping()
        : base() { }

    public StateMachineDynamicLookupParameterMapping(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public StateMachineDynamicLookupParameterMapping(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public override string Icon
    {
        get { return "17"; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Field);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
    #region Properties
    WorkflowEntityParameterMappingType _type = WorkflowEntityParameterMappingType.Current;

    [XmlAttribute(attributeName: "type")]
    public WorkflowEntityParameterMappingType Type
    {
        get { return _type; }
        set { _type = value; }
    }
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
        set { this.FieldId = (Guid)value.PrimaryKey[key: "Id"]; }
    }
    #endregion
}
