using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<string, Versions> instances 
            = new ConcurrentDictionary<string, Versions>();
        public static Version Last { get; } = new Version(Int32.MaxValue, Int32.MaxValue, Int32.MaxValue);
 
        public static Versions GetCurrentClassVersions(string typeName,
            Versions persistedClassVersions)
        {
            return
                instances.GetOrAdd(typeName, name => {
                    if (typeName == "model-persistence") // nodes in .origamGroupReference file
                    {
                        return new Versions();
                    }

                    Type type = Reflector.GetTypeByName(typeName);
                    if (type == null)
                    {
                        return  new Versions {[typeName] = Last}; 
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
                        .ForEach(pair => versions[pair.Key] = Last);
                
                    return versions;
                });
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
        
        public Versions(IEnumerable<OrigamNameSpace> origamNameSpaces)
        {   
            foreach (var origamNameSpace in origamNameSpaces)
            {
                this[origamNameSpace.FullTypeName] = origamNameSpace.Version;
            }
        }
        
    }
}