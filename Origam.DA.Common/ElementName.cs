using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Origam.Extensions;
using Origam.OrigamEngine;
using ProtoBuf;

namespace Origam.DA
{
    public static class ElementNameFactory
    {
        private static readonly Version currentModelVersion  = VersionProvider.CurrentModelMeta;
        private static readonly Version currentPersistenceVersion  = VersionProvider.CurrentPersistenceMeta;
        private static readonly Version currentPackageMeta = VersionProvider.CurrentPackageMeta;
        private static readonly ConcurrentDictionary<string, ElementName> instances = 
            new ConcurrentDictionary<string, ElementName>();

        public static ElementName Create(Type type)
        {
            XmlRootAttribute rootAttribute = FindRootAttribute(type);
            if (rootAttribute == null) return null;

            return CreateOrReturnCached(
                rootAttribute.Namespace,
                rootAttribute.ElementName);
        }

        private static ElementName CreateOrReturnCached(string xmlNamespace, string xmlElementName)
        {
            string strValue = ElementName.MakeName(xmlNamespace, xmlElementName);

            return instances.GetOrAdd(
                strValue, 
                value =>
                {
                    string[] splitElName = xmlNamespace.Split('/');
                    if (splitElName.Length != 5)
                    {
                        throw new ArgumentException(xmlNamespace + " cannot be parsed to element name");
                    }
                    if (!Version.TryParse(splitElName[3], out var version))
                    {
                        throw new ArgumentException(xmlNamespace + " cannot be parsed to element name because " + splitElName[3] + " cannot be parsed to version");
                    }
                    return new ElementName(
                        xmlNamespace: xmlNamespace,
                        xmlElementName: xmlElementName,
                        strValue: value,
                        version: version);
                });
        }

        public static ElementName Create(ElementName groupUri, string itemType)
        {     
            return CreateOrReturnCached(groupUri, itemType);
        }

        public static ElementName Create(XmlNode node)
        {                       
            return CreateOrReturnCached(
                xmlNamespace: node.NamespaceURI, 
                xmlElementName:node.LocalName);
        }

        public static ElementName Create(string elNameCandidate)
        {
            if(elNameCandidate == null) throw new NullReferenceException();
            if (!elNameCandidate.StartsWith("http://schemas.origam.com"))
            {
                throw new ArgumentException(nameof(ElementName)+" must start with http://schemas.origam.com");
            }
            if (!Uri.IsWellFormedUriString(elNameCandidate, UriKind.Absolute))
            {
                throw new ArgumentException(elNameCandidate +" is not a valid absolute Uri");
            }
            string[] splitElName = elNameCandidate.Split('/');
            if (splitElName.Length != 5)
            {
                throw new ArgumentException(elNameCandidate+" cannot be parsed to element name");
            }

            string xmlNamespace = splitElName   
                                      .Take(4)
                                      .Aggregate((name, x) => name+"/"+x)+"/";
            return CreateOrReturnCached(
                xmlNamespace: xmlNamespace,
                xmlElementName: splitElName[4]);
        }
        
        public static ElementName CreatePersistenceElName(string elName)
        {
            return CreateVersionCheckedElName(
                elName: elName,
                current: currentPersistenceVersion,
                namespaceName: "persistence");
        }
        
        public static ElementName CreateModelElName(string elName)
        {     
            return CreateVersionCheckedElName(
                elName: elName,
                current: currentModelVersion,
                namespaceName: "model");
        }
        
        public static ElementName CreatePackageElName(string elName)
        {
            return CreateVersionCheckedElName(
                elName: elName,
                current: currentPackageMeta,
                namespaceName: "package");
        }
        
        private static ElementName CreateVersionCheckedElName(string elName,
            Version current, string namespaceName)
        {
            ElementName elementName = Create(elName);
            
            if (elementName.Version != current &&
                !elementName.Version.DiffersOnlyInBuildFrom(current))
            { 
                throw new ArgumentException(
                    $"Cannot create {namespaceName} element name from: {elementName} because supplied meta model version is not compatible with current {namespaceName} meta model version: {current}");
            }
            return elementName;
        }

        private static XmlRootAttribute FindRootAttribute(Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof(XmlRootAttribute), true);

            if (attributes != null && attributes.Length > 0)
                return (XmlRootAttribute) attributes[0];
            else
                return null;
        }
    }
    

    [ProtoContract]
    public class ElementName
    {
        [ProtoMember(1)]
        public string XmlElementName { get; }
        [ProtoMember(2)]
        public string XmlNamespace { get; }

        private string strValue;
        private Version version;

        private string Value {
            get =>
                strValue ??(strValue = MakeName(XmlNamespace, XmlElementName));
            set => strValue = value;
        }

        public Version Version => 
            version ?? (version = ElementNameFactory.Create(XmlNamespace).Version);

        private ElementName()
        {
        }

        internal ElementName(string xmlNamespace, string xmlElementName, string strValue, Version version)
        {
            this.XmlElementName = xmlElementName;
            this.XmlNamespace = xmlNamespace;
            Value = strValue;
            this.version = version;
        }

        internal static string MakeName(string xmlNamespace, string xmlElementName) => 
            xmlNamespace + xmlElementName;

        public override string ToString() => Value;

        public static bool operator == (ElementName x, ElementName y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null)) return true;
            if (ReferenceEquals(x, null) && !ReferenceEquals(y, null)) return false;
            return x.Equals(y);
        }
        
        public static bool operator != (ElementName x, ElementName y) 
        {
            return !(x == y);
        }

        protected bool Equals(ElementName other)
        {
            if (ReferenceEquals(other, null)) return false;
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ElementName) obj);
        }

        public override int GetHashCode() => (Value != null ? Value.GetHashCode() : 0);
        
        public static implicit operator string(ElementName elementName) => 
           elementName.ToString();
    }
}