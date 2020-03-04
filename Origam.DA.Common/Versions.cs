using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Origam.Extensions;

namespace Origam.DA.Common
{
    public class Versions: Dictionary<string, Version>
    {
        public static Version EndOfLife { get; } = new Version(Int32.MaxValue, Int32.MaxValue, Int32.MaxValue);
        public static Versions FromAttributeString(string xmlAttribute)
        {
            if (string.IsNullOrWhiteSpace(xmlAttribute))
            {
                return new Versions();
            }

            var versionsDict = xmlAttribute
                .Split(";")
                .Where(typeVersionPair => typeVersionPair.Trim() != "")
                .Select(typeVersionPair =>
                {
                    string[] typeAndVersion = typeVersionPair
                        .Split(" ")
                        .Select(x=> x.Trim())
                        .Where(x=>x != "")
                        .ToArray();
                    if (typeAndVersion.Length != 2)
                    {
                        throw new ArgumentException(
                            $"Cannot parse type and version from: \"{xmlAttribute}\"");
                    }
                    
                    Version version = new Version(typeAndVersion[1]);
                    return new Tuple<string, Version>(typeAndVersion[0], version);
                });
           return new Versions(versionsDict);
        }

        public static Versions GetCurrentClassVersions(string typeName)
        {
            Type type = Reflector.GetTypeByName(typeName);
            if (type == null)
            {
                return  new Versions {[typeName] = EndOfLife}; 
            }

            Version classVersion = GetCurrentClassVersion(type);

            Versions versions = new Versions {[typeName] = classVersion};

            foreach (var baseType in type.GetAllBaseTypes())
            {
                if (baseType.GetCustomAttribute(typeof(ClassMetaVersionAttribute)) 
                    is ClassMetaVersionAttribute versionAttribute)
                {
                    versions.Add(baseType.FullName, versionAttribute.Value);
                }
            }

            return versions;
        }

        public static Version GetCurrentClassVersion(Type type)
        {
            var attribute = type.GetCustomAttribute(typeof(ClassMetaVersionAttribute)) as
                    ClassMetaVersionAttribute;
            if (attribute == null)
            {
                throw new Exception(
                    $"Cannot get meta version of class {type.FullName} because it does not have {nameof(ClassMetaVersionAttribute)} on it");
            }

            return attribute.Value;
        }

        public static Versions GetPersistedClassVersion(XmlNode classNode, string type)
        {
            if (classNode == null)
            {
                throw new ArgumentNullException(nameof(classNode));
            }
            
            string versionsAttributeString = classNode.Attributes["versions"]?.Value;
            if (string.IsNullOrWhiteSpace(versionsAttributeString))
            {
                return new Versions(type ,new Version("1.0.0"));
            }

            return FromAttributeString(versionsAttributeString);
        }

        private Versions()
        {
        }

        private Versions(string type, Version version)
        {
            this[type] = version;
        }

        private Versions(IEnumerable<Tuple<string, Version>> classVersions)
        {
            foreach (var typeAndVersion in classVersions)
            {
                this[typeAndVersion.Item1] = typeAndVersion.Item2;
            }
        }

        public string ToAttributeString()
        {
            return string.Join(
                "; ", 
                this.Select(pair => $"{pair.Key} {pair.Value}"));
        }

    }
}