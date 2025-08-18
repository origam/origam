using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service.MetaModelUpgrade;
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

namespace Origam.DA.Service.NamespaceMapping;
class Version6PropertyToNamespaceMapping : PropertyToNamespaceMapping
{
    private static readonly ConcurrentDictionary<Type, Version6PropertyToNamespaceMapping> instances
        = new ConcurrentDictionary<Type, Version6PropertyToNamespaceMapping>();
    private static readonly Version version6 = new Version(6,0,0);
    
    public static Version6PropertyToNamespaceMapping CreateOrGet(
        Type instanceType, ScriptContainerLocator scriptLocator)
    {
        return instances.GetOrAdd(
            instanceType, 
            type =>
            {
                var typeFullName = instanceType.FullName;
                var propertyMappings =
                    GetPropertyMappings(instanceType, scriptLocator);
                return new Version6PropertyToNamespaceMapping(propertyMappings, typeFullName);
            });
    }
    protected Version6PropertyToNamespaceMapping(List<PropertyMapping> propertyMappings, string typeFullName)
        : base(propertyMappings, typeFullName)
    {
        
    }
    
    protected static List<PropertyMapping> GetPropertyMappings(
        Type instanceType, ScriptContainerLocator scriptLocator)
    {
        var propertyMappings = instanceType
            .GetAllBaseTypes()
            .Where(baseType =>
                baseType.GetInterfaces().Contains(typeof(IFilePersistent)))
            .Concat(new[] {instanceType})
            .Select(type => ToPropertyMappings(scriptLocator, type))
            .ToList();
        return propertyMappings;
    }
    private static PropertyMapping ToPropertyMappings(
        ScriptContainerLocator scriptLocator, Type type)
    {
        var oldPropertyNameDict = scriptLocator
                                   .TryFindByTypeName(type.FullName)
                                   ?.OldPropertyXmlNames
                                   ?? new string[0];
        var oldPropertyNames = oldPropertyNameDict
            .Select(oldName =>
                new PropertyName
                {
                    Name = "",
                    XmlAttributeName = oldName
                });
                
        return new PropertyMapping(
            propertyNames: GetXmlPropertyNames(type).Concat(oldPropertyNames),
            xmlNamespace: OrigamNameSpace.CreateOrGet(type, version6),
            xmlNamespaceName: XmlNamespaceTools.GetXmlNamespaceName(type));
    }
}
