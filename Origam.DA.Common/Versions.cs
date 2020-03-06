using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using MoreLinq;
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

        public static Versions GetCurrentClassVersions(string typeName,
            Versions persistedClassVersions)
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

            persistedClassVersions
                .Where(pair => !versions.ContainsKey(pair.Key))
                .ForEach(pair => versions[pair.Key] = EndOfLife);
            
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

        public Versions(IEnumerable<OrigamNameSpace> origamNameSpaces)
        {   
            foreach (var origamNameSpace in origamNameSpaces)
            {
                this[origamNameSpace.FullTypeName] = origamNameSpace.Version;
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