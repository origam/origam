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
            ArrayList schemaItems = dataStructure.ChildItemsByType(DataStructureColumn.CategoryConst);
            if (schemaItems.Count>0 && (relation is RelationType.LeftJoin || relation is RelationType.InnerJoin))
            {
                return new DataException("Child Entities are DataField, but RelationType is set to LeftJoin or InnerJoin");
            }
            return null;
        }
    }
}
