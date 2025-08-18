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

using System;
using Origam.DA.ObjectPersistence; 

namespace Origam.Schema.EntityModel;
/// <summary>
/// Checks a validity of a datastructure sortset item - whether the name defined in "FieldName"
/// corresponds with either a column defined on datastructure level or with an entity column
/// provided that AllFields flag on datastructure entity is set.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple=false, Inherited=true)]
public class SortSetItemValidModelElementRuleAttribute : AbstractModelElementRuleAttribute 
{
	public SortSetItemValidModelElementRuleAttribute()
	{
	}
    public override Exception CheckRule(object instance)
    {
        // get data structure entity to check
        DataStructureSortSetItem sortSetItem = (DataStructureSortSetItem)instance;
        // newly created sort set item doesn't have entity set yet
        if (sortSetItem.Entity == null)
        {
            return null;
        }
		// look into columns defined on data structure level and find out
		// if there is a column from "FieldName" property
		foreach (var col in
			     sortSetItem.Entity.ChildItemsByType<DataStructureColumn>(DataStructureColumn.CategoryConst))
		{
			if (col.Name == sortSetItem.FieldName)
			{
				return null;
			}
		}
		foreach (DataStructureColumn col in 
			sortSetItem.Entity.GetColumnsFromEntity())
		{
			if (col.Name == sortSetItem.FieldName)
			{
				return null;
			}
		}
		return new NullReferenceException(ResourceUtils.GetString(
			"ErrorSortSetItemInvalid", sortSetItem.FieldName));
	}
	public override Exception CheckRule(object instance, string memberName)
	{
		return CheckRule(instance);
	}
}
