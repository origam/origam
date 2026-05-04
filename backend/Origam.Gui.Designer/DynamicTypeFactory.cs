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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using Origam.Schema.GuiModel;

namespace Origam;

public class DynamicTypeFactory
{
    private TypeBuilder typeBuilder;
    private readonly ModuleBuilder moduleBuilder;
    private static readonly string typeNameSeparator = "__--__";
    private static readonly Dictionary<Type, DynamicTypeInfo> controlItemMapping =
        new Dictionary<Type, DynamicTypeInfo>();

    public static ControlItem GetAssociatedControlItem(Type maybeDynamicType)
    {
        return controlItemMapping.ContainsKey(key: maybeDynamicType)
            ? controlItemMapping[key: maybeDynamicType].Inheritor
            : null;
    }

    public static Type GetOriginalType(Type maybeDynamicType)
    {
        return !controlItemMapping.ContainsKey(key: maybeDynamicType)
            ? maybeDynamicType
            : controlItemMapping[key: maybeDynamicType].OriginalType;
    }

    private static string CreateNewTypeName(Type parentType)
    {
        return parentType.Name
            + typeNameSeparator
            + Guid.NewGuid().ToString().Replace(oldValue: "-", newValue: "");
    }

    public DynamicTypeFactory()
    {
        var uniqueIdentifier = Guid.NewGuid().ToString().Replace(oldValue: "-", newValue: "");
        var assemblyName = new AssemblyName(assemblyName: uniqueIdentifier);
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            name: assemblyName,
            access: AssemblyBuilderAccess.RunAndCollect
        );
        moduleBuilder = assemblyBuilder.DefineDynamicModule(name: uniqueIdentifier);
    }

    public Type CreateNewTypeWithDynamicProperties(
        Type parentType,
        ControlItem inheritor,
        IEnumerable<DynamicProperty> dynamicProperties
    )
    {
        typeBuilder = moduleBuilder.DefineType(
            name: CreateNewTypeName(parentType: parentType),
            attr: TypeAttributes.Public
        );
        typeBuilder.SetParent(parent: parentType);
        foreach (DynamicProperty property in dynamicProperties)
        {
            AddDynamicPropertyToType(dynamicProperty: property, inheritor: inheritor);
        }

        Type childType = typeBuilder.CreateType();
        controlItemMapping[key: childType] = new DynamicTypeInfo(
            originalType: parentType,
            inheritor: inheritor
        );
        return childType;
    }

    private void AddDynamicPropertyToType(DynamicProperty dynamicProperty, ControlItem inheritor)
    {
        Type propertyType = dynamicProperty.SystemType;
        string fieldName = $"_{dynamicProperty.Name}";
        FieldBuilder fieldBuilder = typeBuilder.DefineField(
            fieldName: fieldName,
            type: propertyType,
            attributes: FieldAttributes.Private
        );
        MethodAttributes getSetAttributes =
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
        MethodBuilder getMethodBuilder = typeBuilder.DefineMethod(
            name: $"get_{dynamicProperty.Name}",
            attributes: getSetAttributes,
            returnType: propertyType,
            parameterTypes: Type.EmptyTypes
        );
        ILGenerator propertyGetGenerator = getMethodBuilder.GetILGenerator();
        propertyGetGenerator.Emit(opcode: OpCodes.Ldarg_0);
        propertyGetGenerator.Emit(opcode: OpCodes.Ldfld, field: fieldBuilder);
        propertyGetGenerator.Emit(opcode: OpCodes.Ret);
        MethodBuilder setMethodBuilder = typeBuilder.DefineMethod(
            name: $"set_{dynamicProperty.Name}",
            attributes: getSetAttributes,
            returnType: null,
            parameterTypes: new Type[] { propertyType }
        );
        ILGenerator propertySetGenerator = setMethodBuilder.GetILGenerator();
        propertySetGenerator.Emit(opcode: OpCodes.Ldarg_0);
        propertySetGenerator.Emit(opcode: OpCodes.Ldarg_1);
        propertySetGenerator.Emit(opcode: OpCodes.Stfld, field: fieldBuilder);
        propertySetGenerator.Emit(opcode: OpCodes.Ret);
        PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
            name: dynamicProperty.Name,
            attributes: PropertyAttributes.HasDefault,
            returnType: propertyType,
            parameterTypes: null
        );
        propertyBuilder.SetGetMethod(mdBuilder: getMethodBuilder);
        propertyBuilder.SetSetMethod(mdBuilder: setMethodBuilder);
        var categoryAttributeBuilder = new CustomAttributeBuilder(
            con: typeof(CategoryAttribute).GetConstructor(types: new[] { typeof(string) }),
            constructorArgs: new object[] { dynamicProperty.Category },
            namedProperties: new PropertyInfo[] { },
            propertyValues: new object[] { }
        );
        propertyBuilder.SetCustomAttribute(customBuilder: categoryAttributeBuilder);
    }
}

public class DynamicProperty
{
    public string Name { get; set; }
    public Type SystemType { get; set; }
    public string Category { get; set; }
}

class DynamicTypeInfo
{
    public Type OriginalType { get; }
    public ControlItem Inheritor { get; }

    public DynamicTypeInfo(Type originalType, ControlItem inheritor)
    {
        OriginalType = originalType;
        Inheritor = inheritor;
    }
}
