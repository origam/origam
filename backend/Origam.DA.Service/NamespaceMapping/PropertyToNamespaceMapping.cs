using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using MoreLinq.Extensions;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema;
#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

namespace Origam.DA.Service.NamespaceMapping;
public class PropertyToNamespaceMapping : IPropertyToNamespaceMapping
{
    private readonly List<PropertyMapping> propertyMappings;
    private readonly string typeFullName;
    private static readonly Dictionary<Type, PropertyToNamespaceMapping> instances
        = new Dictionary<Type, PropertyToNamespaceMapping>();
    public string NodeNamespaceName { get; }
    public XNamespace NodeNamespace { get; }
    
    public static void Init()
    {
        if (instances.Count > 0)
        {
            return;
        }
        var allTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(x=>x.GetReferencedAssemblies())
            .Where(x=>x.Name.Contains("Origam.Schema"))
            .DistinctBy(x=>x.FullName)
            .Select(Assembly.Load)
            .SelectMany(GetTypes);
        AddMapping(typeof(Package));
        
        allTypes
            .Where(type => 
                typeof(IFilePersistent).IsAssignableFrom(type) && 
                type != typeof(IFilePersistent))
            .ForEach(AddMapping);
    }
    private static IEnumerable<Type> GetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Exception exSub in ex.LoaderExceptions)
            {
                stringBuilder.AppendLine(exSub.Message);
                if (exSub is FileNotFoundException exFileNotFound)
                {
                    if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                    {
                        stringBuilder.AppendLine("Fusion Log:");
                        stringBuilder.AppendLine(exFileNotFound.FusionLog);
                    }
                }
                stringBuilder.AppendLine();
            }
            string errorMessage = stringBuilder.ToString();
            throw new Exception(errorMessage, ex);
        }
    }
    public static void AddMapping(Type type)
    {
        if (instances.ContainsKey(type))
        {
            return;
        }
        var typeFullName = type.FullName;
        var propertyMappings =
            GetPropertyMappings(type, XmlNamespaceTools.GetXmlNameSpace);
        instances[type] =
            new PropertyToNamespaceMapping(propertyMappings, typeFullName);
    }
    public static PropertyToNamespaceMapping Get(Type instanceType)
    {
        return instances[instanceType];
    }
    private static List<PropertyMapping> GetPropertyMappings(Type instanceType, Func<Type, OrigamNameSpace> xmlNamespaceGetter)
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
    
    protected static IEnumerable<PropertyName> GetXmlPropertyNames(Type type)
    {
        return type.GetFields()
            .Cast<MemberInfo>()
            .Concat(type.GetProperties())
            .Where(prop => prop.DeclaringType == type)
            .Select(ToPropertyName)
            .Where(x => x != null);
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
        NodeNamespace = mappingForTheInstanceType.XmlNamespace.StringValue;
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
                nameSpaceShortCut: propertyMapping.XmlNamespaceName, 
                nameSpace: propertyMapping.XmlNamespace);
        }
    }
    public OrigamNameSpace GetNamespaceByPropertyName(string propertyName)
    {
        PropertyMapping propertyMapping = propertyMappings
            .FirstOrDefault(mapping => mapping.ContainsPropertyNamed(propertyName))
              ?? throw new Exception(string.Format(
                                      Strings.CouldNotFindXmlNamespace,
                                      propertyName,
                                      typeFullName));
        return propertyMapping.XmlNamespace;
    }      
    
    public XNamespace GetNamespaceByXmlAttributeName(string xmlAttributeName)
    {
        PropertyMapping propertyMapping = propertyMappings
            .FirstOrDefault(mapping => mapping.ContainsXmlAttributeNamed(xmlAttributeName))
              ?? throw new Exception(string.Format(
                                      Strings.CouldNotFindXmlNamespace,
                                      xmlAttributeName,
                                      typeFullName));
        return propertyMapping.XmlNamespace.StringValue;
    }
    protected class PropertyName
    {
        public string Name { get; set; }
        public string XmlAttributeName { get; set; }
    }
    protected class PropertyMapping
    {
        public List<PropertyName> PropertyNames { get; }
        public OrigamNameSpace XmlNamespace { get; }
        public string XmlNamespaceName { get; set; }
        public PropertyMapping(IEnumerable<PropertyName> propertyNames, OrigamNameSpace xmlNamespace, string xmlNamespaceName)
        {
            PropertyNames = propertyNames.ToList();
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
                    });
            return new PropertyMapping(propertyNames, XmlNamespace, XmlNamespaceName);
        }
    }
}
