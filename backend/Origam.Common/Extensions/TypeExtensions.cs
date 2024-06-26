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
using System.IO;
using System.Linq;
using System.Reflection;

namespace Origam.Extensions;
public static class TypeExtensions
{
    private static readonly log4net.ILog log
        = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);
    public static IEnumerable<Type> GetAllPublicSubTypes(this Type baseType)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .SelectMany(GetExportedTypes)
            .Where(baseType.IsAssignableFrom);
    }
    
    public static object GetValue(this MemberInfo memberInfo, object forObject)
    {
        switch (memberInfo.MemberType)
        {
            case MemberTypes.Field:
                return ((FieldInfo)memberInfo).GetValue(forObject);
            case MemberTypes.Property:
                return ((PropertyInfo)memberInfo).GetValue(forObject);
            default:
                throw new NotImplementedException();
        }
    } 
    
    public static IEnumerable<Type> GetAllBaseTypes(this Type type)
    {
        Type baseType = type.BaseType;
        while (baseType != typeof(object))
        {
            yield return baseType;
            baseType = baseType.BaseType;
        }
    }
    private static IEnumerable<Type> GetExportedTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetExportedTypes();
        }
        catch (FileNotFoundException)
        {
            return new List<Type>();
        }
        catch (TypeLoadException ex)
        {
            log.Warn("Could not load assembly: "+assembly.Location+", reason: "+ex.Message);
            return new List<Type>();
        }
    }
}
