using System;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel
{
	public enum AggregationType
	{
		Count,
		Sum,
		Average,
		Minimum,
		Maximum
	}

	/// <summary>
	/// Summary description for AggregatedColumn.
	/// </summary>
	public class AggregatedColumn : AbstractDataEntityColumn
	{
		public AggregatedColumn() : base() {}

		public AggregatedColumn(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public AggregatedColumn(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractDataEntityColumn Members
		
		public override bool ReadOnly
		{
			get
			{
				return true;
			}
		}

		public override string Icon
		{
			get
			{
				return "2";
			}
		}
		#endregion

		#region Properties
		private AggregationType _aggregationType = AggregationType.Sum;

		[Category("Aggregation")]
		[EntityColumn("AggregationType", "AggregatedColumn")]  
		public AggregationType AggregationType
		{
			get
			{
				return _aggregationType;
			}
			set
			{
				_aggregationType = value;
			}
		}

		[EntityColumn("refRelationId", "AggregatedColumn")]  
		public Guid RelationId;

		[EntityColumn("refRelationSchemaVersionId", "AggregatedColumn")] 
		public Guid RelationSchemaVersionId;

		[Category("Aggregation")]
		[TypeConverter(typeof(EntityRelationConverter))]
		public IAssociation Relation
		{
			get
			{
				SchemaVersionKey key = new SchemaVersionKey();
				key.Id = this.RelationId;
				key.SchemaVersionId = this.RelationSchemaVersionId;

				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key) as IAssociation;
			}
			set
			{
				this.RelationId = (Guid)value.PrimaryKey["Id"];
				this.RelationSchemaVersionId = (Guid)value.PrimaryKey["SchemaVersionId"];
			}
		}

		[EntityColumn("refColumnId", "AggregatedColumn")]  
		public Guid ColumnId;

		[EntityColumn("refColumnSchemaVersionId", "AggregatedColumn")] 
		public Guid ColumnSchemaVersionId;

		[Category("Aggregation")]
		[TypeConverter(typeof(EntityRelationColumnsConverter))]
		public IDataEntityColumn Column
		{
			get
			{
				SchemaVersionKey key = new SchemaVersionKey();
				key.Id = this.ColumnId;
				key.SchemaVersionId = this.ColumnSchemaVersionId;

				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key) as IDataEntityColumn;
			}
			set
			{
				this.ColumnId = (Guid)value.PrimaryKey["Id"];
				this.ColumnSchemaVersionId = (Guid)value.PrimaryKey["SchemaVersionId"];
			}
		}
		#endregion
	}
}
