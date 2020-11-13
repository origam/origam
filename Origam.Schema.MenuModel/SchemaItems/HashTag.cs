using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace Origam.Schema.MenuModel
{
    [SchemaItemDescription("Hash Tag", 151)]
    [HelpTopic("Hash Tags")]
    [XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("1.0.0")]
    public class HashTag : AbstractSchemaItem
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
        [NotNullModelElementRule()]
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
