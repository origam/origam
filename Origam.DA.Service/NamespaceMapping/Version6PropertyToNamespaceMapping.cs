using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Extensions;

namespace Origam.DA.Service.NamespaceMapping
{
    class Version6PropertyToNamespaceMapping : PropertyToNamespaceMapping
    {
        private static readonly ConcurrentDictionary<Type, Version6PropertyToNamespaceMapping> instances
            = new ConcurrentDictionary<Type, Version6PropertyToNamespaceMapping>();
        private static readonly Version version6 = new Version(6,0,0);
        
        public static Version6PropertyToNamespaceMapping CreateOrGet(
            Type instanceType, ScriptContainerLocator scriptLocator)
        {
            return instances.GetOrAdd(
                instanceType, 
                type =>
                {
                    var typeFullName = instanceType.FullName;
                    var propertyMappings =
                        GetPropertyMappings(instanceType, scriptLocator);
                    return new Version6PropertyToNamespaceMapping(propertyMappings, typeFullName);
                });
        }

        protected Version6PropertyToNamespaceMapping(List<PropertyMapping> propertyMappings, string typeFullName)
            : base(propertyMappings, typeFullName)
        {
            
        }
        
        protected static List<PropertyMapping> GetPropertyMappings(
            Type instanceType, ScriptContainerLocator scriptLocator)
        {
            var propertyMappings = instanceType
                .GetAllBaseTypes()
                .Where(baseType =>
                    baseType.GetInterfaces().Contains(typeof(IFilePersistent)))
                .Concat(new[] {instanceType})
                .Select(type => ToPropertyMappings(scriptLocator, type))
                .ToList();
            return propertyMappings;
        }

        private static PropertyMapping ToPropertyMappings(
            ScriptContainerLocator scriptLocator, Type type)
        {
            var oldPropertyNameDict = scriptLocator
                                       .TryFindByTypeName(type.FullName)
                                       ?.OldPropertyXmlNames
                                       ?? new Dictionary<string, string[]>();
            var oldPropertyNames = oldPropertyNameDict
                .SelectMany(pair => 
                    pair.Value.Select(oldName =>
                        new PropertyName
                        {
                            Name = pair.Key,
                            XmlAttributeName = oldName
                        })
                    );

            return new PropertyMapping(
                propertyNames: GetXmlPropertyNames(type).Concat(oldPropertyNames),
                xmlNamespace: TypeToV6Namespace(type),
                xmlNamespaceName: XmlNamespaceTools.GetXmlNamespaceName(
                    type));
        }

        private static string TypeToV6Namespace(Type type)
        {
            return XmlNamespaceTools.GetXmlNamespace(type.FullName, version6);
        }
    }
}