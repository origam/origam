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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.DA.Service;

public class FlatFileSearcher
{
    private readonly string keyWord;

    public FlatFileSearcher(string keyWord)
    {
        this.keyWord = keyWord;
    }

    public List<Guid> SearchIn(IEnumerable<DirectoryInfo> loadedPackageDirectories)
    {
        return GetFilesContainingKeyword(loadedPackageDirectories)
            .Select(GetFileSearcher)
            .SelectMany(searcher => searcher.FindObjectsContainingKeyWord())
            .ToList();
    }

    private IFileSearcher GetFileSearcher(FileInfo fileInfo)
    {
        if (IsExternalSearchableFile(fileInfo))
        {
            return new PlainTextFileSearcher(keyWord, fileInfo);
        }
        if (OrigamFile.IsPersistenceFile(fileInfo))
        {
            return new OrigamFileSearcher(keyWord, fileInfo);
        }
        throw new NotImplementedException(
            "File cannot be searched because no suitable method is implemented: " + fileInfo
        );
    }

    private List<FileInfo> GetFilesContainingKeyword(
        IEnumerable<DirectoryInfo> loadedPackageDirectories
    )
    {
        return loadedPackageDirectories
            .AsParallel()
            .SelectMany(packageDir => packageDir.GetAllFilesInSubDirectories())
            .Where(IsSearchableFile)
            .Where(FileContainsKeyword)
            .ToList();
    }

    private bool IsSearchableFile(FileInfo file) =>
        OrigamFile.IsPersistenceFile(file) || IsExternalSearchableFile(file);

    private static bool IsExternalSearchableFile(FileInfo fileInfo) =>
        ExternalFileExtensionTools.TryParse(fileInfo, out var extension)
        && extension.IsSearchable();

    private bool FileContainsKeyword(FileInfo file)
    {
        string asteriskFreeKeyWord = keyWord.Replace("*", "");
        return File.ReadAllText(file.FullName)
                .IndexOf(asteriskFreeKeyWord, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

internal interface IFileSearcher
{
    IEnumerable<Guid> FindObjectsContainingKeyWord();
}

internal abstract class FileSearcher : IFileSearcher
{
    public abstract IEnumerable<Guid> FindObjectsContainingKeyWord();
    protected readonly string regExString;
    protected readonly FileInfo FileInfo;

    protected FileSearcher(string keyWord, FileInfo fileInfo)
    {
        regExString = BuildRegExString(keyWord);
        this.FileInfo = fileInfo;
    }

    private string BuildRegExString(string keyWord)
    {
        string regEx = keyWord;
        if (!keyWord.StartsWith("*"))
        {
            regEx = @"\b" + regEx;
        }
        if (!keyWord.EndsWith("*"))
        {
            regEx = regEx + @"\b";
        }
        return regEx.Replace("*", "");
    }
}

internal class OrigamFileSearcher : FileSearcher
{
    public OrigamFileSearcher(string keyWord, FileInfo fileInfo)
        : base(keyWord, fileInfo) { }

    public override IEnumerable<Guid> FindObjectsContainingKeyWord()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(FileInfo.FullName);
        XmlNodeList allNodes = xmlDoc.SelectNodes("//*");
        return allNodes
            .Cast<XmlNode>()
            .Where(HasIdAttribute)
            .Where(HasKeyWordInAttributes)
            .Select(node => GetIdItemOrThrow(node, FileInfo))
            .Select(idItem => idItem.Value)
            .Select(guidStr => new Guid(guidStr))
            .ToList();
    }

    private bool HasKeyWordInAttributes(XmlNode node)
    {
        return node.Attributes.Cast<XmlAttribute>().Any(attr => ContainsAMatch(attr.Value));
    }

    private bool HasIdAttribute(XmlNode node)
    {
        return node.Attributes.Cast<XmlAttribute>().Any(attr => attr.Name == "x:id");
    }

    private bool ContainsAMatch(string text) =>
        Regex.Match(text, regExString, RegexOptions.IgnoreCase).Success;

    private XmlNode GetIdItemOrThrow(XmlNode node, FileInfo pathToXml)
    {
        return node.Attributes.GetNamedItem("x:id")
            ?? throw new ArgumentNullException(
                $"{pathToXml} is malformed. Node {node.Name} node id"
            );
    }
}

internal class PlainTextFileSearcher : FileSearcher
{
    public PlainTextFileSearcher(string keyWord, FileInfo fileInfo)
        : base(keyWord, fileInfo) { }

    public override IEnumerable<Guid> FindObjectsContainingKeyWord()
    {
        if (!FileContainsKeyword())
        {
            yield break;
        }

        Guid? id = ExternalFilePath.ParseOwnerId(FileInfo.FullName);
        if (!id.HasValue)
        {
            yield break;
        }

        yield return id.Value;
    }

    private bool FileContainsKeyword()
    {
        string text = File.ReadAllText(FileInfo.FullName);
        return Regex.Match(text, regExString, RegexOptions.IgnoreCase).Success;
    }
}
