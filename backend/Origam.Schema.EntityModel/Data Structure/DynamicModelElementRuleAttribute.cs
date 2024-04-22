#region license
/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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
using Origam.DA.ObjectPersistence;
using System;
using System.Collections;

namespace Origam.Schema.EntityModel;

internal class DynamicModelElementRuleAttribute : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
            return new NotSupportedException(
                ResourceUtils.GetString("MemberNameRequired"));
        }

    public override Exception CheckRule(object instance, string memberName)
    {
            if (string.IsNullOrEmpty(memberName))
            {
                CheckRule(instance);
            }
            var filterSet = (DataStructureFilterSet)instance;
            ArrayList filters = filterSet.ChildItemsRecursive;
            foreach (DataStructureFilterSetFilter filter in filters)
            {
                if (((filter.IgnoreFilterConstantId != Guid.Empty)
                     || !string.IsNullOrEmpty(filter.IgnoreFilterParameterName) 
                     || filter.PassWhenParameterMatch) 
                    && !filterSet.IsDynamic)
                {
                    return new Exception( ResourceUtils.GetString(
                        "ErrorDynamicParameter", memberName));
                }
            }
            return null;
        }
}