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
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Origam.Schema.MenuModel
{
    /// <summary>
    /// Version history:
    /// 1.0.0 Initial version of HashtagCategory
    /// 1.0.1 Renamed to DeepLinkCategory
    /// </summary>
    [SchemaItemDescription("Deep Link Category", "hashtag_category.png")]
    [HelpTopic("Deep+Link+Categories")]
    [XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("1.0.1")]
    public class DeepLinkCategory : AbstractSchemaItem , ILookupReference
    {
        public const string CategoryConst = "DeepLinkCategory";

        public DeepLinkCategory() : base() { Init(); }

        public DeepLinkCategory(Guid schemaExtensionId) : base(schemaExtensionId) { Init(); }

        public DeepLinkCategory(Key primaryKey) : base(primaryKey) { Init(); }

        private void Init()
        {
        }

        private string _Label;
        [Category("Reference")]
        [DisplayName("Label")]
        [Description("A name of the deep link category that will appear to the user when creating a deep link.")]
        [NotNullModelElementRule()]
        [Localizable(true)]
        [XmlAttribute("label")]
        public string Label
        {
            get
            {
                return _Label;
            }
            set
            {
                _Label = value;
            }
        }
        public Guid LookupId;

        [Category("Reference")]
        [TypeConverter(typeof(DataLookupConverter))]
        [LookupServerSideElementRule]
        [NotNullModelElementRule]
        [Description("A lookup which will resolve the list of available values for the link. The lookup must be server-side filtered and must be connected to a menu item so the user can open the link.")]
        [XmlReference("lookup", "LookupId")]
        public IDataLookup Lookup
        {
            get
            {
                return (IDataLookup)this.PersistenceProvider.RetrieveInstance(
                    typeof(AbstractSchemaItem), new ModelElementKey(this.LookupId));
            }
            set
            {
                this.LookupId = (value == null ? Guid.Empty
                    : (Guid)value.PrimaryKey["Id"]);
            }
        }

        private string _roles = "*";
        [Category("Security")]
        [NotNullModelElementRule()]
        [XmlAttribute("roles")]
        public string Roles
        {
            get
            {
                return _roles;
            }
            set
            {
                _roles = value;
            }
        }


        public override bool UseFolders
        {
            get
            {
                return false;
            }
        }
        public override string ItemType
        {
            get
            {
                return CategoryConst;
            }
        }
    }
}
