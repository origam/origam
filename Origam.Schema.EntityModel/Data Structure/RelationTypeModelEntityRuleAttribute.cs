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
            object value = Reflector.GetValue(instance.GetType(), instance, memberName);
            if (value == null || (value as string) == string.Empty)
            {
                return new NullReferenceException(ResourceUtils.GetString("CantBeNull", memberName));
            }
            var dataStructure = (DataStructureEntity)instance;
            ArrayList schemaItems = dataStructure.Entity.ChildItemsByType(EntityRelationColumnPairItem.ItemTypeConst);
            if (schemaItems.Count==0)
            {
                return new DataException("Relationship has no key");
            }
            return null;
        }
    }
}
