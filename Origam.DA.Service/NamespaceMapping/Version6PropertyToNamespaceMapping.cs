using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Origam.DA.Service.NamespaceMapping
{
    class Version6PropertyToNamespaceMapping : PropertyToNamespaceMapping
    {
        private static readonly ConcurrentDictionary<Type, Version6PropertyToNamespaceMapping> instances
            = new ConcurrentDictionary<Type, Version6PropertyToNamespaceMapping>();
        private static readonly Version version6 = new Version(6,0,0);
        
        public static Version6PropertyToNamespaceMapping CreateOrGet(Type instanceType)
        {
            return instances.GetOrAdd(
                instanceType, 
                type =>
                {
                    var typeFullName = instanceType.FullName;
                    var propertyMappings =
                        GetPropertyMappings(instanceType,  TypeToV6Namespace);
                    return new Version6PropertyToNamespaceMapping(propertyMappings, typeFullName);
                });
        }

        protected Version6PropertyToNamespaceMapping(List<PropertyMapping> propertyMappings, string typeFullName)
            : base(propertyMappings, typeFullName)
        {
            
        }

        private static string TypeToV6Namespace(Type type)
        {
            return XmlNamespaceTools.GetXmlNamespace(type.FullName, version6);
        }
    }
}