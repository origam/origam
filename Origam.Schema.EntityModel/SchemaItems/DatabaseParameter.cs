using Origam.DA.ObjectPersistence;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel
{
    public class DatabaseParameter : SchemaItemParameter, IDatabaseDataTypeMapping
    {
        public DatabaseParameter() : base() { }
        public DatabaseParameter(Guid schemaExtensionId) : base(schemaExtensionId) { }
        public DatabaseParameter(Key primaryKey) : base(primaryKey) { }

        [EntityColumn("G05")]
        public Guid dataTypeMappingId;

        [Category("Database Mapping")]
        [TypeConverter(typeof(DataTypeMappingConverter))]
        [Description("Database specific data type")]
        [DisplayName("Data Type Mapping")]
        [XmlReference("mappedDataType", "dataTypeMappingId")]
        public DatabaseDataType MappedDataType
        {
            get
            {
                return (DatabaseDataType)PersistenceProvider.RetrieveInstance(
                    typeof(DatabaseDataType), new ModelElementKey(dataTypeMappingId))
                    as DatabaseDataType;
            }
            set
            {
                dataTypeMappingId = (value == null ? Guid.Empty
                    : (Guid)value.PrimaryKey["Id"]);
            }

        }
    }
}
