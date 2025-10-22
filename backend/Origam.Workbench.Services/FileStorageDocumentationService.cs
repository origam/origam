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
using System.Xml;
using CSharpFunctionalExtensions;
using MoreLinq;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.OrigamEngine;
using Origam.Services;

namespace Origam.Workbench.Services;

public class FileStorageDocumentationService
    : AbstractDocumentationService,
        IFileStorageDocumentationService
{
    private readonly IFilePersistenceProvider filePersistenceProvider;
    private readonly FileEventQueue fileEventQueue;
    private readonly Dictionary<string, string> changedFileHashDictionary =
        new Dictionary<string, string>();
    private readonly Dictionary<string, DocXmlDocument> loadedDocFiles =
        new Dictionary<string, DocXmlDocument>();

    public FileStorageDocumentationService(
        IFilePersistenceProvider persistenceService,
        FileEventQueue fileEventQueue
    )
    {
        filePersistenceProvider = persistenceService ?? throw new ArgumentNullException();
        this.fileEventQueue = fileEventQueue ?? throw new ArgumentNullException();
        fileEventQueue.ReloadNeeded += (sender, args) => loadedDocFiles.Clear();
    }

    public override string GetDocumentation(Guid schemaItemId, DocumentationType docType)
    {
        if (schemaItemId == Guid.Empty)
        {
            return "";
        }

        DocXmlDocument docXmlDocument = GetDocumentFor(schemaItemId);
        IEnumerable<XmlNode> nodes = docXmlDocument.GetNodesFor(schemaItemId);
        string documentation = new DocumentationCompleteXmlDocument(
            nodes
        ).GetDocumentationByDocType(docType);
        return filePersistenceProvider.LocalizationCache.GetLocalizedString(
            schemaItemId,
            "Documentation " + docType,
            documentation
        );
    }

    public override DocumentationComplete LoadDocumentation(Guid schemaItemId)
    {
        DocXmlDocument docXmlDocument = GetDocumentFor(schemaItemId);
        IEnumerable<XmlNode> nodes = docXmlDocument.GetNodesFor(schemaItemId);
        return new DocumentationCompleteXmlDocument(nodes).ToDataSet();
    }

    public override void SaveDocumentation(
        DocumentationComplete documentationData,
        Guid schemaItemId
    )
    {
        if (IsEmpty(documentationData))
        {
            fileEventQueue.Pause();
            DocXmlDocument docXmlDocument = GetDocumentFor(schemaItemId);
            docXmlDocument.RemoveOutDatedNodes(new List<XmlNode>(), schemaItemId);
            docXmlDocument.Save();
            RemovedCachedDocumentFor(schemaItemId);
            fileEventQueue.Continue();
            return;
        }
        var dataSetXmlDocument = new DocumentationCompleteXmlDocument(documentationData);

        fileEventQueue.Pause();
        var modifiedDocuments = new List<DocXmlDocument>();
        foreach (Guid documentedItemId in dataSetXmlDocument.GetAllDocumentedItemIds())
        {
            if (!filePersistenceProvider.Has(documentedItemId))
            {
                continue;
            }

            DocXmlDocument docXmlDocument = GetDocumentFor(documentedItemId);
            List<XmlNode> docNodes = dataSetXmlDocument.GetNodesWith(documentedItemId);
            docXmlDocument.RemoveOutDatedNodes(docNodes, schemaItemId);
            docXmlDocument.AddOrReplace(docNodes);
            modifiedDocuments.Add(docXmlDocument);
        }
        foreach (var docXmlDocument in modifiedDocuments)
        {
            docXmlDocument.Save();
            UpdateFileHash(docXmlDocument.FilePath);
            loadedDocFiles.RemoveByValueSelector(doc => doc == docXmlDocument);
        }
        fileEventQueue.Continue();
    }

    public override void SaveDocumentation(DocumentationComplete documentationData)
    {
        SaveDocumentation(documentationData, Guid.Empty);
    }

    private void UpdateFileHash(string docFilePath)
    {
        string hash = new FileInfo(docFilePath).GetFileBase64Hash();
        changedFileHashDictionary.AddOrReplace(docFilePath, hash);
    }

    private static bool IsEmpty(DocumentationComplete documentationData) =>
        documentationData.Tables.Count == 0 || documentationData.Documentation.Rows.Count == 0;

    private DocXmlDocument GetDocumentFor(Guid itemId)
    {
        DirectoryInfo packageDirectory = filePersistenceProvider.GetParentPackageDirectory(itemId);
        string docFilePath = Path.Combine(
            packageDirectory.FullName,
            DocumentationXmlDocument.DocFilename
        );
        if (!loadedDocFiles.ContainsKey(docFilePath))
        {
            loadedDocFiles.Add(docFilePath, new DocXmlDocument(docFilePath));
        }
        return loadedDocFiles[docFilePath];
    }

    private void RemovedCachedDocumentFor(Guid itemId)
    {
        DirectoryInfo packageDirectory = filePersistenceProvider.GetParentPackageDirectory(itemId);
        string docFilePath = Path.Combine(
            packageDirectory.FullName,
            DocumentationXmlDocument.DocFilename
        );
        loadedDocFiles.Remove(docFilePath);
    }

    public override DocumentationComplete GetAllDocumentation()
    {
        IEnumerable<XmlNode> documentationNodes = filePersistenceProvider
            .TopDirectory.GetAllFilesInSubDirectories()
            .Where(file => file.Extension == DocumentationXmlDocument.DocFilename)
            .Select(file => new DocXmlDocument(file.FullName))
            .SelectMany(xmlDoc => xmlDoc.DocNodes);
        var documentationCompleteXmlDocument = new DocumentationCompleteXmlDocument(
            documentationNodes
        );
        DocumentationComplete documentationData = documentationCompleteXmlDocument.ToDataSet();
        return documentationData;
    }

    public Maybe<string> GetDocumentationFileHash(FileInfo filePath)
    {
        changedFileHashDictionary.TryGetValue(filePath.FullName, out var hash);
        return hash;
    }

    public override void InitializeService() { }

    public override void UnloadService() { }

    public override event EventHandler Initialize
    {
        add { }
        remove { }
    }
    public override event EventHandler Unload
    {
        add { }
        remove { }
    }
}

abstract class DocumentationXmlDocument : XmlDocument
{
    protected XmlNode FindChildByName(XmlNode node, string childName)
    {
        return node.ChildNodes.Cast<XmlNode>().FirstOrDefault(child => child.Name == childName)
            ?? throw ThrowOnCouldNotReadNode(node, childName);
    }

    protected abstract ArgumentException ThrowOnCouldNotReadNode(XmlNode node, string childName);
    internal const string DocFilename = ".origamDoc";
    internal const string DataNodeName = "Data";
    internal const string CategoryNodeName = "Category";
    internal const string RefItemIdNodeName = "refSchemaItemId";
    internal const string IdNodeName = "Id";
}

internal class DocumentationCompleteXmlDocument : DocumentationXmlDocument
{
    public DocumentationCompleteXmlDocument(IEnumerable<XmlNode> nodes)
    {
        XmlNode rootNode = CreateNode(XmlNodeType.Element, "DocumentationComplete", "");
        AppendChild(rootNode);
        foreach (XmlNode node in nodes)
        {
            XmlNode importedNode = ImportNode(node, true);
            FirstChild.AppendChild(importedNode);
        }
    }

    public DocumentationCompleteXmlDocument(DocumentationComplete dataSet)
    {
        string xml = dataSet.GetXml();
        LoadXml(xml);
    }

    public DocumentationComplete ToDataSet()
    {
        var stringReader = new StringReader(InnerXml);
        var documentationComplete = new DocumentationComplete();
        documentationComplete.ReadXml(stringReader);
        documentationComplete.ExtendedProperties["ModelVersion"] =
            VersionProvider.CurrentModelMetaVersion;

        return documentationComplete;
    }

    public List<Guid> GetAllDocumentedItemIds()
    {
        return FirstChild
            .ChildNodes.Cast<XmlNode>()
            .Select(GetRefItemId)
            .Distinct()
            .Where(id => id != Guid.Empty)
            .ToList();
    }

    public List<XmlNode> GetNodesWith(Guid refItemId)
    {
        return FirstChild
            .ChildNodes.Cast<XmlNode>()
            .Where(node => GetRefItemId(node) == refItemId)
            .ToList();
    }

    private Guid GetRefItemId(XmlNode node)
    {
        XmlNode refItemNode = FindChildByName(node, RefItemIdNodeName);
        return new Guid(refItemNode.InnerText);
    }

    public string GetDocumentationByDocType(DocumentationType docType)
    {
        XmlNode nodeWithTheRightCategory = FirstChild
            .ChildNodes.Cast<XmlNode>()
            .FirstOrDefault(node => CategoryMatches(node, docType));
        return nodeWithTheRightCategory != null
            ? FindChildByName(nodeWithTheRightCategory, DataNodeName).InnerText
            : null;
    }

    private bool CategoryMatches(XmlNode node, DocumentationType docType) =>
        FindChildByName(node, CategoryNodeName).InnerText == docType.ToString();

    protected override ArgumentException ThrowOnCouldNotReadNode(XmlNode node, string childName)
    {
        return new ArgumentException(
            $"Could not read DataSet node because node \"{childName}\" was not found in {node.InnerXml}"
        );
    }
}

internal class DocXmlDocument : DocumentationXmlDocument
{
    public string FilePath { get; }
    public IEnumerable<XmlNode> DocNodes => FirstChild.ChildNodes.Cast<XmlNode>();

    public DocXmlDocument(string filePath)
    {
        FilePath = filePath;
        Load();
        AddRootNodeIfEmpty();
        CheckNodes();
    }

    public void Save()
    {
        Save(FilePath);
    }

    public void RemoveOutDatedNodes(List<XmlNode> newNodes, Guid refItemId)
    {
        HashSet<Guid> newNodeIds = new HashSet<Guid>(newNodes.Select(GetId));
        GetNodesFor(refItemId)
            .Where(node => !newNodeIds.Contains(GetId(node)))
            .ForEach(node => FirstChild.RemoveChild(node));
    }

    public void AddOrReplace(List<XmlNode> newNodes)
    {
        foreach (XmlNode newNode in newNodes)
        {
            AddOrReplace(newNode);
        }
    }

    public IEnumerable<XmlNode> GetNodesFor(Guid schemaItemId)
    {
        return FirstChild
            .ChildNodes.Cast<XmlNode>()
            .Where(node => GetRefItemId(node) == schemaItemId)
            .ToList();
    }

    private void CheckNodes()
    {
        FirstChild.ChildNodes.Cast<XmlNode>().ForEach(ReadChildrenOrThrow);
    }

    private void ReadChildrenOrThrow(XmlNode node)
    {
        XmlNode dataNode = FindChildByName(node, DataNodeName);
        XmlNode categoryNode = FindChildByName(node, CategoryNodeName);
        if (!Enum.TryParse(categoryNode.InnerText, true, out DocumentationType _))
        {
            throw new ArgumentException(
                $"Could not read file: {FilePath} because documentation category \"{categoryNode.InnerText}\" was wrong in {node.InnerXml}"
            );
        }
        XmlNode refItemIdNode = FindChildByName(node, RefItemIdNodeName);
        if (!Guid.TryParse(refItemIdNode.InnerText, out _))
        {
            throw new ArgumentException(
                $"Could not read file: {FilePath} because reference item id \"{refItemIdNode.InnerText}\" could not be parded to Guid in {node.InnerXml}"
            );
        }
        XmlNode idNode = FindChildByName(node, IdNodeName);
        if (!Guid.TryParse(idNode.InnerText, out _))
        {
            throw new ArgumentException(
                $"Could not read file: {FilePath} because item id \"{idNode.InnerText}\" could not be parded to Guid in {node.InnerXml}"
            );
        }
    }

    protected override ArgumentException ThrowOnCouldNotReadNode(XmlNode node, string childName)
    {
        return new ArgumentException(
            $"Could not read file: {FilePath} because node \"{childName}\" was not found in {node.InnerXml}"
        );
    }

    private void AddRootNodeIfEmpty()
    {
        if (FirstChild == null)
        {
            XmlNode rootNode = CreateNode(XmlNodeType.Element, "Root", "");
            AppendChild(rootNode);
        }
    }

    private void Load()
    {
        if (File.Exists(FilePath))
        {
            base.Load(FilePath);
        }
    }

    private void AddOrReplace(XmlNode newNode)
    {
        Guid refItemId = new Guid(FindIdNode(newNode).InnerText);
        XmlNode nodeToUpdate = FirstChild
            .ChildNodes.Cast<XmlNode>()
            .FirstOrDefault(node => new Guid(FindIdNode(node).InnerText) == refItemId);
        XmlNode importedNode = ImportNode(newNode, true);
        if (nodeToUpdate == null)
        {
            FirstChild.AppendChild(importedNode);
        }
        else
        {
            FirstChild.ReplaceChild(importedNode, nodeToUpdate);
        }
    }

    private XmlNode FindIdNode(XmlNode documentationNode) =>
        FindChildByName(documentationNode, IdNodeName);

    private Guid GetId(XmlNode node) => new Guid(FindIdNode(node).InnerText);

    private XmlNode FindRefItemIdNode(XmlNode documentationNode) =>
        FindChildByName(documentationNode, RefItemIdNodeName);

    private Guid GetRefItemId(XmlNode node) => new Guid(FindRefItemIdNode(node).InnerText);
}
