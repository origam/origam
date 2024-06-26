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
using System.IO;

namespace Origam.DA.ObjectPersistence;

[AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
public class XmlExternalFileReference: Attribute
{
    public ExternalFileExtension Extension { get; }
    public string ContainerName { get; }
    public XmlExternalFileReference( string containerName,
        ExternalFileExtension extension = ExternalFileExtension.Xml)
    {
        Extension = extension;
        ContainerName = containerName;
    }
}

/// <summary>
/// Edit IsSearchable if you want your newly defined ExternalFileExtension to be
/// marked as searchable.
/// </summary>
public enum ExternalFileExtension
{
    Xml,
    Png,
    Xslt,
    Bin,
    Txt
}
public static class ExternalFileExtensionTools
{
    public static bool IsSearchable(this ExternalFileExtension extension)
    {
        switch (extension)
        {
            case ExternalFileExtension.Xml:
                return true;
            case ExternalFileExtension.Png:
                return false;
            case ExternalFileExtension.Xslt:
                return true;
            case ExternalFileExtension.Bin:
                return false;
            case ExternalFileExtension.Txt:
                return true;
            default:
                return false;
        }
    }
    public static bool TryParse(FileInfo fileinfo, out ExternalFileExtension value) => 
        TryParsePrivate(fileinfo.Extension, out value);
    public static bool TryParse(string filePath, out ExternalFileExtension value)
    {
        string extension = Path.GetExtension(filePath);
        return TryParsePrivate(extension, out value);
    }
    private static bool TryParsePrivate(string extension, out ExternalFileExtension ext)
    {
        if (extension == "")
        {
            ext = ExternalFileExtension.Bin;
            return false;
        }
        string extensionWithoutDot = extension.Substring(1);
        bool parseSucess = Enum.TryParse(
            value: extensionWithoutDot,
            ignoreCase: true, 
            result: out ExternalFileExtension parsedExt);
        ext = parsedExt;
        return parseSucess;
    }
}  
