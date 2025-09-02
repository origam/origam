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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Origam.DA.Common;
using Origam.DA.Common.ObjectPersistence.Attributes;

namespace Origam.DA.Service.NamespaceMapping
{
    public static class XmlNamespaceTools
    {
        private static readonly ConcurrentDictionary<Type, OrigamNameSpace> namespaces =
            new ConcurrentDictionary<Type, OrigamNameSpace>();
        private static readonly Dictionary<Type, string> namespaceNames =
            new Dictionary<Type, string>();

        public static OrigamNameSpace GetXmlNameSpace(Type type)
        {
            if (!namespaces.ContainsKey(type))
            {
                Version currentClassVersion = Versions.GetCurrentClassVersion(type);
                namespaces[type] = OrigamNameSpace.CreateOrGet(type, currentClassVersion);
            }
            return namespaces[type];
        }

        public static string GetXmlNamespaceName(Type type)
        {
            if (!namespaceNames.ContainsKey(type))
            {
                namespaceNames[type] = GetNamespaceName(type, namespaceNames);
            }
            return namespaceNames[type];
        }

        public static string GetXmlNamespaceName(string typeName)
        {
            var charArray = typeName.Where(char.IsUpper).Select(char.ToLower).ToArray();
            return new string(charArray);
        }

        private static string GetNamespaceName(Type type, Dictionary<Type, string> namespaceDict)
        {
            if (
                type.GetCustomAttribute(typeof(XmlNamespaceNameAttribute))
                is XmlNamespaceNameAttribute attribute
            )
            {
                return attribute.XmlNamespaceName;
            }

            string namespaceName = GetXmlNamespaceName(type.Name);

            Regex regex = new Regex($"{namespaceName}$|{namespaceName}(\\d+)");
            var orderNumbers = namespaceDict
                .Values.Select(name => regex.Match(name))
                .Where(match => match.Success)
                .Select(match =>
                {
                    string nameNumber = match.Groups[1].Value;
                    return nameNumber == "" ? 0 : int.Parse(nameNumber);
                })
                .OrderBy(number => number)
                .ToList();

            return orderNumbers.Count == 0
                ? namespaceName
                : $"{namespaceName}{orderNumbers.Last() + 1}";
        }
    }
}
