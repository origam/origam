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

namespace Origam.Schema.LookupModel;

/// <summary>
/// Summary description for NotNullModelElementRuleAttribute.
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field,
    AllowMultiple = false,
    Inherited = true
)]
public class LookupServerSideFilterModelElementRuleAttribute : AbstractModelElementRuleAttribute
{
    public LookupServerSideFilterModelElementRuleAttribute() { }

    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        AbstractDataLookup lookup = instance as AbstractDataLookup;
        if (lookup != null && lookup.IsFilteredServerside)
        {
            switch (memberName)
            {
                case "ServersideFilterParameter":
                {
                    if (
                        lookup.ServersideFilterParameter == null
                        || lookup.ServersideFilterParameter == string.Empty
                    )
                    {
                        return new ArgumentNullException("ServersideFilterParameter");
                    }
                    break;
                }

                case "ListMethod":
                {
                    if (lookup.ListMethod == null)
                    {
                        return new ArgumentNullException("ListMethod");
                    }
                    break;
                }
            }
        }
        return null;
    }
}
