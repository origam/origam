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
using System.Data;
using System.Linq;
using Origam.DA.ObjectPersistence;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class NoDuplicateNamesInDataConstantRuleAtribute : AbstractModelElementRuleAttribute
{
    public NoDuplicateNamesInDataConstantRuleAtribute() { }

    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (string.IsNullOrEmpty(memberName))
        {
            return CheckRule(instance);
        }

        if (memberName != "Name")
        {
            throw new Exception(
                nameof(NoDuplicateNamesInDataConstantRuleAtribute)
                    + " can be only applied to Name properties"
            );
        }

        if (instance is not DataConstant dataConstant)
        {
            return null;
        }

        string instanceName = (string)Reflector.GetValue(instance.GetType(), instance, memberName);
        // An empty name belongs to StringNotEmptyModelElementRule.
        if (string.IsNullOrEmpty(instanceName))
        {
            return null;
        }

        // The architect caches the provider's child items, a persistence lookup reloads them all.
        DataConstantSchemaItemProvider constants = ServiceManager
            .Services.GetService<ISchemaService>()
            ?.GetProvider<DataConstantSchemaItemProvider>();
        if (constants == null)
        {
            return null;
        }

        bool duplicateExists = constants
            .ChildItems.OfType<DataConstant>()
            .Any(other => other.Id != dataConstant.Id && other.Name == instanceName);
        if (duplicateExists)
        {
            return new DataException(
                ResourceUtils.GetString("ErrorDuplicateDataConstantName", instanceName)
            );
        }
        return null;
    }
}
