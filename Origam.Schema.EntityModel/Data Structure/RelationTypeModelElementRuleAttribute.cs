using Origam.Schema.EntityModel;
using System;
using System.Collections;
using System.Data;

namespace Origam.DA.ObjectPersistence.Attributes
{
    public class RelationTypeModelElementRuleAttribute : AbstractModelElementRuleAttribute
    {
        public RelationTypeModelElementRuleAttribute()
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
            bool allFields = dataStructure.AllFields;
            RelationType relation = dataStructure.RelationType;
            if (relation is RelationType.LeftJoin || relation is RelationType.InnerJoin)
            {
                if (allFields)
                {
                    return new DataException("Field 'AllFields' MUST BE false, if RelationType is set LeftJoin or InnerJoin");
                }
            }
            // pro prohledavani childu a zjistovani jestli tam neni field.
            ArrayList schemaItems = dataStructure.ChildItemsByType(DataStructureColumn.ItemTypeConst);
            if (schemaItems.Count>0 && (relation is RelationType.LeftJoin || relation is RelationType.InnerJoin))
            {
                return new DataException("Child Entities are DataField, but RelationType is set to LeftJoin or InnerJoin");
            }
            return null;
        }
    }
}
