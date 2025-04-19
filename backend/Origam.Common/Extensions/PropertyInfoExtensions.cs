using System;
using System.Reflection;

namespace Origam.Extensions;

public static class PropertyInfoExtensions
{
    public static T GetAttribute<T>(this PropertyInfo property) where T : Attribute
    {
        object[] attributes = property.GetCustomAttributes(typeof(T), true);
        return attributes is { Length: > 0 } 
            ? attributes[0] as T 
            : null;
    }
}