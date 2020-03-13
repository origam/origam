using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using MoreLinq;
using Origam.DA.Common;
using Origam.DA.Common.ObjectPersistence.Attributes;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.DA.Service
{
    internal interface IPropertyToNamespaceMapping
    {
        XNamespace NodeNamespace { get; }
        XNamespace GetNamespaceByXmlAttributeName(string xmlAttributeName);
        void AddNamespacesToDocument(OrigamXDocument document);
    }

    class DeadClassPropertyToNamespaceMapping : IPropertyToNamespaceMapping
    {
        private string xmlNamespaceName;

        public DeadClassPropertyToNamespaceMapping(string fullTypeName, Version version)
        {
            string typeName = fullTypeName.Split(".").Last();
            NodeNamespace =  XmlNamespaceTools.GetXmlNamespace(fullTypeName, version);
            xmlNamespaceName = XmlNamespaceTools.GetXmlNamespaceName(typeName);
        }

        public XNamespace NodeNamespace { get; }

        public XNamespace GetNamespaceByXmlAttributeName(string xmlAttributeName)
        {
            return NodeNamespace;
        }

        public void AddNamespacesToDocument(OrigamXDocument document)
        {
            xmlNamespaceName = document.AddNamespace(
                nameSpaceName: xmlNamespaceName, 
                nameSpace: NodeNamespace.ToString());
        }
    }
    
    class Version6PropertyToNamespaceMapping : PropertyToNamespaceMapping
    {
        private readonly Version version6 = new Version(6,0,0);

        public Version6PropertyToNamespaceMapping(Type instanceType) : base(instanceType)
        {
        }

        protected override string TypeToNamespace(Type type)
        {
            return XmlNamespaceTools.GetXmlNamespace(type.FullName, version6);
        }
    }

    class PropertyToNamespaceMapping : IPropertyToNamespaceMapping
    {
        private readonly List<PropertyMapping> propertyMappings;
        private readonly Func<Type, string> XmlNamespaceMapper;
        private readonly string typeFullName;

        public PropertyToNamespaceMapping(Type instanceType)
        {
            typeFullName = instanceType.FullName;
            propertyMappings = instanceType
                .GetAllBaseTypes()
                .Where(baseType => baseType.GetInterfaces().Contains(typeof(IFilePersistent)))
                .Concat( new []{instanceType})
                .Select(type => new PropertyMapping(
                    propertyNames: GetXmlPropertyNames(type),
                    xmlNamespace: TypeToNamespace(type),
                    xmlNamespaceName: XmlNamespaceTools.GetXmlNamespaceName(type)))
                .ToList();

            var mappingForTheInstanceType = propertyMappings.Last();
            NodeNamespaceName = mappingForTheInstanceType.XmlNamespaceName;
            NodeNamespace = mappingForTheInstanceType.XmlNamespace;
        }

        public string NodeNamespaceName { get; }
        public XNamespace NodeNamespace { get; }

        protected virtual string TypeToNamespace(Type type)
        {
           return XmlNamespaceTools.GetXmlNameSpace(type);
        }

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
        public void AddNamespacesToDocument(OrigamXDocument document)
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
        private static readonly ConcurrentDictionary<Type, string> namespaces 
            =  new ConcurrentDictionary<Type, string>();
        private static readonly ConcurrentDictionary<Type, string> namespaceNames
            =  new ConcurrentDictionary<Type, string>();
        public static string GetXmlNameSpace(Type type)
        {
            return
                namespaces.GetOrAdd(type, type1 =>
                {
                    Version currentClassVersion = Versions.GetCurrentClassVersion(type1);
                    return GetXmlNamespace(type1.FullName, currentClassVersion);
                });
        }

        public static string GetXmlNamespace(string fullTypeName, Version version)
        {
            return $"http://schemas.origam.com/{fullTypeName}/{version}";
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