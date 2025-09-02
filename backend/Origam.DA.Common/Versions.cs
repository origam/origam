using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Origam.Extensions;
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

namespace Origam.DA.Common;

public class Versions
{
    private readonly Dictionary<string, Version> versionDict = new Dictionary<string, Version>();
    private static readonly ConcurrentDictionary<string, Versions> instances =
        new ConcurrentDictionary<string, Versions>();
    public static Version Last { get; } =
        new Version(Int32.MaxValue, Int32.MaxValue, Int32.MaxValue);
    public IEnumerable<string> TypeNames => versionDict.Keys;
    public bool IsDead => versionDict.Count == 1 && versionDict.First().Value == Last;

    public static Versions GetCurrentClassVersions(string fullTypeName)
    {
        return instances.GetOrAdd(fullTypeName, MakeCurrentClassVersions);
    }

    private static Versions MakeCurrentClassVersions(string fullTypeName)
    {
        if (fullTypeName == "model-persistence") // nodes in .origamGroupReference file
        {
            return new Versions();
        }
        Type type = Reflector.GetTypeByName(fullTypeName);
        if (type == null)
        {
            return new Versions(fullTypeName, Last);
        }
        Version classVersion = GetCurrentClassVersion(type);
        Versions versions = new Versions(fullTypeName, classVersion);
        foreach (var baseType in type.GetAllBaseTypes())
        {
            if (
                baseType.GetCustomAttribute(typeof(ClassMetaVersionAttribute), false)
                is ClassMetaVersionAttribute versionAttribute
            )
            {
                versions.versionDict.Add(baseType.FullName, versionAttribute.Value);
            }
        }
        return versions;
    }

    public static Version TryGetCurrentClassVersion(string fullTypeName)
    {
        Type type = Reflector.GetTypeByName(fullTypeName);
        var attribute =
            type.GetCustomAttribute(typeof(ClassMetaVersionAttribute)) as ClassMetaVersionAttribute;
        return attribute?.Value;
    }

    public static Version GetCurrentClassVersion(Type type)
    {
        var attribute =
            type.GetCustomAttribute(typeof(ClassMetaVersionAttribute)) as ClassMetaVersionAttribute;
        if (attribute == null)
        {
            throw new Exception(
                $"Cannot get meta version of class {type.FullName} because it does not have {nameof(ClassMetaVersionAttribute)} on it"
            );
        }
        return attribute.Value;
    }

    private Versions() { }

    private Versions(string fullTypeName, Version version)
    {
        versionDict.Add(fullTypeName, version);
    }

    public Version this[string fullTypeName] => versionDict[fullTypeName];

    public Versions(IEnumerable<OrigamNameSpace> origamNameSpaces)
    {
        foreach (var origamNameSpace in origamNameSpaces)
        {
            versionDict[origamNameSpace.FullTypeName] = origamNameSpace.Version;
        }
    }

    public Versions(Versions other, IEnumerable<string> deadClasses)
    {
        versionDict.AddOrReplaceRange(other.versionDict);
        foreach (string deadClassName in deadClasses)
        {
            if (!versionDict.ContainsKey(deadClassName))
            {
                versionDict.Add(deadClassName, Last);
            }
        }
    }

    public bool Contains(string className) => versionDict.ContainsKey(className);
}
