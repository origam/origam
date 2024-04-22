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

    private static readonly Dictionary<Type, DynamicTypeInfo>
        controlItemMapping =
            new Dictionary<Type, DynamicTypeInfo>();

    public static ControlItem GetAssociatedControlItem(
        Type maybeDynamicType)
    {
            return controlItemMapping.ContainsKey(maybeDynamicType)
                ? controlItemMapping[maybeDynamicType].Inheritor
                : null;
        }

    public static Type GetOriginalType(Type maybeDynamicType)
    {
            return !controlItemMapping.ContainsKey(maybeDynamicType) 
                ? maybeDynamicType 
                : controlItemMapping[maybeDynamicType].OriginalType; 
        }

    private static string CreateNewTypeName(Type parentType)
    {
            return parentType.Name + typeNameSeparator +
                   Guid.NewGuid().ToString().Replace("-", "");
        }

    public DynamicTypeFactory()
    {
            var uniqueIdentifier = Guid.NewGuid().ToString().Replace("-", "");
            var assemblyName = new AssemblyName(uniqueIdentifier);

            var assemblyBuilder =
                AssemblyBuilder.DefineDynamicAssembly(assemblyName,
                    AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder =
                assemblyBuilder.DefineDynamicModule(uniqueIdentifier);
        }

    public Type CreateNewTypeWithDynamicProperties(Type parentType,
        ControlItem inheritor,
        IEnumerable<DynamicProperty> dynamicProperties)
    {
            typeBuilder =
                moduleBuilder.DefineType(CreateNewTypeName(parentType),
                    TypeAttributes.Public);
            typeBuilder.SetParent(parentType);

            foreach (DynamicProperty property in dynamicProperties)
                AddDynamicPropertyToType(property, inheritor);

            Type childType = typeBuilder.CreateType();
            controlItemMapping[childType] = new DynamicTypeInfo(
                inheritor: inheritor,
                originalType: parentType);
            return childType;
        }

    private void AddDynamicPropertyToType(DynamicProperty dynamicProperty,
        ControlItem inheritor)
    {
            Type propertyType = dynamicProperty.SystemType;
            string fieldName = $"_{dynamicProperty.Name}";

            FieldBuilder fieldBuilder = typeBuilder.DefineField(fieldName,
                propertyType, FieldAttributes.Private);

            MethodAttributes getSetAttributes = MethodAttributes.Public |
                                                MethodAttributes.SpecialName |
                                                MethodAttributes.HideBySig;

            MethodBuilder getMethodBuilder =
                typeBuilder.DefineMethod($"get_{dynamicProperty.Name}",
                    getSetAttributes, propertyType, Type.EmptyTypes);
            ILGenerator propertyGetGenerator =
                getMethodBuilder.GetILGenerator();
            propertyGetGenerator.Emit(OpCodes.Ldarg_0);
            propertyGetGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            propertyGetGenerator.Emit(OpCodes.Ret);

            MethodBuilder setMethodBuilder =
                typeBuilder.DefineMethod($"set_{dynamicProperty.Name}",
                    getSetAttributes, null, new Type[] { propertyType });
            ILGenerator propertySetGenerator =
                setMethodBuilder.GetILGenerator();
            propertySetGenerator.Emit(OpCodes.Ldarg_0);
            propertySetGenerator.Emit(OpCodes.Ldarg_1);
            propertySetGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            propertySetGenerator.Emit(OpCodes.Ret);

            PropertyBuilder propertyBuilder =
                typeBuilder.DefineProperty(dynamicProperty.Name,
                    PropertyAttributes.HasDefault, propertyType, null);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);

            var categoryAttributeBuilder = new CustomAttributeBuilder(
                typeof(CategoryAttribute).GetConstructor(new[]
                    { typeof(string) }),
                new object[] { dynamicProperty.Category },
                new PropertyInfo[] { },
                new object[] { }
            );
            propertyBuilder.SetCustomAttribute(categoryAttributeBuilder);
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