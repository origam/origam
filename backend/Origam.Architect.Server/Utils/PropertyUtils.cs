using System.ComponentModel;
using System.Reflection;
using Origam.Extensions;
using Origam.Schema;

namespace Origam.Architect.Server.Utils;

public static class PropertyUtils
{
    public static bool CanBeEdited(PropertyInfo property)
    {        
        var browsableAttribute = property.GetAttribute<BrowsableAttribute>();
        if (browsableAttribute == null)
        {
            return true;
        }

        return browsableAttribute.Browsable;
    }
}