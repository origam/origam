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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Origam.Extensions;

namespace Origam;

public class Reflector
{
	static public readonly BindingFlags SearchCriteria = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
	static private Hashtable memberTypeCache = new Hashtable();
	static private IReflectorCache classCache;
	private static readonly log4net.ILog log
		= log4net.LogManager.GetLogger(
			MethodBase.GetCurrentMethod().DeclaringType);

	protected Reflector() 
	{
	}

	static public IReflectorCache ClassCache
	{
		get
		{
			return classCache;
		}
		set
		{
			classCache = value;
		}
	}
		
	static public IList FindConstructors( Type type )
	{
		ArrayList result = new ArrayList();
		foreach( ConstructorInfo constructorInfo in type.GetConstructors( SearchCriteria ) )
		{
			// exclude abstract constructors (weird concept anyway)
			if( ! constructorInfo.IsAbstract )
				result.Add( constructorInfo );
		}
		return result;
	}
	public static void CopyMembers(object source, object target, Type[] attributeTypes)
	{
		FindMembers(source.GetType(), attributeTypes)
			.ForEach(attrInfo =>
			{
				MemberInfo memberInfo = attrInfo.MemberInfo;
				if (memberInfo is PropertyInfo propInfo)
				{
					object value = propInfo.GetValue(source);
					propInfo.SetValue(target, value);
				} else if (memberInfo is FieldInfo fieldInfo)
				{
					object value = fieldInfo.GetValue(source);
					fieldInfo.SetValue(target, value);
				}
			});
	}

	public static List<MemberAttributeInfo> FindMembers(Type type,
		IEnumerable<Type> primaryAttributes)
	{
		return primaryAttributes
			.SelectMany(attr => FindMembers(type, attr).Cast<MemberAttributeInfo>())
			.ToList();
	}

	// this signature causes nothing to be returned.. very strange
	// static public MemberAttributeInfo[] FindMembers( Type type, Type primaryAttribute, params Type[] secondaryAttributes )
	static public IList FindMembers( Type type, Type primaryAttribute, params Type[] secondaryAttributes )
	{
		// cache only requests without secondary attributes
		if(secondaryAttributes.Length == 0)
		{
			if(memberTypeCache.ContainsKey(type))
			{
				if((memberTypeCache[type] as Hashtable).ContainsKey(primaryAttribute))
				{
					return (memberTypeCache[type] as Hashtable)[primaryAttribute] as IList;
				}
			}
			else
			{
				lock(memberTypeCache)
				{
					memberTypeCache.Add(type, new Hashtable());
				}
			}
		}

		ArrayList result = new ArrayList();
		foreach( MemberInfo memberInfo in type.GetMembers( SearchCriteria ) )
		{
			object[] attrs = memberInfo.GetCustomAttributes( primaryAttribute, true );
			if( attrs != null)
			{
				foreach (object attr in attrs)
				{
					IList memberAttrs = FindAttributes(memberInfo, secondaryAttributes);
					result.Add(new MemberAttributeInfo(memberInfo, attr as Attribute, memberAttrs));
				}
			}
		}
		// return result.ToArray() as MemberAttributeInfo[];

		lock(memberTypeCache)
		{
			if(! (memberTypeCache[type] as Hashtable).Contains(primaryAttribute))
				(memberTypeCache[type] as Hashtable).Add(primaryAttribute, result);
		}

		return result;
	}

	//static public Attribute[] FindAttributes( MemberInfo memberInfo, params Type[] attributes )
	static public IList FindAttributes( MemberInfo memberInfo, params Type[] attributes )
	{
		ArrayList result = new ArrayList();
		foreach( Type attribute in attributes )
		{
			object[] attrs = memberInfo.GetCustomAttributes( attribute, true );
			if( attrs != null && attrs.Length == 1 )
				result.Add( attrs[ 0 ] as Attribute );
		}
		// return result.Count == 0 ? null : result.ToArray() as Attribute[];
		return result;
	}

	static private void ActivateCache()
	{
		if(classCache == null)
		{
			// new way a)
			classCache = Activator.CreateInstance(
					Type.GetType("Origam.ReflectorCache.ReflectorCache,Origam.ReflectorCache"))
				as IReflectorCache;

			// new way b)
			//Assembly a = Assembly.Load("Origam.ReflectorCache");
			//classCache = a.CreateInstance("Origam.ReflectorCache.ReflectorCache") as IReflectorCache;

			// old way
			//System.Runtime.Remoting.ObjectHandle oh = Activator.CreateInstance("Origam.ReflectorCache", "Origam.ReflectorCache.ReflectorCache");
			//classCache = oh.Unwrap() as IReflectorCache;


		}
	}

	private static string ComposeAssemblyPath(string assembly)
	{
		return AppDomain.CurrentDomain.BaseDirectory
		       + assembly.Split(",".ToCharArray())[0].Trim()
		       + ".dll";
	}

	static public object InvokeObject(string classname, string assembly)
	{
		Reflector.ActivateCache();

		object result = null;

		// try to get the object from the cache
		if (classCache != null)
			result = classCache.InvokeObject(classname, assembly);

		if (result != null)
			return result;

		// it was not instanced by cache, so we invoke it using reflection

		// new way - a)
		Type classType = ResolveTypeFromAssembly(classname, assembly);
		if (classType == null)
		{
			throw new Exception($"Class {classname} from assembly {assembly} was not found.");
		}

		return Activator.CreateInstance(classType);

		// new way - b)
		//Assembly a = Assembly.Load(assembly);
		//return a.CreateInstance(classname);

		// old way
		//System.Runtime.Remoting.ObjectHandle oh = Activator.CreateInstance(assembly, classname);
		//return oh.Unwrap();


		//			Assembly foundAssembly=LoadAssembly(assembly);
		//			if(foundAssembly!=null)
		//			{
		//				result=foundAssembly.CreateInstance(classname);
		//			}
		//			return result;
	}

	public static Type ResolveTypeFromAssembly(
		string classname, string assemblyName)
	{
		var classType = Type.GetType(classname + "," + assemblyName);
#if NETSTANDARD
            if (classType == null)
            {
                // try to load assembly to default application context
                // With .NET Core, we need to explicitly load assemblies, that are not a part of the .dep.json file
                var assembly = System.Runtime.Loader.AssemblyLoadContext
                    .Default.LoadFromAssemblyPath(ComposeAssemblyPath(
                        assemblyName));
                classType = assembly.GetType(classname);
                if (log.IsDebugEnabled && classType == null)
                {
	                log.RunHandled(() =>
	                {
	                    log.DebugFormat("Can't resolve type '{0}' from assembly path '{1}'",
	                        classname + "," + assemblyName,
	                        ComposeAssemblyPath(assemblyName));
	                });
                }
            }
#endif
		return classType;
	}

	static public object InvokeObject(string assemblyName, string typeName, object[] args)
	{
		Reflector.ActivateCache();

		object instance = null;

		if(classCache != null)
			instance = classCache.InvokeObject(typeName, args);

		if(instance != null)
			return instance;

		/*
		 * new proposed way a)
		 */

		var classType = ResolveTypeFromAssembly(typeName, assemblyName);
		return Activator.CreateInstance(classType, BindingFlags.DeclaredOnly |
		                                           BindingFlags.Public | BindingFlags.NonPublic |
		                                           BindingFlags.Instance | BindingFlags.CreateInstance, null, args,
			System.Globalization.CultureInfo.CurrentCulture);

		/*
		 * new proposed way b)
		 *
		Assembly a = Assembly.Load(assemblyName);
		return a.CreateInstance(typeName, true, BindingFlags.DeclaredOnly |
		    BindingFlags.Public | BindingFlags.NonPublic |
		    BindingFlags.Instance | BindingFlags.CreateInstance
		    , null, args, System.Globalization.CultureInfo.CurrentCulture, null);
		*/
            
		/*
		 * Old way
		 *
		System.Runtime.Remoting.ObjectHandle oh = Activator.CreateInstance(assemblyName, typeName, false, BindingFlags.DeclaredOnly |
			BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.Instance | BindingFlags.CreateInstance
			, null, args, System.Globalization.CultureInfo.CurrentCulture, null, null);
		return oh.Unwrap();
		*/
	}

	public static object GetValue(Type type, object instance, string memberName)
	{
		MemberInfo[] mi = type.GetMember(memberName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if(mi.Length == 0)
		{
			throw new ArgumentOutOfRangeException("memberName", memberName, ResourceUtils.GetString("InvalidProperty"));
		}
		return GetValue(mi[0], instance);
	}

	public static void SetValue(object instance, string memberName, object value)
	{
		MemberInfo[] mi = instance.GetType().GetMember(
			memberName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if (mi.Length == 0)
		{
			throw new ArgumentOutOfRangeException("memberName", memberName, ResourceUtils.GetString("InvalidProperty"));
		}
		SetValue(mi[0], instance, value);
	}

	public static object GetValue(MemberInfo mi, object instance)
	{
		PropertyInfo pi = mi as PropertyInfo;
		FieldInfo fi = mi as FieldInfo;

		if(pi != null)
		{
			return pi.GetValue(instance, new object[0]);
		}
		else if(fi != null)
		{
			return fi.GetValue(instance);
		}
		else
		{
			throw new ArgumentOutOfRangeException(mi.Name, mi, ResourceUtils.GetString("UnsupportedType"));
		}
	}

#if DEBUG
	static Hashtable _reflectionAnalyzer =  new Hashtable();
#endif

	static public void SetValue(MemberInfo mi, object instance, object value)
	{
		PropertyInfo pi = mi as PropertyInfo;

		if(pi != null)
		{
			if(!pi.CanWrite)	return;
		}
		
#if DEBUGX
			Hashtable memberAnalyzer;
			if(_reflectionAnalyzer.Contains(mi.DeclaringType))
			{
				memberAnalyzer = _reflectionAnalyzer[mi.DeclaringType] as Hashtable;
			}
			else
			{
				memberAnalyzer = new Hashtable();
				_reflectionAnalyzer.Add(mi.DeclaringType, memberAnalyzer);
			}

			if(! memberAnalyzer.Contains(mi.Name))
			{
				memberAnalyzer.Add(mi.Name, mi);
			}
#endif
			
		try
		{
			Reflector.ActivateCache();

			// first we try cache
			if(classCache.SetValue(instance, mi.Name, value)) return;

			// Set the value
			if(pi != null)
			{
				// convert from decimal to int
				object finalValue;
				if(pi.PropertyType == typeof(int) && value is decimal)
				{
					finalValue = Convert.ToInt32((decimal)value);
				}
				else
				{
					finalValue = value;
				}
				pi.SetValue(instance, finalValue, Reflector.SearchCriteria, null, null, null);
			}
			else
			{
				(mi as FieldInfo).SetValue(instance, value, Reflector.SearchCriteria, null, null);
			}
		}
		catch(Exception ex)
		{
			throw new Exception(String.Format("Failed setting value '{0}' ({1}) to {2} {3}.{4} ({5})",
					value,
					(value == null ? "null" : value.GetType().Name),
					mi.MemberType.ToString(),
					instance.GetType().Name,
					mi.Name,
					pi == null ? (mi as FieldInfo).FieldType.Name : pi.PropertyType.Name),
				ex
			);
		}
	}
		
	public static Type GetTypeByName(string typeName)
	{
		string assemblyName = typeName.Substring(0, typeName.LastIndexOf('.'));
		return Type.GetType(typeName + "," + assemblyName);
	}

#if DEBUG
	public static string GenerateReflectorCacheMethods()
	{
		StringBuilder result = new StringBuilder();

		result.Append("public object InvokeObject(Type type, object[] args)" + Environment.NewLine);
		result.Append("{" + Environment.NewLine);
		result.Append("\tOrigam.Key key = args[0] as Origam.Key;" + Environment.NewLine);

		result.Append("\tswitch(type.ToString())" + Environment.NewLine);
		result.Append("\t{" + Environment.NewLine);

		foreach(DictionaryEntry entry in _reflectionAnalyzer)
		{
			Type classType = entry.Key as Type;

			if(!classType.IsAbstract)
			{
				result.AppendFormat("\t\tcase \"{0}\":" + Environment.NewLine, classType.ToString());
				result.AppendFormat("\t\t\treturn new {0}(key);" + Environment.NewLine, classType.ToString());
			}
		}

		result.Append("\t}" + Environment.NewLine);	// switch(type.ToString()...
		result.Append("\treturn null;" + Environment.NewLine);	// if(instance...
		result.Append("}" + Environment.NewLine);	// method end

		return result.ToString();
	}
		
	public static string GenerateReflectorCacheMethods2()
	{
		StringBuilder result = new StringBuilder();

		result.Append("public bool SetValue(object instance, string property, object value)" + Environment.NewLine);
		result.Append("{" + Environment.NewLine);

		foreach(DictionaryEntry entry in _reflectionAnalyzer)
		{
			Type classType = entry.Key as Type;

			// class type begin
			result.AppendFormat("\tif(instance is {0})" + Environment.NewLine, classType.ToString());
			result.Append("\t{" + Environment.NewLine);
			result.Append("\t\tswitch(property)" + Environment.NewLine);
			result.Append("\t\t{" + Environment.NewLine);

			Hashtable memberAnalyzer = entry.Value as Hashtable;
			if(memberAnalyzer != null)
			{
				foreach(DictionaryEntry memberEntry in memberAnalyzer)
				{
					Type type;
					MemberInfo member = memberEntry.Value as MemberInfo; 

					if(member is FieldInfo)
					{
						type = (member as FieldInfo).FieldType;
					}
					else if(member is PropertyInfo)
					{
						type = (member as PropertyInfo).PropertyType;
					}
					else
					{
						throw new Exception("Member must be property or field.");
					}

					result.AppendFormat("\t\t\tcase \"{0}\":" + Environment.NewLine, member.Name);
					result.AppendFormat("\t\t\t\t(instance as {0}).{1} = ", classType.ToString(), member.Name);
						
					switch(type.ToString())
					{
						case "System.String":
							result.Append("value == null ? null : (string)value;" + Environment.NewLine);
							break;

						case "System.Int32":
							result.Append("(int)value;" + Environment.NewLine);
							break;

						case "System.Int64":
							result.Append("(long)value;" + Environment.NewLine);
							break;

						case "System.Guid":
							result.Append("value == null ? Guid.Empty : (Guid)value;" + Environment.NewLine);
							break;

						default:
							result.AppendFormat("({0})value;" + Environment.NewLine, type.ToString());
							break;
					}

					result.AppendFormat("\t\t\t\treturn true;" + Environment.NewLine);
				}
			}

			result.Append("\t\t}" + Environment.NewLine);	// switch(property...
			result.Append("\t}" + Environment.NewLine);	// if(instance...
		}

		result.Append("\treturn false;" + Environment.NewLine);	// if(instance...
		result.Append("}" + Environment.NewLine);	// method end

		return result.ToString();
	}
#endif
}