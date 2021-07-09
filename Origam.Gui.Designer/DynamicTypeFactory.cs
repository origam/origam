using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;

namespace Origam
{
  public class DynamicTypeFactory
    {
        private TypeBuilder typeBuilder;
        private readonly AssemblyBuilder assemblyBuilder;
        private readonly ModuleBuilder   moduleBuilder;
        private static readonly string typeNameSeparator = "__--__";

        private static Dictionary<Type, Type> originalTypeMapping =
            new Dictionary<Type, Type>();
        
        public static Type GetOriginalType(Type maybeDynamicType)
        {
            return !originalTypeMapping.ContainsKey(maybeDynamicType) 
                ? maybeDynamicType 
                : originalTypeMapping[maybeDynamicType];
        }
        
        private static string CreateNewTypeName(Type parentType)
        {
            return parentType.Name + typeNameSeparator +  Guid.NewGuid().ToString().Replace("-","");
        }
        
        public DynamicTypeFactory()
        {
            var uniqueIdentifier = Guid.NewGuid().ToString().Replace("-","");
            var assemblyName = new AssemblyName(uniqueIdentifier);

            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(uniqueIdentifier);
        }
        
        public Type CreateNewTypeWithDynamicProperties(Type parentType, IEnumerable<DynamicProperty> dynamicProperties)
        {
            typeBuilder = moduleBuilder.DefineType(CreateNewTypeName(parentType), TypeAttributes.Public);
            typeBuilder.SetParent(parentType);

            foreach (DynamicProperty property in dynamicProperties)
                AddDynamicPropertyToType(property);

            Type childType = typeBuilder.CreateType();
            originalTypeMapping[childType] = parentType;
            return childType;
        }

        private void AddDynamicPropertyToType(DynamicProperty dynamicProperty)
        {
            Type   propertyType = dynamicProperty.SystemType;
            string fieldName    = $"_{dynamicProperty.Name}";

            FieldBuilder fieldBuilder = typeBuilder.DefineField(fieldName, propertyType, FieldAttributes.Private);
            
            MethodAttributes getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            
            MethodBuilder getMethodBuilder     = typeBuilder.DefineMethod($"get_{dynamicProperty.Name}", getSetAttributes, propertyType, Type.EmptyTypes);
            ILGenerator   propertyGetGenerator = getMethodBuilder.GetILGenerator();
            propertyGetGenerator.Emit(OpCodes.Ldarg_0);
            propertyGetGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            propertyGetGenerator.Emit(OpCodes.Ret);
            
            MethodBuilder setMethodBuilder     = typeBuilder.DefineMethod($"set_{dynamicProperty.Name}", getSetAttributes, null, new Type[] { propertyType });
            ILGenerator   propertySetGenerator = setMethodBuilder.GetILGenerator();
            propertySetGenerator.Emit(OpCodes.Ldarg_0);
            propertySetGenerator.Emit(OpCodes.Ldarg_1);
            propertySetGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            propertySetGenerator.Emit(OpCodes.Ret);
            
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(dynamicProperty.Name, PropertyAttributes.HasDefault, propertyType, null);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
            
            var attributeType    = typeof(CategoryAttribute);
            var attributeBuilder = new CustomAttributeBuilder(
                attributeType.GetConstructor(new [] { typeof(string) }), 
                new object[] { dynamicProperty.Category }, 
                new PropertyInfo[] { },                  
                new object[] { } 
            );
            propertyBuilder.SetCustomAttribute(attributeBuilder);
        }
    }
  
  public class DynamicProperty
  {
      public string Name { get; set; }
      public Type SystemType { get; set; }
      public string Category { get; set; }
  }
}