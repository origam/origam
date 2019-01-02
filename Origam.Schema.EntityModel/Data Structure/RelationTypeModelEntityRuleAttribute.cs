using Origam.Schema.EntityModel;
using System;
using System.Collections;
using System.Data;

namespace Origam.DA.ObjectPersistence.Attributes
{
    public class RelationTypeModelEntityRuleAttribute : AbstractModelElementRuleAttribute
    {
        public RelationTypeModelEntityRuleAttribute()
        {
        }

        public override Exception CheckRule(object instance)
        {
            return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
        }

        public override Exception CheckRule(object instance, string memberName)
        {
            if (memberName == String.Empty | memberName == null) CheckRule(instance);
            var dataStructure = (DataStructureEntity)instance;
            if (dataStructure.Entity != null)
            {
                var relation = dataStructure.Entity.ChildItemsByType(EntityRelationItem.ItemTypeConst);
                foreach (EntityRelationItem rel in relation)
                {
                    ArrayList schemaItems = rel.ChildItemsByType(EntityRelationColumnPairItem.ItemTypeConst);
                    if (schemaItems.Count == 0)
                    {
                        return new DataException("Relationship has no key");
                    }
                }
            }
            return null;
        }
    }
}
