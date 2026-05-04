#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel.Attributes;

[AttributeUsage(validOn: AttributeTargets.Property | AttributeTargets.Field)]
public class LengthLimitAttribute : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(message: Strings.MemberNameRequired);
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (string.IsNullOrEmpty(value: memberName))
        {
            CheckRule(instance: instance);
        }

        var databaseProfile = ServiceManager.Services.GetService<DatabaseProfileService>();
        if (
            Reflector.GetValue(type: instance.GetType(), instance: instance, memberName: memberName)
            is not string value
        )
        {
            return null;
        }

        string errorMessage = databaseProfile.CheckIdentifierLength(length: value.Length);
        if (!string.IsNullOrEmpty(value: errorMessage))
        {
            return new Exception(message: errorMessage);
        }
        return null;
    }
}
