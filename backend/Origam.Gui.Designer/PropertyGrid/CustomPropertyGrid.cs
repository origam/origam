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

/* All code is written my me,Ben Ratzlaff and is available for any free use except where noted.
 *I assume no responsibility for how anyone uses the information available*/

//uncomment this line to have the program save the emitted assembly to Test.dll
#define SaveDLL

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Forms;

namespace Origam.Gui.Designer.PropertyGrid;

/// <summary>
/// A property grid that dynamically generates a Type to conform to desired input
/// </summary>
public class CustomPropertyGrid : System.Windows.Forms.PropertyGrid
{
    private Hashtable typeHash;
    private string typeName = "DefType";
    private Settings settings;
    private bool instantUpdate = true;

    public CustomPropertyGrid()
    {
        initTypes();
    }

    [Description(description: "Name of the type that will be internally created")]
    [DefaultValue(value: "DefType")]
    public string TypeName
    {
        get { return typeName; }
        set { typeName = value; }
    }

    [DefaultValue(value: true)]
    [Description(
        description: "If true, the Setting.Update() event will be called when a property changes"
    )]
    public bool InstantUpdate
    {
        get { return instantUpdate; }
        set { instantUpdate = value; }
    }

    protected override void OnPropertyValueChanged(PropertyValueChangedEventArgs e)
    {
        base.OnPropertyValueChanged(e: e);
        if (settings == null)
        {
            return;
        }
        ((Setting)settings[key: e.ChangedItem.Label]).Value = e.ChangedItem.Value;
        if (instantUpdate)
        {
            ((Setting)settings[key: e.ChangedItem.Label]).FireUpdate(e: e);
        }
    }

    [Browsable(browsable: false)]
    public Settings Settings
    {
        set
        {
            settings = value;
            //Reflection.Emit code below copied and modified from http://longhorn.msdn.microsoft.com/lhsdk/ref/ns/system.reflection.emit/c/propertybuilder/propertybuilder.aspx
            AppDomain myDomain = Thread.GetDomain();
            AssemblyName myAsmName = new AssemblyName();
            myAsmName.Name = "TempAssembly";
            //Only save the custom-type dll while debugging
#if SaveDLL && DEBUG
            AssemblyBuilder assemblyBuilder = myDomain.DefineDynamicAssembly(
                name: myAsmName,
                access: AssemblyBuilderAccess.RunAndSave
            );
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(
                name: "TempModule",
                fileName: "Test.dll"
            );
#else
            AssemblyBuilder assemblyBuilder = myDomain.DefineDynamicAssembly(
                myAsmName,
                AssemblyBuilderAccess.Run
            );
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("TempModule");
#endif
            //create our type
            TypeBuilder newType = moduleBuilder.DefineType(
                name: typeName,
                attr: TypeAttributes.Public,
                parent: typeof(System.ComponentModel.Component)
            );
            //create the hashtable used to store property values
            FieldBuilder hashField = newType.DefineField(
                fieldName: "table",
                type: typeof(Hashtable),
                attributes: FieldAttributes.Private
            );
            createHashMethod(
                propBuild: newType.DefineProperty(
                    name: "Hash",
                    attributes: PropertyAttributes.None,
                    returnType: typeof(Hashtable),
                    parameterTypes: new Type[] { }
                ),
                typeBuild: newType,
                hash: hashField
            );

            Hashtable h = new Hashtable();
            foreach (string key in settings.Keys)
            {
                Setting s = settings[key: key];
                h[key: key] = s.Value;
                emitProperty(tb: newType, hash: hashField, s: s, name: key);
            }
            Type myType = newType.CreateType();
#if SaveDLL && DEBUG
            assemblyBuilder.Save(assemblyFileName: "Test.dll");
#endif
            ConstructorInfo ci = myType.GetConstructor(types: new Type[] { });
            System.ComponentModel.Component o =
                ci.Invoke(parameters: new Object[] { }) as System.ComponentModel.Component;
            //set the object's hashtable - in the future i would like to do this in the emitted object's constructor
            PropertyInfo pi = myType.GetProperty(name: "Hash");
            pi.SetValue(obj: o, value: h, index: null);
            //o.Site = new Origam.Gui.Designer.SiteImpl(o, "propertyComponent", settings.DesignerHost);
            //settings.DesignerHost.Add(o, "propertyComponent");
            SelectedObject = o;
        }
    }

    private void createHashMethod(
        PropertyBuilder propBuild,
        TypeBuilder typeBuild,
        FieldBuilder hash
    )
    {
        // First, we'll define the behavior of the "get" property for Hash as a method.
        MethodBuilder typeHashGet = typeBuild.DefineMethod(
            name: "GetHash",
            attributes: MethodAttributes.Public,
            returnType: typeof(Hashtable),
            parameterTypes: new Type[] { }
        );
        ILGenerator ilg = typeHashGet.GetILGenerator();
        ilg.Emit(opcode: OpCodes.Ldarg_0);
        ilg.Emit(opcode: OpCodes.Ldfld, field: hash);
        ilg.Emit(opcode: OpCodes.Ret);
        // Now, we'll define the behavior of the "set" property for Hash.
        MethodBuilder typeHashSet = typeBuild.DefineMethod(
            name: "SetHash",
            attributes: MethodAttributes.Public,
            returnType: null,
            parameterTypes: new Type[] { typeof(Hashtable) }
        );
        ilg = typeHashSet.GetILGenerator();
        ilg.Emit(opcode: OpCodes.Ldarg_0);
        ilg.Emit(opcode: OpCodes.Ldarg_1);
        ilg.Emit(opcode: OpCodes.Stfld, field: hash);
        ilg.Emit(opcode: OpCodes.Ret);
        // map the two methods created above to their property
        propBuild.SetGetMethod(mdBuilder: typeHashGet);
        propBuild.SetSetMethod(mdBuilder: typeHashSet);
        //add the [Browsable(false)] property to the Hash property so it doesnt show up on the property list
        ConstructorInfo ci = typeof(BrowsableAttribute).GetConstructor(
            types: new Type[] { typeof(bool) }
        );
        CustomAttributeBuilder cab = new CustomAttributeBuilder(
            con: ci,
            constructorArgs: new object[] { false }
        );
        propBuild.SetCustomAttribute(customBuilder: cab);
    }

    /// <summary>
    /// Initialize a private hashtable with type-opCode pairs so i dont have to write a long if/else statement when outputting msil
    /// </summary>
    private void initTypes()
    {
        typeHash = new Hashtable();
        typeHash[key: typeof(sbyte)] = OpCodes.Ldind_I1;
        typeHash[key: typeof(byte)] = OpCodes.Ldind_U1;
        typeHash[key: typeof(char)] = OpCodes.Ldind_U2;
        typeHash[key: typeof(short)] = OpCodes.Ldind_I2;
        typeHash[key: typeof(ushort)] = OpCodes.Ldind_U2;
        typeHash[key: typeof(int)] = OpCodes.Ldind_I4;
        typeHash[key: typeof(uint)] = OpCodes.Ldind_U4;
        typeHash[key: typeof(long)] = OpCodes.Ldind_I8;
        typeHash[key: typeof(ulong)] = OpCodes.Ldind_I8;
        typeHash[key: typeof(bool)] = OpCodes.Ldind_I1;
        typeHash[key: typeof(double)] = OpCodes.Ldind_R8;
        typeHash[key: typeof(float)] = OpCodes.Ldind_R4;
    }

    /// <summary>
    /// emits a generic get/set property in which the result returned resides in a hashtable whos key is the name of the property
    /// </summary>
    /// <param name="pb">PropertyBuilder used to emit</param>
    /// <param name="tb">TypeBuilder of the class</param>
    /// <param name="hash">FieldBuilder of the hashtable used to store the object</param>
    /// <param name="po">PropertyObject of this property</param>
    private void emitProperty(TypeBuilder tb, FieldBuilder hash, Setting s, string name)
    {
        //to figure out what opcodes to emit, i would compile a small class having the functionality i wanted, and viewed it with ildasm.
        //peverify is also kinda nice to use to see what errors there are.

        //define the property first
        PropertyBuilder pb = tb.DefineProperty(
            name: name,
            attributes: PropertyAttributes.None,
            returnType: s.Type,
            parameterTypes: new Type[] { }
        );
        Type objType = s.Type;
        //now we define the get method for the property
        MethodBuilder getMethod = tb.DefineMethod(
            name: "get_" + name,
            attributes: MethodAttributes.Public,
            returnType: objType,
            parameterTypes: new Type[] { }
        );
        ILGenerator ilg = getMethod.GetILGenerator();
        ilg.DeclareLocal(localType: objType);
        ilg.Emit(opcode: OpCodes.Ldarg_0);
        ilg.Emit(opcode: OpCodes.Ldfld, field: hash);
        ilg.Emit(opcode: OpCodes.Ldstr, str: name);

        ilg.EmitCall(
            opcode: OpCodes.Callvirt,
            methodInfo: typeof(Hashtable).GetMethod(name: "get_Item"),
            optionalParameterTypes: null
        );
        if (objType.IsValueType)
        {
            ilg.Emit(opcode: OpCodes.Unbox, cls: objType);
            if (typeHash[key: objType] != null)
            {
                ilg.Emit(opcode: (OpCode)typeHash[key: objType]);
            }
            else
            {
                ilg.Emit(opcode: OpCodes.Ldobj, cls: objType);
            }
        }
        else
        {
            ilg.Emit(opcode: OpCodes.Castclass, cls: objType);
        }

        ilg.Emit(opcode: OpCodes.Stloc_0);
        ilg.Emit(opcode: OpCodes.Br_S, arg: (byte)0);
        ilg.Emit(opcode: OpCodes.Ldloc_0);
        ilg.Emit(opcode: OpCodes.Ret);
        //now we generate the set method for the property
        MethodBuilder setMethod = tb.DefineMethod(
            name: "set_" + name,
            attributes: MethodAttributes.Public,
            returnType: null,
            parameterTypes: new Type[] { objType }
        );
        ilg = setMethod.GetILGenerator();
        ilg.Emit(opcode: OpCodes.Ldarg_0);
        ilg.Emit(opcode: OpCodes.Ldfld, field: hash);
        ilg.Emit(opcode: OpCodes.Ldstr, str: name);
        ilg.Emit(opcode: OpCodes.Ldarg_1);
        if (objType.IsValueType)
        {
            ilg.Emit(opcode: OpCodes.Box, cls: objType);
        }

        ilg.EmitCall(
            opcode: OpCodes.Callvirt,
            methodInfo: typeof(Hashtable).GetMethod(name: "set_Item"),
            optionalParameterTypes: null
        );
        ilg.Emit(opcode: OpCodes.Ret);
        //put the get/set methods in with the property
        pb.SetGetMethod(mdBuilder: getMethod);
        pb.SetSetMethod(mdBuilder: setMethod);
        //if we specified a description, we will now add the DescriptionAttribute to our property
        if (s.Description != null)
        {
            ConstructorInfo ci = typeof(DescriptionAttribute).GetConstructor(
                types: new Type[] { typeof(string) }
            );
            CustomAttributeBuilder cab = new CustomAttributeBuilder(
                con: ci,
                constructorArgs: new object[] { s.Description }
            );
            pb.SetCustomAttribute(customBuilder: cab);
        }
        //add a CategoryAttribute if specified
        if (s.Category != null)
        {
            ConstructorInfo ci = typeof(CategoryAttribute).GetConstructor(
                types: new Type[] { typeof(string) }
            );
            CustomAttributeBuilder cab = new CustomAttributeBuilder(
                con: ci,
                constructorArgs: new object[] { s.Category }
            );
            pb.SetCustomAttribute(customBuilder: cab);
        }
        if (s.TypeConverter != null)
        {
            ConstructorInfo ci = s.TypeConverter.GetType().GetConstructor(types: new Type[] { });
            CustomAttributeBuilder cab = new CustomAttributeBuilder(
                con: ci,
                constructorArgs: new object[] { }
            );
            pb.SetCustomAttribute(customBuilder: cab);
        }
        if (s.UITypeEditor != null)
        {
            ConstructorInfo ci = s.UITypeEditor.GetType().GetConstructor(types: new Type[] { });
            CustomAttributeBuilder cab = new CustomAttributeBuilder(
                con: ci,
                constructorArgs: new object[] { }
            );
            pb.SetCustomAttribute(customBuilder: cab);
        }
    }
}
