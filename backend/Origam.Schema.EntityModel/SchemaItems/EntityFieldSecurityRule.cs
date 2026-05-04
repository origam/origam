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
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for FieldSecurityRule.
/// </summary>
[SchemaItemDescription(
    name: "Row Level Security Rule",
    folderName: "Row Level Security",
    iconName: "icon_row-level-security-rule.png"
)]
[HelpTopic(topic: "Row+Level+Security+Rules")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class EntityFieldSecurityRule : AbstractEntitySecurityRule
{
    public EntityFieldSecurityRule()
        : base() { }

    public EntityFieldSecurityRule(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public EntityFieldSecurityRule(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    private bool _read = true;

    [
        Category(category: "Credentials"),
        DefaultValue(value: false),
        RefreshProperties(refresh: RefreshProperties.Repaint)
    ]
    [XmlAttribute(attributeName: "readCredential")]
    public bool ReadCredential
    {
        get { return _read; }
        set
        {
            _read = value;
            this.CredentialsChanged();
        }
    }
    private bool _update = true;

    [
        Category(category: "Credentials"),
        DefaultValue(value: false),
        RefreshProperties(refresh: RefreshProperties.Repaint)
    ]
    [XmlAttribute(attributeName: "updateCredential")]
    public bool UpdateCredential
    {
        get { return _update; }
        set
        {
            _update = value;
            this.CredentialsChanged();
        }
    }
    internal override string CredentialsShortcut
    {
        get
        {
            string result = "";
            result += (this.ReadCredential ? "Read" : "");
            result += (this.UpdateCredential ? "Update" : "");
            return result;
        }
    }
    #endregion
}
