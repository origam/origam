using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Origam.DA.Common;
using Origam.DA.Common.ObjectPersistence.Attributes;

namespace Origam.DA.Service.NamespaceMapping
{
    static class XmlNamespaceTools
    {
        private static readonly ConcurrentDictionary<Type, OrigamNameSpace> namespaces 
            =  new ConcurrentDictionary<Type, OrigamNameSpace>();
        private static readonly ConcurrentDictionary<Type, string> namespaceNames
            =  new ConcurrentDictionary<Type, string>();
        public static OrigamNameSpace GetXmlNameSpace(Type type)
        {
            return
                namespaces.GetOrAdd(type, type1 =>
                {
                    Version currentClassVersion = Versions.GetCurrentClassVersion(type1);
                    return OrigamNameSpace.CreateOrGet(type1, currentClassVersion);
                });
        }

        public static string GetXmlNamespaceName(Type type)
        {
            return
                namespaceNames.GetOrAdd(
                    type, 
                    typePar =>
                    {
                        if (typePar.GetCustomAttribute(typeof(XmlNamespaceNameAttribute)) 
                            is XmlNamespaceNameAttribute attribute)
                        {
                            return attribute.XmlNamespaceName;
                        }
                        return GetXmlNamespaceName(typePar.Name);
                    });
        }

        public static string GetXmlNamespaceName(string typeName)
        {
            var charArray = typeName
                .Where(char.IsUpper)
                .Select(char.ToLower)
                .ToArray();
            return new string(charArray);
        }
    }
}