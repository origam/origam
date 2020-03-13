using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.DA.Service.NamespaceMapping
{
    class PropertyToNamespaceMapping : IPropertyToNamespaceMapping
    {
        private readonly List<PropertyMapping> propertyMappings;
        private readonly string typeFullName;
        private static readonly ConcurrentDictionary<Type, PropertyToNamespaceMapping> instances
            = new ConcurrentDictionary<Type, PropertyToNamespaceMapping>();

        public string NodeNamespaceName { get; }
        public XNamespace NodeNamespace { get; }
        
        public static PropertyToNamespaceMapping CreateOrGet(Type instanceType)
        {
            return instances.GetOrAdd(instanceType, type =>
            {
                var typeFullName = instanceType.FullName;
                var propertyMappings =
                    GetPropertyMappings(instanceType,  XmlNamespaceTools.GetXmlNameSpace);
                return new PropertyToNamespaceMapping(propertyMappings, typeFullName);
            });
        }

        protected static List<PropertyMapping> GetPropertyMappings(Type instanceType, Func<Type, string> xmlNamespaceGetter)
        {
            var propertyMappings = instanceType
                .GetAllBaseTypes()
                .Where(baseType =>
                    baseType.GetInterfaces().Contains(typeof(IFilePersistent)))
                .Concat(new[] {instanceType})
                .Select(type => new PropertyMapping(
                    propertyNames: GetXmlPropertyNames(type),
                    xmlNamespace: xmlNamespaceGetter(type),
                    xmlNamespaceName: XmlNamespaceTools.GetXmlNamespaceName(type)))
                .ToList();
            return propertyMappings;
        }
        
        private static List<PropertyName> GetXmlPropertyNames(Type type)
        {
            return type.GetFields()
                .Cast<MemberInfo>()
                .Concat(type.GetProperties())
                .Where(prop => prop.DeclaringType == type)
                .Select(ToPropertyName)
                .Where(x => x != null)
                .ToList();
        }

        private static PropertyName ToPropertyName(MemberInfo prop)
        {
            string xmlAttributeName;
            if (Attribute.IsDefined(prop, typeof(XmlAttributeAttribute)))
            {
                var xmlAttribute = prop.GetCustomAttribute(typeof(XmlAttributeAttribute)) as XmlAttributeAttribute;
                xmlAttributeName = xmlAttribute.AttributeName;
            }
            else if (Attribute.IsDefined(prop, typeof(XmlExternalFileReference)))
            {
                var xmlAttribute = prop.GetCustomAttribute(typeof(XmlExternalFileReference)) as XmlExternalFileReference;
                xmlAttributeName = xmlAttribute.ContainerName;
            }
            else if (Attribute.IsDefined(prop, typeof(XmlPackageReferenceAttribute)) || Attribute.IsDefined(prop, typeof(XmlReferenceAttribute)))
            {
                var xmlAttribute = prop.GetCustomAttribute(typeof(XmlReferenceAttribute)) as XmlReferenceAttribute;
                xmlAttributeName = xmlAttribute.AttributeName;
            }
            else
            {
                return null;
            }

            return new PropertyName {Name = prop.Name, XmlAttributeName = xmlAttributeName};
        }

        protected PropertyToNamespaceMapping(List<PropertyMapping> propertyMappings, string typeFullName)
        {
            this.propertyMappings = propertyMappings;
            this.typeFullName = typeFullName;
            
            var mappingForTheInstanceType = propertyMappings.Last();
            NodeNamespaceName = mappingForTheInstanceType.XmlNamespaceName;
            NodeNamespace = mappingForTheInstanceType.XmlNamespace;
        }

        public PropertyToNamespaceMapping DeepCopy()
        {
            var mappings = propertyMappings
                .Select(x => x.DeepCopy())
                .ToList();
            return new PropertyToNamespaceMapping(mappings, typeFullName);
        }
        
        public void AddNamespacesToDocumentAndAdjustMappings(OrigamXmlDocument xmlDocument)
        {
            foreach (var propertyMapping in propertyMappings)
            {
                propertyMapping.XmlNamespaceName = xmlDocument.AddNamespace(
                    nameSpaceName: propertyMapping.XmlNamespaceName, 
                    nameSpace: propertyMapping.XmlNamespace);
            }
        }       
        public void AddNamespacesToDocumentAndAdjustMappings(OrigamXDocument document)
        {
            foreach (var propertyMapping in propertyMappings)
            {
                propertyMapping.XmlNamespaceName = document.AddNamespace(
                    nameSpaceName: propertyMapping.XmlNamespaceName, 
                    nameSpace: propertyMapping.XmlNamespace);
            }
        }

        public string GetNamespaceByPropertyName(string propertyName)
        {
            PropertyMapping propertyMapping = propertyMappings
                                                  .FirstOrDefault(mapping => mapping.ContainsPropertyNamed(propertyName))
                                              ?? throw new Exception($"Could not find xmlNamespace for  \"{propertyName}\" in {typeFullName} and its base types");
            return propertyMapping.XmlNamespace;
        }      
        
        public XNamespace GetNamespaceByXmlAttributeName(string xmlAttributeName)
        {
            PropertyMapping propertyMapping = propertyMappings
                                                  .FirstOrDefault(mapping => mapping.ContainsXmlAttributeNamed(xmlAttributeName))
                                              ?? throw new Exception($"Could not find xmlNamespace for  \"{xmlAttributeName}\" in {typeFullName} and its base types");
            return propertyMapping.XmlNamespace;
        }

        protected class PropertyName
        {
            public string Name { get; set; }
            public string XmlAttributeName { get; set; }
        }

        protected class PropertyMapping
        {
            public List<PropertyName> PropertyNames { get; }
            public string XmlNamespace { get; }
            public string XmlNamespaceName { get; set; }

            public PropertyMapping(List<PropertyName> propertyNames, string xmlNamespace, string xmlNamespaceName)
            {
                PropertyNames = propertyNames;
                XmlNamespace = xmlNamespace;
                XmlNamespaceName = xmlNamespaceName;
            }

            public bool ContainsPropertyNamed(string propertyName)
            {
                return PropertyNames.Any(propName => propName.Name == propertyName);
            }

            public bool ContainsXmlAttributeNamed(string xmlAttributeName)
            {
                return PropertyNames.Any(propName => propName.XmlAttributeName == xmlAttributeName);
            }

            public PropertyMapping DeepCopy()
            {
                var propertyNames = PropertyNames
                    .Select(x =>
                        new PropertyName
                        {
                            Name = x.Name,
                            XmlAttributeName = x.XmlAttributeName
                        })
                    .ToList();
                return new PropertyMapping(propertyNames, XmlNamespace, XmlNamespaceName);
            }
        }
    }
}