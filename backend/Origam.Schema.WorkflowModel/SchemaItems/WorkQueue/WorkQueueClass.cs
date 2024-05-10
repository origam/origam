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
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for AbstractService.
	/// </summary>
	[SchemaItemDescription("Work Queue Class", "work-queue-class.png")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class WorkQueueClass : AbstractSchemaItem, ISchemaItemFactory
	{
		public const string CategoryConst = "WorkQueueClass";

		public WorkQueueClass() : base() {Init();}
		public WorkQueueClass(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
		public WorkQueueClass(Key primaryKey) : base(primaryKey)	{Init();}

		public WorkQueueWorkflowCommand GetCommand(string name)
		{
			WorkQueueWorkflowCommand cmd = this.GetChildByName(name, WorkQueueWorkflowCommand.CategoryConst) as WorkQueueWorkflowCommand;

			if(cmd == null) throw new ArgumentOutOfRangeException("name", name, ResourceUtils.GetString("ErrorUknownWorkQueueCommand"));

			return cmd;
		}

		public WorkqueueLoader GetLoader(string name)
		{
			WorkqueueLoader loader = this.GetChildByName(name, WorkqueueLoader.CategoryConst) as WorkqueueLoader;

			if(loader == null) throw new ArgumentOutOfRangeException("name", name, ResourceUtils.GetString("ErrorUknownWorkQueueLoader"));

			return loader;
		}

		private void Init()
		{
			this.ChildItemTypes.Add(typeof(WorkQueueClassEntityMapping));
			this.ChildItemTypes.Add(typeof(WorkQueueWorkflowCommand));
			this.ChildItemTypes.Add(typeof(WorkqueueLoader));
		}

		#region Overriden AbstractSchemaItem Members
		
		public override string ItemType => CategoryConst;

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			if(this.EntityStructure != null) dependencies.Add(this.EntityStructure);
			if(this.EntityStructurePrimaryKeyMethod != null) dependencies.Add(this.EntityStructurePrimaryKeyMethod);
			if(this.Entity != null)	dependencies.Add(this.Entity);
			if(this.ConditionFilter != null)	dependencies.Add(this.ConditionFilter);
			if(this.RelatedEntity1 != null)	dependencies.Add(this.RelatedEntity1);
			if(this.RelatedEntity2 != null)	dependencies.Add(this.RelatedEntity2);
			if(this.RelatedEntity3 != null)	dependencies.Add(this.RelatedEntity3);
			if(this.RelatedEntity4 != null)	dependencies.Add(this.RelatedEntity4);
			if(this.RelatedEntity5 != null)	dependencies.Add(this.RelatedEntity5);
			if(this.RelatedEntity6 != null)	dependencies.Add(this.RelatedEntity6);
			if(this.RelatedEntity7 != null)	dependencies.Add(this.RelatedEntity7);
			if(this.WorkQueueStructure != null) dependencies.Add(this.WorkQueueStructure);
			if(this.WorkQueueStructureUserListMethod != null) dependencies.Add(this.WorkQueueStructureUserListMethod);
			if(this.NotificationLoadMethod != null) dependencies.Add(this.NotificationLoadMethod);
			if(this.NotificationStructure != null) dependencies.Add(this.NotificationStructure);
			if(this.WorkQueueItemCountLookup != null) dependencies.Add(this.WorkQueueItemCountLookup);
			if(this.WorkQueueStructureSortSet != null) dependencies.Add(this.WorkQueueStructureSortSet);

			base.GetExtraDependencies (dependencies);
		}
		#endregion

		#region Properties
		[Browsable(false)]
		public ArrayList EntityMappings => 
			this.ChildItemsByType(WorkQueueClassEntityMapping.CategoryConst);

		public Guid EntityId;

		[TypeConverter(typeof(EntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("entity", "EntityId")]
		public IDataEntity Entity
		{
			get => (IDataEntity)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(EntityId));
			set
			{
				this.EntityId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
                this.ConditionFilter = null;
			}
		}
 
		public Guid EntityConditionFilterId;

		[TypeConverter(typeof(WorkQueueClassFilterConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("conditionFilter", "EntityConditionFilterId")]
		public EntityFilter ConditionFilter
		{
			get => (EntityFilter)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(EntityConditionFilterId));
			set
			{
				this.EntityConditionFilterId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid WorkQueueStructureId;

		[TypeConverter(typeof(DataStructureConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [NotNullModelElementRule()]
		[StructureMustHaveGetByIdFilterRule]
		[XmlReference("workQueueStructure", "WorkQueueStructureId")]
        public DataStructure WorkQueueStructure
		{
			get => (DataStructure)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(WorkQueueStructureId));
			set
			{
				this.WorkQueueStructureId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
				this.WorkQueueStructureSortSet = null;
				this.WorkQueueStructureUserListMethod = null;
			}
		}
        
		public Guid RelatedEntity1Id;

		[TypeConverter(typeof(EntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("relatedEntity1", "RelatedEntity1Id")]
		public IDataEntity RelatedEntity1
		{
			get => (IDataEntity)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(RelatedEntity1Id));
			set
			{
				this.RelatedEntity1Id = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid RelatedEntity2Id;

		[TypeConverter(typeof(EntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("relatedEntity2", "RelatedEntity2Id")]
		public IDataEntity RelatedEntity2
		{
			get => (IDataEntity)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(RelatedEntity2Id));
			set
			{
				this.RelatedEntity2Id = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid RelatedEntity3Id;

		[TypeConverter(typeof(EntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("relatedEntity3", "RelatedEntity3Id")]
		public IDataEntity RelatedEntity3
		{
			get => (IDataEntity)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(RelatedEntity3Id));
			set
			{
				this.RelatedEntity3Id = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid RelatedEntity4Id;

		[TypeConverter(typeof(EntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("relatedEntity4", "RelatedEntity4Id")]
		public IDataEntity RelatedEntity4
		{
			get => (IDataEntity)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(RelatedEntity4Id));
			set
			{
				this.RelatedEntity4Id = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid RelatedEntity5Id;

		[TypeConverter(typeof(EntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("relatedEntity5", "RelatedEntity5Id")]
		public IDataEntity RelatedEntity5
		{
			get => (IDataEntity)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(RelatedEntity5Id));
			set
			{
				this.RelatedEntity5Id = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid RelatedEntity6Id;

		[TypeConverter(typeof(EntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("relatedEntity6", "RelatedEntity6Id")]
		public IDataEntity RelatedEntity6
		{
			get => (IDataEntity)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(RelatedEntity6Id));
			set
			{
				this.RelatedEntity6Id = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid RelatedEntity7Id;

		[TypeConverter(typeof(EntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("relatedEntity7", "RelatedEntity7Id")]
		public IDataEntity RelatedEntity7
		{
			get => (IDataEntity)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(RelatedEntity7Id));
			set
			{
				this.RelatedEntity7Id = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid EntityStructureId;

		[TypeConverter(typeof(DataStructureConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [NotNullModelElementRule("Entity")]
		[XmlReference("entityStructure", "EntityStructureId")]
        public DataStructure EntityStructure
		{
			get => (DataStructure)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(EntityStructureId));
			set
			{
				this.EntityStructureId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
                this.EntityStructurePrimaryKeyMethod = null;
			}
		}
        
		public Guid EntityStructurePkMethodId;

		[TypeConverter(typeof(WorkQueueClassEntityStructureFilterConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [NotNullModelElementRule("EntityStructure")]
		[XmlReference("entityStructurePrimaryKeyMethod", "EntityStructurePkMethodId")]
        public DataStructureMethod EntityStructurePrimaryKeyMethod
		{
			get => (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(EntityStructurePkMethodId));
			set
			{
				this.EntityStructurePkMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
        
		public Guid WorkQueueStructureUserListMethodId;

		[TypeConverter(typeof(WorkQueueClassWQDataStructureFilterConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("workQueueStructureUserListMethod", "WorkQueueStructureUserListMethodId")]
		public DataStructureMethod WorkQueueStructureUserListMethod
		{
			get => (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(WorkQueueStructureUserListMethodId));
			set
			{
				this.WorkQueueStructureUserListMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid WorkQueueStructureSortSetId;

		[TypeConverter(typeof(WorkQueueClassWQDataStructureSortSetConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [NotNullModelElementRule]
		[XmlReference("workQueueStructureSortSet", "WorkQueueStructureSortSetId")]
		public DataStructureSortSet WorkQueueStructureSortSet
		{
			get => (DataStructureSortSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(WorkQueueStructureSortSetId));
			set
			{
				this.WorkQueueStructureSortSetId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid WorkQueueItemCountLookupId;

		[TypeConverter(typeof(DataLookupConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [NotNullModelElementRule()]
		[XmlReference("workQueueItemCountLookup", "WorkQueueItemCountLookupId")]
        public IDataLookup WorkQueueItemCountLookup
		{
			get => (IDataLookup)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(WorkQueueItemCountLookupId));
			set
			{
				this.WorkQueueItemCountLookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
        
		public Guid NotificationStructureId;

		[TypeConverter(typeof(DataStructureConverter))]
		[RefreshProperties(RefreshProperties.Repaint), Category("Notification")]
		[XmlReference("notificationStructure", "NotificationStructureId")]
		public DataStructure NotificationStructure
		{
			get => (DataStructure)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(NotificationStructureId));
			set
			{
				this.NotificationStructureId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
                this.NotificationLoadMethod = null;
			}
		}
		
		public Guid NotificationLoadMethodId;

		[TypeConverter(typeof(WorkQueueClassNotificationStructureFilterConverter))]
		[RefreshProperties(RefreshProperties.Repaint), Category("Notification")]
		[XmlReference("notificationLoadMethod", "NotificationLoadMethodId")]
		public DataStructureMethod NotificationLoadMethod
		{
			get => (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(NotificationLoadMethodId));
			set
			{
				this.NotificationLoadMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		[Category("Notification")]
		[XmlAttribute ("notificationFilterPkParameter")]
		public string NotificationFilterPkParameter { get; set; } = "";
		
		[Category("UI")]
		[XmlAttribute ("defaultPanelConfiguration")]
		public string DefaultPanelConfiguration { get; set; } = "";
		#endregion
	}
	
	[AttributeUsage(AttributeTargets.Property)]
	public class StructureMustHaveGetByIdFilterRule : AbstractModelElementRuleAttribute 
	{
		public override Exception CheckRule(object instance)
		{
			if (!(instance is WorkQueueClass workQueueClass))
			{
				throw new Exception(
					$"{nameof(StructureMustHaveGetByIdFilterRule)} can be only applied to type {nameof(WorkQueueClass)}");  
			}
			if (workQueueClass.WorkQueueStructure == null)
			{
				return null;
			}

			DataStructureFilterSet getByIdFilterSet = workQueueClass.WorkQueueStructure
				.ChildItems.ToGeneric()
				.OfType<DataStructureFilterSet>()
				.FirstOrDefault(filterSet => filterSet.Name == "GetById");

			return getByIdFilterSet == null
				? new Exception($"The {nameof(workQueueClass.WorkQueueStructure)} of " +
				                $"{nameof(WorkQueueClass)} {workQueueClass.Name}, " +
				                $"Id: {workQueueClass.Id} does not have filter set named GetById which is required.")
				: null;
		}
		
		public override Exception CheckRule(object instance, string memberName)
		{
			return CheckRule(instance);
		}
	}
}
