using Origam.Schema.EntityModel;
using System;
using System.Data;

namespace Origam.DA.ObjectPersistence.Attributes
{
    public class RelationTypeParentModelElementRuleAttribute : AbstractModelElementRuleAttribute
    {
        public RelationTypeParentModelElementRuleAttribute()
        {
        }

        public override Exception CheckRule(object instance)
        {
            return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
        }

        public override Exception CheckRule(object instance, string memberName)
        {
            if (memberName == String.Empty | memberName == null) CheckRule(instance);

            var dataStructureColumn = (DataStructureColumn)instance;
            var abstractSchemaItemParent = dataStructureColumn.ParentItem;
            if (abstractSchemaItemParent?.ItemType is DataStructureEntity.ItemTypeConst)
            {
                var dataStructureEntity = (DataStructureEntity)abstractSchemaItemParent;
                if (dataStructureEntity.RelationType is RelationType.LeftJoin || dataStructureEntity.RelationType is RelationType.InnerJoin)
                {
                    return new DataException("Parent entity have set RelationType LeftJoin or InnerJoin.");
                }
            }
            return null;
        }
    }
}
