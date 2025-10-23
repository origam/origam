#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Linq;
using System.Xml;
using Origam.DA.Common;

namespace Origam.DA.Service;

public static class XmlUtils
{
    public static Guid? ReadId(XmlNode node)
    {
        return ReadGuid(node, OrigamFile.IdAttribute);
    }

    public static Guid? ReadParenId(XmlNode node)
    {
        return ReadGuid(node, OrigamFile.ParentIdAttribute);
    }

    public static Guid? ReadId(XmlReader xmlReader) =>
        ReadGuid(xmlReader, OrigamFile.IdAttribute, OrigamFile.ModelPersistenceUri);

    public static string ReadNewModelId(XmlFileData xmlFileData)
    {
        return xmlFileData
            ?.XmlDocument?.SelectSingleNode("//p:package", xmlFileData.NamespaceManager)
            ?.Attributes?[$"x:{OrigamFile.IdAttribute}"]?.Value;
    }

    public static string ReadId(XmlFileData xmlFileData)
    {
        return xmlFileData
            ?.XmlDocument?.SelectSingleNode("package", xmlFileData.NamespaceManager)
            ?.Attributes?[$"x:{OrigamFile.IdAttribute}"]?.Value;
    }

    private static Guid? ReadGuid(XmlReader xmlReader, string attrName, string attrNamespace)
    {
        string result = xmlReader.GetAttribute(attrName, attrNamespace);
        if (string.IsNullOrWhiteSpace(result))
        {
            return null;
        }

        return new Guid(result);
    }

    private static Guid? ReadGuid(XmlNode node, string attrName)
    {
        if (node?.Attributes == null)
        {
            return null;
        }

        XmlAttribute idAtt = node.Attributes[attrName, OrigamFile.ModelPersistenceUri];
        Guid? id = null;
        if (idAtt != null)
        {
            id = new Guid(idAtt.Value);
        }
        return id;
    }

    public static string ReadType(XmlReader xmlReader)
    {
        return xmlReader.NamespaceURI.Split('/')[3];
    }

    public static OrigamNameSpace[] ReadNamespaces(XmlReader xmlReader)
    {
        return Enumerable
            .Range(0, xmlReader.AttributeCount)
            .Select(xmlReader.GetAttribute)
            .Select(OrigamNameSpace.CreateOrGet)
            .ToArray();
    }
}
