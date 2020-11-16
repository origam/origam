#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
    [SchemaItemDescription("Hash Tag", 151)]
    [HelpTopic("Hash Tags")]
    [XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("1.0.0")]
    public class HashTag : AbstractSchemaItem , IHashTag
    {
        public const string CategoryConst = "HashTag";

        public HashTag() : base() { Init(); }

        public HashTag(Guid schemaExtensionId) : base(schemaExtensionId) { Init(); }

        public HashTag(Key primaryKey) : base(primaryKey) { Init(); }

        private void Init()
        {
        }

        private string _Label;
        [Category("Reference")]
        [DisplayName("Label")]
        [Description("A text what will be Name in list of Lookup HashTags")]
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
        [LookupServerSideElementRule()]
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
        [EntityColumn("ItemType")]
        public override string ItemType
        {
            get
            {
                return CategoryConst;
            }
        }

        public override string Icon
        {
            get
            {
                return "151";
            }
        }
    }
}
