using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Origam.Extensions
{
    public static class TypeExtensions
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);

        public static IEnumerable<Type> GetAllPublicSubTypes(this Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(GetExportedTypes)
                .Where(baseType.IsAssignableFrom);
        }
        
        public static object GetValue(this MemberInfo memberInfo, object forObject)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(forObject);
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(forObject);
                default:
                    throw new NotImplementedException();
            }
        } 

        private static IEnumerable<Type> GetExportedTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (FileNotFoundException)
            {
                return new List<Type>();
            }
            catch (TypeLoadException ex)
            {
                log.Warn("Could not load assembly: "+assembly.Location+", reason: "+ex.Message);
                return new List<Type>();
            }
        }
    }
}