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
using System.Runtime.InteropServices.ComTypes;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Extensions;
using ProtoBuf;

namespace Origam.DA.Service;

public class OrigamPath
{
    public string Relative { get; private set; }
    private readonly string BasePath;

    public string Absolute
    {
        get => Path.Combine(BasePath, Relative);
        protected set
        {
            string basePath = BasePath;
            if (!value.StartsWith(basePath))
            {
                throw new ArgumentException($"OrigamFile is not on base path:{value}");
            }
            Relative = AbsoluteToRelative(value);
        }
    }

    private OrigamPath() { }

    public bool RelativeEquals(string otherRelativePath)
    {
        return string.Equals(
            Path.Combine(BasePath, otherRelativePath),
            Path.Combine(BasePath, Relative),
            StringComparison.CurrentCultureIgnoreCase
        );
    }

    public DirectoryInfo Directory => new DirectoryInfo(Path.GetDirectoryName(Absolute));
    public bool Exists => File.Exists(Absolute);
    public string FileName => Path.GetFileName(Absolute);

    protected OrigamPath(string basePath)
    {
        BasePath = basePath;
    }

    internal OrigamPath(string absolutePath, string basePath)
    {
        BasePath = basePath;
        Absolute = absolutePath;
    }

    public string AbsoluteToRelative(string absolutePath)
    {
        string basePath = BasePath;
        if (basePath.EndsWith("\\"))
        {
            return absolutePath.Substring(basePath.Length);
        }
        else
        {
            return absolutePath.Substring(basePath.Length + 1);
        }
    }

    public bool EqualsTo(FileInfo file)
    {
        return Absolute.ToLower() == file.FullName.ToLower();
    }

    protected bool Equals(OrigamPath other) => string.Equals(Relative, other.Relative);

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((OrigamPath)obj);
    }

    public override int GetHashCode() => Relative != null ? Relative.GetHashCode() : 0;

    public OrigamPath UpdateToNew(DirectoryInfo dirToRename, string newDirPath)
    {
        string newAbsolutePath = Absolute.Replace(dirToRename.FullName, newDirPath);
        if (!newAbsolutePath.ToLower().Contains(BasePath.ToLower()))
        {
            throw new Exception(
                "cannot create an OrigamPath outside of basePath: " + newAbsolutePath
            );
        }
        return new OrigamPath(
            absolutePath: Absolute.Replace(dirToRename.FullName, newDirPath),
            basePath: BasePath
        );
    }

    public override string ToString()
    {
        return Absolute;
    }
}

public class ExternalFilePath : OrigamPath
{
    private const string ExternalFileLinkPrefix = "-***-ExternalFile:";
    private const string Delimiter = "___";
    private readonly ExternalFileExtension extension;
    private readonly string fieldName;
    public ExternalFileExtension Extension => extension;
    public string FieldName => fieldName;
    public Guid OwnerOjectId { get; }
    public string LinkWithPrefix { get; }

    public static bool IsExternalFileLink(string mayBePath)
    {
        if (string.IsNullOrEmpty(mayBePath))
            return false;
        if (!mayBePath.StartsWith(ExternalFileLinkPrefix))
            return false;
        if (
            !ParseOwnerId(mayBePath).HasValue
            || !ExternalFileExtensionTools.TryParse(mayBePath, out var _)
            || !TryParseFieldName(mayBePath, out var _)
        )
        {
            throw new Exception(
                "Input starts with "
                    + nameof(ExternalFileLinkPrefix)
                    + " but owner id, filed name or file extension cannot be parsed from it."
            );
        }
        return true;
    }

    public static Guid? ParseOwnerId(string externalFilePath)
    {
        string[] splitPath = externalFilePath.Split(Delimiter);
        string guidWithFileExtension = splitPath[splitPath.Length - 1];
        string guidStr = Path.GetFileNameWithoutExtension(guidWithFileExtension);
        bool parseSuccess = Guid.TryParse(guidStr, out var guid);
        return !parseSuccess ? (Guid?)null : guid;
    }

    public ExternalFilePath(
        OrigamPath ownerOrigamFilePath,
        string fieldName,
        Guid ownerOjectId,
        ExternalFileExtension extension,
        string basePath
    )
        : base(basePath)
    {
        if (fieldName.Contains(Delimiter))
        {
            throw new ArgumentException($"fieldName cannot contain \"{Delimiter}\"");
        }

        OwnerOjectId = ownerOjectId;
        this.fieldName = fieldName;
        this.extension = extension;
        string ext = extension.ToString().ToLower();
        Absolute =
            ownerOrigamFilePath.Absolute
            + Delimiter
            + fieldName
            + Delimiter
            + ownerOjectId
            + "."
            + ext;

        LinkWithPrefix = MakeExternalLinkWithPrefix(FileName);
    }

    public ExternalFilePath(
        OrigamPath ownerOrigamFilePath,
        string externalLinkWithPrefix,
        string basePath
    )
        : base(basePath)
    {
        LinkWithPrefix = externalLinkWithPrefix;
        string externalFileName = LinkWithPrefix.Substring(ExternalFileLinkPrefix.Length);
        if (!ExternalFileExtensionTools.TryParse(externalFileName, out extension))
        {
            throw new Exception($"Could not parse file extension from {externalFileName}");
        }
        if (!TryParseFieldName(externalFileName, out fieldName))
        {
            throw new Exception($"Could not parse filed name from {externalFileName}");
        }
        Absolute = Path.Combine(ownerOrigamFilePath.Directory.FullName, externalFileName);
        Guid? ownerOjectId = ParseOwnerId(externalFileName);
        if (!ownerOjectId.HasValue)
        {
            throw new Exception($"Could not get Id from file:{Absolute}");
        }
        OwnerOjectId = ownerOjectId.Value;
    }

    private static bool TryParseFieldName(string externalFileName, out string filedName)
    {
        string[] splitName = externalFileName.Split(Delimiter);
        if (splitName.Length < 2)
        {
            filedName = null;
            return false;
        }
        filedName = splitName[splitName.Length - 2];
        return true;
    }

    private string MakeExternalLinkWithPrefix(string extFileName) =>
        ExternalFileLinkPrefix + extFileName;

    public override string ToString() => Absolute;
}

public class OrigamPathFactory
{
    private readonly string BasePath;

    public OrigamPathFactory(DirectoryInfo toDirectory)
    {
        BasePath = toDirectory.FullName;
    }

    public OrigamPath CreateFromRelative(string relativePath)
    {
        string newAbsPath = Path.Combine(BasePath, relativePath);
        return new OrigamPath(newAbsPath, BasePath);
    }

    public OrigamPath Create(string absolutePath)
    {
        return new OrigamPath(absolutePath, BasePath);
    }

    internal ExternalFilePath Create(
        OrigamPath ownerOrigamFilePath,
        string fieldName,
        Guid ownerOjectId,
        ExternalFileExtension extension
    )
    {
        return new ExternalFilePath(
            ownerOrigamFilePath,
            fieldName,
            ownerOjectId,
            extension,
            BasePath
        );
    }

    internal ExternalFilePath Create(OrigamPath ownerOrigamFilePath, string externalLinkWithPrefix)
    {
        return new ExternalFilePath(ownerOrigamFilePath, externalLinkWithPrefix, BasePath);
    }

    public OrigamPath Create(FileInfo file)
    {
        return new OrigamPath(file.FullName, BasePath);
    }
}
