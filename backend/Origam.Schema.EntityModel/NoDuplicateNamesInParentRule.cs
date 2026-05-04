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
using System.Linq;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;

namespace Origam.DA.EntityModel;

[AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class NoDuplicateNamesInParentRule : AbstractModelElementRuleAttribute
{
    public NoDuplicateNamesInParentRule() { }

    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(
            message: ResourceUtils.GetString(key: "MemberNameRequired")
        );
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (string.IsNullOrEmpty(value: memberName))
        {
            CheckRule(instance: instance);
        }

        if (memberName != "Name")
        {
            throw new Exception(
                message: nameof(NoDuplicateNamesInParentRule)
                    + " can be only applied to Name properties"
            );
        }

        if (!(instance is ISchemaItem abstractSchemaItem))
        {
            return null;
        }

        if (abstractSchemaItem.ParentItem == null)
        {
            return null;
        }

        string instanceName = (string)
            Reflector.GetValue(
                type: instance.GetType(),
                instance: instance,
                memberName: memberName
            );
        ISchemaItem itemWithDuplicateName = abstractSchemaItem
            .ParentItem.ChildItems.Where(predicate: item => item is AbstractDataEntityColumn)
            .Where(predicate: item => item.Name == instanceName)
            .FirstOrDefault(predicate: item => item.Id != abstractSchemaItem.Id);
        if (itemWithDuplicateName != null)
        {
            return new Exception(
                message: abstractSchemaItem.ParentItem.Name
                    + " contains duplicate child names: "
                    + instanceName
            );
        }
        return null;
    }
}
