using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.DA.Service
{
    class PropertyToNamespaceMapping
    {
        private readonly List<PropertyMapping> propertyMappings;

        public PropertyToNamespaceMapping(IFilePersistent instance)
        {
            propertyMappings = instance
                .GetType()
                .GetAllBaseTypes()
                .Where(baseType => baseType.GetInterfaces().Contains(typeof(IFilePersistent)))
                .Concat( new []{instance.GetType()})
                .Select(type => new PropertyMapping(
                    propertyNames: GetXmlPropertyNames(type),
                    xmlNamespace: MakeXmlNameSpace(type),
                    xmlNamespaceName: MakeNamespaceName(type)))
                .ToList();

            var mappingForTheInstanceType = propertyMappings.Last();
            NodeNamespaceName = mappingForTheInstanceType.XmlNamespaceName;
            NodeNamespace = mappingForTheInstanceType.XmlNamespace;
        }

        public string NodeNamespaceName { get; }
        public string NodeNamespace { get; }

        private List<string> GetXmlPropertyNames(Type type)
        {
            return type.GetFields()
                .Cast<MemberInfo>()
                .Concat(type.GetProperties())
                .Where(prop => prop.DeclaringType == type)
                .Where(prop =>
                    Attribute.IsDefined(prop, typeof(XmlAttributeAttribute)) ||
                    Attribute.IsDefined(prop, typeof(XmlExternalFileReference)) ||
                    Attribute.IsDefined(prop, typeof(XmlPackageReferenceAttribute)) ||
                    Attribute.IsDefined(prop, typeof(XmlReferenceAttribute)))
                .Select(prop => prop.Name)
                .ToList();
        }
    
        private static string MakeXmlNameSpace(Type type)
        {
            Version currentClassVersion = Versions.GetCurrentClassVersion(type);
            return $"http://schemas.origam.com/{type.FullName}/{currentClassVersion}";
        }

        private static string MakeNamespaceName(Type type)
        {
            var charArray = type.Name
                .Where(char.IsUpper)
                .Select(char.ToLower)
                .ToArray();
            string namespaceName = new string(charArray);
            return namespaceName;
        }
        
        private class PropertyMapping
        {
            public List<string> PropertyNames { get; }
            public string XmlNamespace { get; }
            public string XmlNamespaceName { get; set; }

            public PropertyMapping(List<string> propertyNames, string xmlNamespace, string xmlNamespaceName)
            {
                PropertyNames = propertyNames;
                XmlNamespace = xmlNamespace;
                XmlNamespaceName = xmlNamespaceName;
            }
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

        public string GetNamespace(string propertyName)
        {
            PropertyMapping propertyMapping = propertyMappings
                                                  .FirstOrDefault(mapping => mapping.PropertyNames.Contains(propertyName))
                                              ?? throw new Exception($"Could not find xmlNamespace for  \"{propertyName}\"");
            return propertyMapping.XmlNamespace;
        }
    }
}