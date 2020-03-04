using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.Common.ObjectPersistence.Attributes;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.DA.Service
{
    class PropertyToNamespaceMapping
    {
        private readonly List<PropertyMapping> propertyMappings;
        private readonly Func<Type, string> XmlNamespaceMapper;
        public PropertyToNamespaceMapping(Type instanceIype, Func<Type, string> xmlNamespaceMapper = null)
        {
            XmlNamespaceMapper = xmlNamespaceMapper ?? XmlNamespaceTools.GetXmlNameSpace;
            propertyMappings = instanceIype
                .GetAllBaseTypes()
                .Where(baseType => baseType.GetInterfaces().Contains(typeof(IFilePersistent)))
                .Concat( new []{instanceIype})
                .Select(type => new PropertyMapping(
                    propertyNames: GetXmlPropertyNames(type),
                    xmlNamespace: XmlNamespaceMapper(type),
                    xmlNamespaceName: XmlNamespaceTools.GetXmlNamespaceName(type)))
                .ToList();

            var mappingForTheInstanceType = propertyMappings.Last();
            NodeNamespaceName = mappingForTheInstanceType.XmlNamespaceName;
            NodeNamespace = mappingForTheInstanceType.XmlNamespace;
        }

        public string NodeNamespaceName { get; }
        public string NodeNamespace { get; }

        private List<PropertyName> GetXmlPropertyNames(Type type)
        {
            return type.GetFields()
                .Cast<MemberInfo>()
                .Concat(type.GetProperties())
                .Where(prop => prop.DeclaringType == type)
                .Select(ToPropertyName)
                .Where(x => x != null)
                .ToList();
        }

        private PropertyName ToPropertyName(MemberInfo prop)
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
        
        public void AddNamespacesToDocument(OrigamXmlDocument xmlDocument)
        {
            foreach (var propertyMapping in propertyMappings)
            {
                propertyMapping.XmlNamespaceName = xmlDocument.AddNamespace(
                    nameSpaceName: propertyMapping.XmlNamespaceName, 
                    nameSpace: propertyMapping.XmlNamespace);
            }
        }

        public string GetNamespaceByPropertyName(string propertyName)
        {
            PropertyMapping propertyMapping = propertyMappings
                                                  .FirstOrDefault(mapping => mapping.ContainsPropertyNamed(propertyName))
                                              ?? throw new Exception($"Could not find xmlNamespace for  \"{propertyName}\"");
            return propertyMapping.XmlNamespace;
        }      
        
        public string GetNamespaceByXmlAttributeName(string xmlAttributeName)
        {
            PropertyMapping propertyMapping = propertyMappings
                                                  .FirstOrDefault(mapping => mapping.ContainsXmlAttributeNamed(xmlAttributeName))
                                              ?? throw new Exception($"Could not find xmlNamespace for  \"{xmlAttributeName}\"");
            return propertyMapping.XmlNamespace;
        }
        
        private class PropertyName
        {
            public string Name { get; set; }
            public string XmlAttributeName { get; set; }
        }
        
        private class PropertyMapping
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
        }
    }

    static class XmlNamespaceTools
    {
        private static readonly Dictionary<Type, string> namespaces 
            =  new Dictionary<Type, string>();
        private static readonly Dictionary<Type, string> namespaceNames
            =  new Dictionary<Type, string>();
        public static string GetXmlNameSpace(Type type)
        {
            if (namespaces.ContainsKey(type)) return namespaces[type];
            
            Version currentClassVersion = Versions.GetCurrentClassVersion(type);
            string xmlNameSpace = $"http://schemas.origam.com/{type.FullName}/{currentClassVersion}";
            namespaces[type] = xmlNameSpace;
            return xmlNameSpace;
        }

        public static string GetXmlNamespaceName(Type type)
        {
            if (namespaceNames.ContainsKey(type)) return namespaceNames[type];
            if (type.GetCustomAttribute(typeof(XmlNamespaceNameAttribute)) 
                is XmlNamespaceNameAttribute attribute)
            {
                return attribute.XmlNamespaceName;
            }
            
            var charArray = type.Name
                .Where(char.IsUpper)
                .Select(char.ToLower)
                .ToArray();
            string xmlNamespaceName = new string(charArray);
            namespaceNames[type]= xmlNamespaceName;
            return xmlNamespaceName;
        }
    }
}