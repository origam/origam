using Origam.Schema.EntityModel;
using System;
using System.Collections;
using System.Data;

namespace Origam.DA.ObjectPersistence.Attributes
{
    public class RelationshipWithKeyRuleAttribute : AbstractModelElementRuleAttribute
    {
        public RelationshipWithKeyRuleAttribute()
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
            if (dataStructure.Entity != null && dataStructure.Entity is IAssociation)
            {
                ArrayList schemaItems = dataStructure.Entity.ChildItemsByType(EntityRelationColumnPairItem.ItemTypeConst);
                if (schemaItems.Count == 0)
                {
                    return new DataException("Relationship has no key");
                }
            }
            return null;
        }
    }
}
