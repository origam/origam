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
#pragma warning disable IDE0005
using System.Collections;
#pragma warning restore IDE0005
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#pragma warning disable IDE0005
using System.Text;
using Origam.Extensions;
#pragma warning restore IDE0005

namespace Origam;

public class Reflector
{
    private static readonly BindingFlags SearchCriteria =
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly ConcurrentDictionary<
        Type,
        ConcurrentDictionary<Type, List<MemberAttributeInfo>>
    > memberTypeCache = new();
    private static IReflectorCache classCache;
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: MethodBase.GetCurrentMethod()?.DeclaringType
    );

    protected Reflector() { }

    public static List<ConstructorInfo> FindConstructors(Type type)
    {
        var result = new List<ConstructorInfo>();
        foreach (
            ConstructorInfo constructorInfo in type.GetConstructors(bindingAttr: SearchCriteria)
        )
        {
            // exclude abstract constructors (weird concept anyway)
            if (!constructorInfo.IsAbstract)
            {
                result.Add(item: constructorInfo);
            }
        }
        return result;
    }

    public static void CopyMembers(object source, object target, Type[] attributeTypes)
    {
        FindMembers(type: source.GetType(), primaryAttributes: attributeTypes)
            .ForEach(action: attrInfo =>
            {
                MemberInfo memberInfo = attrInfo.MemberInfo;
                switch (memberInfo)
                {
                    case PropertyInfo propInfo:
                    {
                        object value = propInfo.GetValue(obj: source);
                        propInfo.SetValue(obj: target, value: value);
                        break;
                    }
                    case FieldInfo fieldInfo:
                    {
                        object value = fieldInfo.GetValue(obj: source);
                        fieldInfo.SetValue(obj: target, value: value);
                        break;
                    }
                }
            });
    }

    public static List<MemberAttributeInfo> FindMembers(
        Type type,
        IEnumerable<Type> primaryAttributes
    )
    {
        return primaryAttributes
            .SelectMany(selector: attr => FindMembers(type: type, primaryAttribute: attr))
            .ToList();
    }

    public static List<MemberAttributeInfo> FindMembers(
        Type type,
        Type primaryAttribute,
        params Type[] secondaryAttributes
    )
    {
        List<MemberAttributeInfo> GetAttributes(Type key)
        {
            var result = new List<MemberAttributeInfo>();
            foreach (MemberInfo memberInfo in type.GetMembers(bindingAttr: SearchCriteria))
            {
                object[] attrs = memberInfo.GetCustomAttributes(
                    attributeType: primaryAttribute,
                    inherit: true
                );
                foreach (object attr in attrs)
                {
                    List<Attribute> memberAttrs = FindAttributes(
                        memberInfo: memberInfo,
                        attributes: secondaryAttributes
                    );
                    result.Add(
                        item: new MemberAttributeInfo(
                            memberInfo: memberInfo,
                            attribute: attr as Attribute,
                            memberAttributes: memberAttrs
                        )
                    );
                }
            }
            return result;
        }
        // cache only requests without secondary attributes
        if (secondaryAttributes.Length == 0)
        {
            var innerDictionary = memberTypeCache.GetOrAdd(
                key: type,
                valueFactory: _ => new ConcurrentDictionary<Type, List<MemberAttributeInfo>>()
            );
            return innerDictionary.GetOrAdd(key: primaryAttribute, valueFactory: GetAttributes);
        }
        return GetAttributes(key: type);
    }

    public static List<Attribute> FindAttributes(MemberInfo memberInfo, params Type[] attributes)
    {
        var result = new List<Attribute>();
        foreach (Type attribute in attributes)
        {
            object[] attrs = memberInfo.GetCustomAttributes(
                attributeType: attribute,
                inherit: true
            );
            if (attrs is { Length: 1 })
            {
                result.Add(item: attrs[0] as Attribute);
            }
        }
        return result;
    }

    private static void ActivateCache()
    {
        classCache ??=
            Activator.CreateInstance(
                type: Type.GetType(
                    typeName: "Origam.ReflectorCache.ReflectorCache,Origam.ReflectorCache"
                )
            ) as IReflectorCache;
    }

    private static string ComposeAssemblyPath(string assembly)
    {
        return AppDomain.CurrentDomain.BaseDirectory
            + assembly.Split(separator: ",".ToCharArray())[0].Trim()
            + ".dll";
    }

    public static object InvokeObject(string classname, string assembly)
    {
        ActivateCache();
        object result = null;
        // try to get the object from the cache
        if (classCache != null)
        {
            result = classCache.InvokeObject(classname: classname, assembly: assembly);
        }
        if (result != null)
        {
            return result;
        }
        // it was not instanced by cache, so we invoke it using reflection
        Type classType = ResolveTypeFromAssembly(classname: classname, assemblyName: assembly);
        if (classType == null)
        {
            throw new Exception(
                message: $"Class {classname} from assembly {assembly} was not found."
            );
        }
        return Activator.CreateInstance(type: classType);
    }

    public static Type ResolveTypeFromAssembly(string classname, string assemblyName)
    {
        var classType = Type.GetType(typeName: classname + "," + assemblyName);
#if NETSTANDARD
        if (classType == null)
        {
            // try to load assembly to default application context
            // With .NET Core, we need to explicitly load assemblies, that are not a part of the .dep.json file
            var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(
                assemblyPath: ComposeAssemblyPath(assembly: assemblyName)
            );
            classType = assembly.GetType(name: classname);
            if (log.IsDebugEnabled && classType == null)
            {
                log.RunHandled(loggingAction: () =>
                {
                    log.DebugFormat(
                        format: "Can't resolve type '{0}' from assembly path '{1}'",
                        arg0: classname + "," + assemblyName,
                        arg1: ComposeAssemblyPath(assembly: assemblyName)
                    );
                });
            }
        }
#endif
        return classType;
    }

    public static object InvokeObject(string assemblyName, string typeName, object[] args)
    {
        ActivateCache();
        object instance = null;
        if (classCache != null)
        {
            instance = classCache.InvokeObject(typeName: typeName, args: args);
        }
        if (instance != null)
        {
            return instance;
        }
        var classType = ResolveTypeFromAssembly(classname: typeName, assemblyName: assemblyName);
        return Activator.CreateInstance(
            type: classType,
            bindingAttr: BindingFlags.DeclaredOnly
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Instance
                | BindingFlags.CreateInstance,
            binder: null,
            args: args,
            culture: System.Globalization.CultureInfo.CurrentCulture
        );
    }

    public static object GetValue(Type type, object instance, string memberName)
    {
        MemberInfo[] memberInfos = type.GetMember(
            name: memberName,
            bindingAttr: BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Instance
        );
        if (memberInfos.Length == 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(memberName),
                actualValue: memberName,
                message: ResourceUtils.GetString(key: "InvalidProperty")
            );
        }
        return GetValue(memberInfo: memberInfos[0], instance: instance);
    }

    public static void SetValue(object instance, string memberName, object value)
    {
        MemberInfo[] memberInfos = instance
            .GetType()
            .GetMember(
                name: memberName,
                bindingAttr: BindingFlags.Static
                    | BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
            );
        if (memberInfos.Length == 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(memberName),
                actualValue: memberName,
                message: ResourceUtils.GetString(key: "InvalidProperty")
            );
        }
        SetValue(memberInfo: memberInfos[0], instance: instance, value: value);
    }

    public static object GetValue(MemberInfo memberInfo, object instance)
    {
        var propertyInfo = memberInfo as PropertyInfo;
        var fieldInfo = memberInfo as FieldInfo;
        if (propertyInfo != null)
        {
            return propertyInfo.GetValue(obj: instance, index: Array.Empty<object>());
        }
        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(obj: instance);
        }
        throw new ArgumentOutOfRangeException(
            paramName: memberInfo.Name,
            actualValue: memberInfo,
            message: ResourceUtils.GetString(key: "UnsupportedType")
        );
    }

#if DEBUG
    static readonly Hashtable reflectionAnalyzer = new Hashtable();
#endif

    public static void SetValue(MemberInfo memberInfo, object instance, object value)
    {
        var propertyInfo = memberInfo as PropertyInfo;
        if (propertyInfo != null)
        {
            if (!propertyInfo.CanWrite)
            {
                return;
            }
        }
#if DEBUGX
        Hashtable memberAnalyzer;
        if (reflectionAnalyzer.Contains(memberInfo.DeclaringType))
        {
            memberAnalyzer = reflectionAnalyzer[memberInfo.DeclaringType] as Hashtable;
        }
        else
        {
            memberAnalyzer = new Hashtable();
            reflectionAnalyzer.Add(memberInfo.DeclaringType, memberAnalyzer);
        }
        if (!memberAnalyzer.Contains(memberInfo.Name))
        {
            memberAnalyzer.Add(memberInfo.Name, memberInfo);
        }
#endif
        try
        {
            ActivateCache();
            // first we try cache
            if (classCache.SetValue(instance: instance, property: memberInfo.Name, value: value))
            {
                return;
            }
            // Set the value
            if (propertyInfo != null)
            {
                // convert from decimal to int
                object finalValue;
                if ((propertyInfo.PropertyType == typeof(int)) && (value is decimal decimalValue))
                {
                    finalValue = Convert.ToInt32(value: decimalValue);
                }
                else
                {
                    finalValue = value;
                }
                propertyInfo.SetValue(
                    obj: instance,
                    value: finalValue,
                    invokeAttr: SearchCriteria,
                    binder: null,
                    index: null,
                    culture: null
                );
            }
            else
            {
                (memberInfo as FieldInfo)?.SetValue(
                    obj: instance,
                    value: value,
                    invokeAttr: SearchCriteria,
                    binder: null,
                    culture: null
                );
            }
        }
        catch (Exception ex)
        {
            throw new Exception(
                message: $"Failed setting {nameof(value)} '{value}' ({(value == null ? "null" : value.GetType().Name)}) "
                    + $"to {memberInfo.MemberType.ToString()} {instance.GetType().Name}.{memberInfo.Name} "
                    + $"({(propertyInfo == null ? (memberInfo as FieldInfo)?.FieldType.Name : propertyInfo.PropertyType.Name)})",
                innerException: ex
            );
        }
    }

    public static Type GetTypeByName(string typeName)
    {
        string assemblyName = typeName.Substring(
            startIndex: 0,
            length: typeName.LastIndexOf(value: '.')
        );
        return Type.GetType(typeName: typeName + "," + assemblyName);
    }

#if DEBUG
    public static string GenerateReflectorCacheMethods()
    {
        var result = new StringBuilder();
        result.Append(value: "public object InvokeObject(Type type, object[] args)");
        result.AppendLine();
        result.Append(value: "{");
        result.AppendLine();
        result.Append(value: "\tOrigam.Key key = args[0] as Origam.Key;");
        result.AppendLine();
        result.Append(value: "\tswitch(type.ToString())");
        result.AppendLine();
        result.Append(value: "\t{");
        result.AppendLine();
        foreach (DictionaryEntry entry in reflectionAnalyzer)
        {
            Type classType = entry.Key as Type;
            if (classType is { IsAbstract: true })
            {
                continue;
            }
            result.AppendFormat(format: "\t\tcase \"{0}\":", arg0: classType);
            result.AppendLine();
            result.AppendFormat(format: "\t\t\treturn new {0}(key);", arg0: classType);
            result.AppendLine();
        }
        result.Append(value: "\t}");
        result.AppendLine();
        result.Append(value: "\treturn null;");
        result.AppendLine();
        result.Append(value: "}");
        result.AppendLine();
        return result.ToString();
    }

    public static string GenerateReflectorCacheMethods2()
    {
        var result = new StringBuilder();
        result.Append(
            value: "public bool SetValue(object instance, string property, object value)"
        );
        result.AppendLine();
        result.Append(value: "{");
        result.AppendLine();
        foreach (DictionaryEntry entry in reflectionAnalyzer)
        {
            Type classType = entry.Key as Type;
            // class type begin
            result.AppendFormat(format: "\tif(instance is {0})", arg0: classType);
            result.AppendLine();
            result.Append(value: "\t{");
            result.AppendLine();
            result.Append(value: "\t\tswitch(property)");
            result.AppendLine();
            result.Append(value: "\t\t{");
            result.AppendLine();
            if (entry.Value is Hashtable memberAnalyzer)
            {
                foreach (DictionaryEntry memberEntry in memberAnalyzer)
                {
                    Type type;
                    var memberInfo = memberEntry.Value as MemberInfo;
                    type = memberInfo switch
                    {
                        FieldInfo info => info.FieldType,
                        PropertyInfo info => info.PropertyType,
                        _ => throw new Exception(message: "Member must be property or field."),
                    };
                    result.AppendFormat(format: "\t\t\tcase \"{0}\":", arg0: memberInfo.Name);
                    result.AppendLine();
                    result.AppendFormat(
                        format: "\t\t\t\t(instance as {0}).{1} = ",
                        arg0: classType,
                        arg1: memberInfo.Name
                    );
                    switch (type.ToString())
                    {
                        case "System.String":
                        {
                            result.Append(value: "value == null ? null : (string)value;");
                            break;
                        }
                        case "System.Int32":
                        {
                            result.Append(value: "(int)value;");
                            break;
                        }
                        case "System.Int64":
                        {
                            result.Append(value: "(long)value;");
                            break;
                        }
                        case "System.Guid":
                        {
                            result.Append(value: "value == null ? Guid.Empty : (Guid)value;");
                            break;
                        }
                        default:
                        {
                            result.AppendFormat(format: "({0})value;", arg0: type);
                            break;
                        }
                    }
                    result.AppendLine();
                    result.AppendFormat(format: "\t\t\t\treturn true;");
                    result.AppendLine();
                }
            }
            result.Append(value: "\t\t}");
            result.AppendLine();
            result.Append(value: "\t}");
            result.AppendLine();
        }
        result.Append(value: "\treturn false;");
        result.AppendLine();
        result.Append(value: "}");
        result.AppendLine();
        return result.ToString();
    }
#endif
}
