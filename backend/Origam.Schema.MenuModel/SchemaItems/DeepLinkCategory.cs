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
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;

namespace Origam.Schema.MenuModel;

/// <summary>
/// Version history:
/// 1.0.0 Initial version of HashtagCategory
/// 1.0.1 Renamed to DeepLinkCategory
/// </summary>
[SchemaItemDescription(name: "Deep Link Category", iconName: "hashtag_category.png")]
[HelpTopic(topic: "Deep+Link+Categories")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "1.0.1")]
public class DeepLinkCategory : AbstractSchemaItem, ILookupReference
{
    public const string CategoryConst = "DeepLinkCategory";

    public DeepLinkCategory()
        : base()
    {
        Init();
    }

    public DeepLinkCategory(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public DeepLinkCategory(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init() { }

    private string _Label;

    [Category(category: "Reference")]
    [DisplayName(displayName: "Label")]
    [Description(
        description: "A name of the deep link category that will appear to the user when creating a deep link."
    )]
    [NotNullModelElementRule()]
    [Localizable(isLocalizable: true)]
    [XmlAttribute(attributeName: "label")]
    public string Label
    {
        get { return _Label; }
        set { _Label = value; }
    }
    public Guid LookupId;

    [Category(category: "Reference")]
    [TypeConverter(type: typeof(DataLookupConverter))]
    [LookupServerSideElementRule]
    [NotNullModelElementRule]
    [Description(
        description: "A lookup which will resolve the list of available values for the link. The lookup must be server-side filtered and must be connected to a menu item so the user can open the link."
    )]
    [XmlReference(attributeName: "lookup", idField: "LookupId")]
    public IDataLookup Lookup
    {
        get
        {
            return (IDataLookup)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.LookupId)
                );
        }
        set { this.LookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]); }
    }
    private string _roles = "*";

    [Category(category: "Security")]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "roles")]
    public string Roles
    {
        get { return _roles; }
        set { _roles = value; }
    }
    public override bool UseFolders
    {
        get { return false; }
    }
    public override string ItemType
    {
        get { return CategoryConst; }
    }
}
