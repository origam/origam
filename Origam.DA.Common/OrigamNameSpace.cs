#region license

/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.Xml.Serialization;

namespace Origam.DA.Common
{
    public class OrigamNameSpace
    {
        public Version Version { get; }
        public string StringValue { get; }
        public string FullTypeName { get; }
        
        private static readonly ConcurrentDictionary<string, OrigamNameSpace> instances 
            =  new ConcurrentDictionary<string, OrigamNameSpace>();

        public static OrigamNameSpace Create(Type type)
        {
            XmlRootAttribute rootAttribute = FindRootAttribute(type);
            
            if (string.IsNullOrEmpty(rootAttribute.Namespace))
            {
                Version currentClassVersion = Versions.GetCurrentClassVersion(type);
                return CreateOrGet(type.FullName, currentClassVersion);
            }
            return CreateOrGet(rootAttribute.Namespace);
        }
        
        public static OrigamNameSpace CreateOrGet(string fullTypeName, Version version)
        {
            return CreateOrGet( $"http://schemas.origam.com/{fullTypeName}/{version}");
        }

        public static OrigamNameSpace CreateOrGet(string xmlNamespace)
        {
            return instances.GetOrAdd(xmlNamespace, CreateNonCached);
        }

        private static OrigamNameSpace CreateNonCached(string xmlNamespace)
        {
            if (xmlNamespace == null)
                throw new ArgumentNullException(nameof(xmlNamespace));

            if (!xmlNamespace.StartsWith("http://schemas.origam.com"))
            {
                throw new ArgumentException(
                    $" {nameof(OrigamNameSpace)} must start with http://schemas.origam.com");
            }

            if (!Uri.IsWellFormedUriString(xmlNamespace, UriKind.Absolute))
            {
                throw new ArgumentException(
                    $"{xmlNamespace} is not a valid absolute Uri");
            }

            string[] splitElName = xmlNamespace.Split('/');
            if (splitElName.Length < 5)
            {
                throw new ArgumentException(
                    $"{xmlNamespace} cannot be parsed to {nameof(OrigamNameSpace)}");
            }

            if (!Version.TryParse(splitElName[4], out var version))
            {
                throw new ArgumentException(
                    $"{xmlNamespace} cannot be parsed to {nameof(OrigamNameSpace)} because \"{splitElName[4]}\" cannot be parsed to version");
            }

            return new OrigamNameSpace(
                version: version,
                stringValue: xmlNamespace,
                fullTypeName: splitElName[3]);
        }

        private static XmlRootAttribute FindRootAttribute(Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof(XmlRootAttribute), true);
        
            if (attributes != null && attributes.Length > 0)
                return (XmlRootAttribute) attributes[0];
            else
                return null;
        }

        private OrigamNameSpace(Version version, string stringValue, string fullTypeName)
        {
            Version = version;
            StringValue = stringValue;
            FullTypeName = fullTypeName;
        }
    }
}