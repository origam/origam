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

namespace Origam.Schema.MenuModel;

[SchemaItemDescription(
    name: "Client Script Invocation",
    folderName: "Scripts",
    iconName: "icon_client-script-invocation.png"
)]
[HelpTopic(topic: "Client+Script")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class EntityWorkflowActionScriptCall : AbstractSchemaItem
{
    public const string CategoryConst = "EntityWorkflowActionScriptCall";

    public EntityWorkflowActionScriptCall()
        : base() { }

    public EntityWorkflowActionScriptCall(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public EntityWorkflowActionScriptCall(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    private string _roles = "";

    [Category(category: "Condition"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [StringNotEmptyModelElementRule()]
    [XmlAttribute(attributeName: "roles")]
    public string Roles
    {
        get { return _roles; }
        set { _roles = value; }
    }
    private string _features;

    [Category(category: "Condition")]
    [XmlAttribute(attributeName: "features")]
    public string Features
    {
        get { return _features; }
        set { _features = value; }
    }
    public Guid RuleId;

    [Category(category: "Condition")]
    [TypeConverter(type: typeof(EntityRuleConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "rule", idField: "RuleId")]
    public IEntityRule Rule
    {
        get
        {
            return (IEntityRule)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RuleId)
                );
        }
        set { RuleId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]; }
    }
    private int _order = 0;

    [Category(category: "Script")]
    [XmlAttribute(attributeName: "order")]
    public int Order
    {
        get { return _order; }
        set { _order = value; }
    }
    private string _script;

    [Category(category: "Script")]
    [XmlAttribute(attributeName: "script")]
    public string Script
    {
        get { return _script; }
        set { _script = value; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (Rule != null)
        {
            dependencies.Add(item: Rule);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }
}
