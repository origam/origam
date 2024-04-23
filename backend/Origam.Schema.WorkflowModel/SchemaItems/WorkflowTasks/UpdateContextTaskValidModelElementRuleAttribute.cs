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

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Checks a validity of a workflow update context task schemaitem - whether the name defined in "FieldName"
/// corresponds with either a column defined on datastructure level or with an entity column
/// provided that AllFields flag on datastructure entity is set.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple=false, Inherited=true)]
public class UpdateContextTaskValidModelElementRuleAttribute : AbstractModelElementRuleAttribute 
{
	public UpdateContextTaskValidModelElementRuleAttribute()
	{
		}

	public override Exception CheckRule(object instance)
	{
			UpdateContextTask updateContextTask = (UpdateContextTask) instance;

			if (updateContextTask.OutputContextStore == null || updateContextTask.OutputContextStore.Structure == null)
			{
				// Output context sture is not a structure, don't check
				return null;
			}
			
			if (updateContextTask.GetFieldSchemaItem() == null)
			{
                return new NullReferenceException(ResourceUtils.GetString(
					"ErrorUpdateContextTaskInvalid", updateContextTask.FieldName));
			}
            return null;
		}

	public override Exception CheckRule(object instance, string memberName)
	{
			return CheckRule(instance);
		}
}