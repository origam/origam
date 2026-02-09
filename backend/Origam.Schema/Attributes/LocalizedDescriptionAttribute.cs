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
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Origam.Schema;

[AttributeUsage(AttributeTargets.All)]
public sealed class LocalizedDescriptionAttribute : DescriptionAttribute
{
    private readonly string resourceKey;
    private readonly Type resourceType;
    private bool isLocalized;

    public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
        : base(resourceKey)
    {
        this.resourceKey = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey));
        this.resourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
    }

    public override string Description
    {
        get
        {
            if (!isLocalized)
            {
                DescriptionValue = ResolveDescription();
                isLocalized = true;
            }

            return base.Description;
        }
    }

    private string ResolveDescription()
    {
        var resourceProperty = resourceType.GetProperty(
            resourceKey,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
        );
        if (resourceProperty?.PropertyType == typeof(string))
        {
            return resourceProperty.GetValue(null, null) as string ?? resourceKey;
        }

        var resourceManagerProperty = resourceType.GetProperty(
            "ResourceManager",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
        );
        if (resourceManagerProperty?.GetValue(null, null) is ResourceManager resourceManager)
        {
            return resourceManager.GetString(resourceKey, CultureInfo.CurrentUICulture)
                ?? resourceKey;
        }

        return resourceKey;
    }
}
