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
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for EntitySecurityRule.
/// </summary>
[SchemaItemDescription(
    name: "Row Level Security Rule",
    folderName: "Row Level Security",
    iconName: "icon_row-level-security-rule.png"
)]
[HelpTopic(topic: "Row+Level+Security+Rules")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.1.0")]
public class EntitySecurityRule : AbstractEntitySecurityRule
{
    public EntitySecurityRule()
        : base() { }

    public EntitySecurityRule(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public EntitySecurityRule(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties

    [TypeConverter(type: typeof(EntityRuleConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NoRuleForExportRowLevelSecurityRule]
    [Category(category: "Security")]
    [XmlReference(attributeName: "rule", idField: "RuleId")]
    public override IEntityRule Rule
    {
        get => base.Rule;
        set => base.Rule = value;
    }

    private bool create = true;

    [
        Category(category: "Credentials"),
        DefaultValue(value: false),
        RefreshProperties(refresh: RefreshProperties.Repaint)
    ]
    [Description(description: "If set to true, the rule is applied to create operation.")]
    [XmlAttribute(attributeName: "createCredential")]
    public bool CreateCredential
    {
        get => create;
        set
        {
            create = value;
            CredentialsChanged();
        }
    }

    private bool update = true;

    [
        Category(category: "Credentials"),
        DefaultValue(value: false),
        RefreshProperties(refresh: RefreshProperties.Repaint)
    ]
    [Description(description: "If set to true, the rule is applied to update operation.")]
    [XmlAttribute(attributeName: "updateCredential")]
    public bool UpdateCredential
    {
        get => update;
        set
        {
            update = value;
            CredentialsChanged();
        }
    }

    private bool delete = true;

    [
        Category(category: "Credentials"),
        DefaultValue(value: false),
        RefreshProperties(refresh: RefreshProperties.Repaint)
    ]
    [Description(description: "If set to true, the rule is applied to delete operation.")]
    [XmlAttribute(attributeName: "deleteCredential")]
    public bool DeleteCredential
    {
        get => delete;
        set
        {
            delete = value;
            CredentialsChanged();
        }
    }

    private bool export = true;

    [
        Category(category: "Credentials"),
        DefaultValue(value: false),
        RefreshProperties(refresh: RefreshProperties.Repaint)
    ]
    [Description(description: "If set to true, the rule is applied to export operation.")]
    [XmlAttribute(attributeName: "exportCredential")]
    public bool ExportCredential
    {
        get => export;
        set
        {
            export = value;
            CredentialsChanged();
        }
    }

    internal override string CredentialsShortcut
    {
        get
        {
            var stringBuilder = new StringBuilder();
            if (CreateCredential)
            {
                stringBuilder.Append(value: "Create");
            }
            if (UpdateCredential)
            {
                stringBuilder.Append(value: "Update");
            }
            if (DeleteCredential)
            {
                stringBuilder.Append(value: "Delete");
            }
            if (ExportCredential)
            {
                stringBuilder.Append(value: "Export");
            }
            return stringBuilder.ToString();
        }
    }

    #endregion
}
