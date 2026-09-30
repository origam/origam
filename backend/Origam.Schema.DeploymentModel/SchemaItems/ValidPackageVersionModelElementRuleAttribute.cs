#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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
using System.Text.RegularExpressions;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.DeploymentModel.SchemaItems;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ValidPackageVersionModelElementRuleAttribute : AbstractModelElementRuleAttribute
{
    private static readonly Regex VersionNumberRegex = new Regex(
        @"^[0-9]+(\.[0-9]+){1,}$",
        RegexOptions.CultureInvariant
    );

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
        string versionString =
            Reflector.GetValue(instance.GetType(), instance, memberName) as string;
        if (string.IsNullOrEmpty(versionString))
        {
            return null;
        }
        if (!IsValidVersion(versionString))
        {
            return new ArgumentException(
                ResourceUtils.GetString("ErrorInvalidPackageVersion", versionString)
            );
        }
        return null;
    }

    private static bool IsValidVersion(string versionString)
    {
        return VersionNumberRegex.IsMatch(versionString)
            && versionString.Split('.').All(part => int.TryParse(part, out _));
    }
}
