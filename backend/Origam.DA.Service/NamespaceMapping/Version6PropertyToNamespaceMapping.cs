using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Origam.DA.Common;
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
                                       ?? new string[0];
            var oldPropertyNames = oldPropertyNameDict
                .Select(oldName =>
                    new PropertyName
                    {
                        Name = "",
                        XmlAttributeName = oldName
                    });
                    
            return new PropertyMapping(
                propertyNames: GetXmlPropertyNames(type).Concat(oldPropertyNames),
                xmlNamespace: OrigamNameSpace.CreateOrGet(type, version6),
                xmlNamespaceName: XmlNamespaceTools.GetXmlNamespaceName(type));
        }
    }
}