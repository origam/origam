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
using System.Collections.Generic;

namespace Origam.Extensions;

public static class ObjectExtensions
{
    private static readonly Dictionary<Type, object> caschedDefaultTypes =
        new Dictionary<Type, object>();

    public static bool IsDefault(this object obj)
    {
        if (ReferenceEquals(objA: obj, objB: null))
        {
            return true;
        }

        Type type = obj.GetType();
        if (type.IsValueType)
        {
            return obj.Equals(obj: GetValueTypeDefault(type: type));
        }
        return false;
    }

    private static object GetValueTypeDefault(Type type)
    {
        if (caschedDefaultTypes.ContainsKey(key: type))
        {
            return caschedDefaultTypes[key: type];
        }
        object defValue = Activator.CreateInstance(type: type);
        caschedDefaultTypes.Add(key: type, value: defValue);
        return defValue;
    }
}
