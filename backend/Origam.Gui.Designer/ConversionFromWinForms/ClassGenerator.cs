using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Origam.Gui.Designer;


using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;

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
        
        // Begin class definition
        sb.AppendLine("public class " + className);
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