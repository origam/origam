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

namespace Origam.Gui.Designer;

using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;

// This is useful for converting win forms components to Html Architect classes.
// Should be removed once Html Architect implementation is complete.
public class ClassGenerator
{
    public static string GenerateClass(string className, IEnumerable<PropertyInfo> properties)
    {
        var sb = new StringBuilder();
        
        // Add using statements
        var usings = new HashSet<string>();
        foreach (var prop in properties)
        {
            // Add namespace for property type
            if (prop.PropertyType.Namespace != "System" && !prop.PropertyType.Namespace.StartsWith("System.Windows.Forms"))
            {
                usings.Add($"using {prop.PropertyType.Namespace};");
            }
            
            // Add namespaces from attributes, excluding Windows.Forms
            foreach (var attr in prop.GetCustomAttributes(true))
            {
                var attrNamespace = attr.GetType().Namespace;
                if (attrNamespace != "System" && 
                    !string.IsNullOrEmpty(attrNamespace) && 
                    !attrNamespace.StartsWith("System.Windows.Forms"))
                {
                    usings.Add($"using {attrNamespace};");
                }
            }
        }
        
        foreach (var @using in usings.OrderBy(u => u))
        {
            sb.AppendLine(@using);
        }
        sb.AppendLine();
        sb.AppendLine("namespace Origam.Architect.Server.Controls;");
        sb.AppendLine();

        // Begin class definition
        sb.AppendLine("public class " + className + ": IControl");
        sb.AppendLine("{");
        
        // Add properties
        foreach (var prop in properties)
        {
            var attributesToIgnore = new List<string>
            {
                "EditorBrowsableAttribute",
                "RefreshPropertiesAttribute",
                "DesignerSerializationVisibilityAttribute"
            };
            // Filter out Windows.Forms attributes
            var filteredAttributes = prop.GetCustomAttributes(true)
                .Where(attr => attr.GetType().Namespace == null || 
                              !attr.GetType().Namespace.StartsWith("System.Windows.Forms"))
                .Where(attr => !attributesToIgnore.Contains(attr.GetType().Name));

            // Add remaining attributes
            foreach (var attr in filteredAttributes)
            {
                sb.Append("    [");
                sb.Append(GetAttributeString(attr));
                sb.AppendLine("]");
            }
            
            // Add property, but check if type is from Windows.Forms
            Type propertyType = prop.PropertyType;
            if (propertyType.Namespace?.StartsWith("System.Windows.Forms") == true)
            {
                // Skip Windows.Forms properties
                continue;
            }
            
            string typeName = GetTypeName(propertyType);
            sb.AppendLine($"    public {typeName} {prop.Name} {{ get; set; }}");
            sb.AppendLine();
        }

        sb.AppendLine("    public void Initialize(ControlSetItem controlSetItem)");
        sb.AppendLine("    {");
        sb.AppendLine("    }");

        sb.AppendLine("}");

        return sb.ToString();
    }
    
    private static string GetAttributeString(object attribute)
    {
        var attrType = attribute.GetType();
        var attrName = attrType.Name;
        
        // Remove "Attribute" suffix if present
        if (attrName.EndsWith("Attribute"))
        {
            attrName = attrName.Substring(0, attrName.Length - 9);
        }
        
        // Get constructor parameters
        var constructorParams = new List<string>();
        foreach (var prop in attrType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.Name == "TypeId")
            {
                continue;
            }
            var value = prop.GetValue(attribute);
            if (value != null)
            {
                if (value is string)
                {
                    constructorParams.Add($"\"{value}\"");
                }
                else if(value is bool boolValue){
                    constructorParams.Add(boolValue ? "true" : "false");
                }
                else 
                {
                    constructorParams.Add(value.ToString());
                }
            }
        }
        
        if (constructorParams.Count > 0)
        {
            return $"{attrName}({string.Join(", ", constructorParams)})";
        }
        
        return attrName;
    }
    
    private static string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetTypeName));
            var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
            return $"{genericTypeName}<{genericArgs}>";
        }
        
        // Handle nullable types
        if (Nullable.GetUnderlyingType(type) != null)
        {
            return GetTypeName(Nullable.GetUnderlyingType(type)) + "?";
        }
        
        // Use C# keywords for primitive types
        switch (type.FullName)
        {
            case "System.String": return "string";
            case "System.Int32": return "int";
            case "System.Int64": return "long";
            case "System.Double": return "double";
            case "System.Boolean": return "bool";
            case "System.Decimal": return "decimal";
            case "System.DateTime": return "DateTime";
            default: return type.Name;
        }
    }
}