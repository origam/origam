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

        DocXmlDocument docXmlDocument = GetDocumentFor(itemId: schemaItemId);
        IEnumerable<XmlNode> nodes = docXmlDocument.GetNodesFor(schemaItemId: schemaItemId);
        string documentation = new DocumentationCompleteXmlDocument(
            nodes: nodes
        ).GetDocumentationByDocType(docType: docType);
        return filePersistenceProvider.LocalizationCache.GetLocalizedString(
            elementId: schemaItemId,
            memberName: "Documentation " + docType,
            defaultString: documentation
        );
    }

    public override DocumentationComplete LoadDocumentation(Guid schemaItemId)
    {
        DocXmlDocument docXmlDocument = GetDocumentFor(itemId: schemaItemId);
        IEnumerable<XmlNode> nodes = docXmlDocument.GetNodesFor(schemaItemId: schemaItemId);
        return new DocumentationCompleteXmlDocument(nodes: nodes).ToDataSet();
    }

    public override void SaveDocumentation(
        DocumentationComplete documentationData,
        Guid schemaItemId
    )
    {
        if (IsEmpty(documentationData: documentationData))
        {
            fileEventQueue.Pause();
            DocXmlDocument docXmlDocument = GetDocumentFor(itemId: schemaItemId);
            docXmlDocument.RemoveOutDatedNodes(
                newNodes: new List<XmlNode>(),
                refItemId: schemaItemId
            );
            docXmlDocument.Save();
            RemovedCachedDocumentFor(itemId: schemaItemId);
            fileEventQueue.Continue();
            return;
        }
        var dataSetXmlDocument = new DocumentationCompleteXmlDocument(dataSet: documentationData);

        fileEventQueue.Pause();
        var modifiedDocuments = new List<DocXmlDocument>();
        foreach (Guid documentedItemId in dataSetXmlDocument.GetAllDocumentedItemIds())
        {
            if (!filePersistenceProvider.Has(id: documentedItemId))
            {
                continue;
            }

            DocXmlDocument docXmlDocument = GetDocumentFor(itemId: documentedItemId);
            List<XmlNode> docNodes = dataSetXmlDocument.GetNodesWith(refItemId: documentedItemId);
            docXmlDocument.RemoveOutDatedNodes(newNodes: docNodes, refItemId: schemaItemId);
            docXmlDocument.AddOrReplace(newNodes: docNodes);
            modifiedDocuments.Add(item: docXmlDocument);
        }
        foreach (var docXmlDocument in modifiedDocuments)
        {
            docXmlDocument.Save();
            UpdateFileHash(docFilePath: docXmlDocument.FilePath);
            loadedDocFiles.RemoveByValueSelector(valueSelectorFunc: doc => doc == docXmlDocument);
        }
        fileEventQueue.Continue();
    }

    public override void SaveDocumentation(DocumentationComplete documentationData)
    {
        SaveDocumentation(documentationData: documentationData, schemaItemId: Guid.Empty);
    }

    private void UpdateFileHash(string docFilePath)
    {
        string hash = new FileInfo(fileName: docFilePath).GetFileBase64Hash();
        changedFileHashDictionary.AddOrReplace(key: docFilePath, value: hash);
    }

    private static bool IsEmpty(DocumentationComplete documentationData) =>
        documentationData.Tables.Count == 0 || documentationData.Documentation.Rows.Count == 0;

    private DocXmlDocument GetDocumentFor(Guid itemId)
    {
        DirectoryInfo packageDirectory = filePersistenceProvider.GetParentPackageDirectory(
            itemId: itemId
        );
        string docFilePath = Path.Combine(
            path1: packageDirectory.FullName,
            path2: DocumentationXmlDocument.DocFilename
        );
        if (!loadedDocFiles.ContainsKey(key: docFilePath))
        {
            loadedDocFiles.Add(key: docFilePath, value: new DocXmlDocument(filePath: docFilePath));
        }
        return loadedDocFiles[key: docFilePath];
    }

    private void RemovedCachedDocumentFor(Guid itemId)
    {
        DirectoryInfo packageDirectory = filePersistenceProvider.GetParentPackageDirectory(
            itemId: itemId
        );
        string docFilePath = Path.Combine(
            path1: packageDirectory.FullName,
            path2: DocumentationXmlDocument.DocFilename
        );
        loadedDocFiles.Remove(key: docFilePath);
    }

    public override DocumentationComplete GetAllDocumentation()
    {
        IEnumerable<XmlNode> documentationNodes = filePersistenceProvider
            .TopDirectory.GetAllFilesInSubDirectories()
            .Where(predicate: file => file.Extension == DocumentationXmlDocument.DocFilename)
            .Select(selector: file => new DocXmlDocument(filePath: file.FullName))
            .SelectMany(selector: xmlDoc => xmlDoc.DocNodes);
        var documentationCompleteXmlDocument = new DocumentationCompleteXmlDocument(
            nodes: documentationNodes
        );
        DocumentationComplete documentationData = documentationCompleteXmlDocument.ToDataSet();
        return documentationData;
    }

    public Maybe<string> GetDocumentationFileHash(FileInfo filePath)
    {
        changedFileHashDictionary.TryGetValue(key: filePath.FullName, value: out var hash);
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
        return node.ChildNodes.Cast<XmlNode>()
                .FirstOrDefault(predicate: child => child.Name == childName)
            ?? throw ThrowOnCouldNotReadNode(node: node, childName: childName);
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
        XmlNode rootNode = CreateNode(
            type: XmlNodeType.Element,
            name: "DocumentationComplete",
            namespaceURI: ""
        );
        AppendChild(newChild: rootNode);
        foreach (XmlNode node in nodes)
        {
            XmlNode importedNode = ImportNode(node: node, deep: true);
            FirstChild.AppendChild(newChild: importedNode);
        }
    }

    public DocumentationCompleteXmlDocument(DocumentationComplete dataSet)
    {
        string xml = dataSet.GetXml();
        LoadXml(xml: xml);
    }

    public DocumentationComplete ToDataSet()
    {
        var stringReader = new StringReader(s: InnerXml);
        var documentationComplete = new DocumentationComplete();
        documentationComplete.ReadXml(reader: stringReader);
        documentationComplete.ExtendedProperties[key: "ModelVersion"] =
            VersionProvider.CurrentModelMetaVersion;

        return documentationComplete;
    }

    public List<Guid> GetAllDocumentedItemIds()
    {
        return FirstChild
            .ChildNodes.Cast<XmlNode>()
            .Select(selector: GetRefItemId)
            .Distinct()
            .Where(predicate: id => id != Guid.Empty)
            .ToList();
    }

    public List<XmlNode> GetNodesWith(Guid refItemId)
    {
        return FirstChild
            .ChildNodes.Cast<XmlNode>()
            .Where(predicate: node => GetRefItemId(node: node) == refItemId)
            .ToList();
    }

    private Guid GetRefItemId(XmlNode node)
    {
        XmlNode refItemNode = FindChildByName(node: node, childName: RefItemIdNodeName);
        return new Guid(g: refItemNode.InnerText);
    }

    public string GetDocumentationByDocType(DocumentationType docType)
    {
        XmlNode nodeWithTheRightCategory = FirstChild
            .ChildNodes.Cast<XmlNode>()
            .FirstOrDefault(predicate: node => CategoryMatches(node: node, docType: docType));
        return nodeWithTheRightCategory != null
            ? FindChildByName(node: nodeWithTheRightCategory, childName: DataNodeName).InnerText
            : null;
    }

    private bool CategoryMatches(XmlNode node, DocumentationType docType) =>
        FindChildByName(node: node, childName: CategoryNodeName).InnerText == docType.ToString();

    protected override ArgumentException ThrowOnCouldNotReadNode(XmlNode node, string childName)
    {
        return new ArgumentException(
            message: $"Could not read DataSet node because node \"{childName}\" was not found in {node.InnerXml}"
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
        Save(filename: FilePath);
    }

    public void RemoveOutDatedNodes(List<XmlNode> newNodes, Guid refItemId)
    {
        HashSet<Guid> newNodeIds = new HashSet<Guid>(collection: newNodes.Select(selector: GetId));
        GetNodesFor(schemaItemId: refItemId)
            .Where(predicate: node => !newNodeIds.Contains(item: GetId(node: node)))
            .ForEach(action: node => FirstChild.RemoveChild(oldChild: node));
    }

    public void AddOrReplace(List<XmlNode> newNodes)
    {
        foreach (XmlNode newNode in newNodes)
        {
            AddOrReplace(newNode: newNode);
        }
    }

    public IEnumerable<XmlNode> GetNodesFor(Guid schemaItemId)
    {
        return FirstChild
            .ChildNodes.Cast<XmlNode>()
            .Where(predicate: node => GetRefItemId(node: node) == schemaItemId)
            .ToList();
    }

    private void CheckNodes()
    {
        FirstChild.ChildNodes.Cast<XmlNode>().ForEach(action: ReadChildrenOrThrow);
    }

    private void ReadChildrenOrThrow(XmlNode node)
    {
        XmlNode dataNode = FindChildByName(node: node, childName: DataNodeName);
        XmlNode categoryNode = FindChildByName(node: node, childName: CategoryNodeName);
        if (
            !Enum.TryParse(
                value: categoryNode.InnerText,
                ignoreCase: true,
                result: out DocumentationType _
            )
        )
        {
            throw new ArgumentException(
                message: $"Could not read file: {FilePath} because documentation category \"{categoryNode.InnerText}\" was wrong in {node.InnerXml}"
            );
        }
        XmlNode refItemIdNode = FindChildByName(node: node, childName: RefItemIdNodeName);
        if (!Guid.TryParse(input: refItemIdNode.InnerText, result: out _))
        {
            throw new ArgumentException(
                message: $"Could not read file: {FilePath} because reference item id \"{refItemIdNode.InnerText}\" could not be parded to Guid in {node.InnerXml}"
            );
        }
        XmlNode idNode = FindChildByName(node: node, childName: IdNodeName);
        if (!Guid.TryParse(input: idNode.InnerText, result: out _))
        {
            throw new ArgumentException(
                message: $"Could not read file: {FilePath} because item id \"{idNode.InnerText}\" could not be parded to Guid in {node.InnerXml}"
            );
        }
    }

    protected override ArgumentException ThrowOnCouldNotReadNode(XmlNode node, string childName)
    {
        return new ArgumentException(
            message: $"Could not read file: {FilePath} because node \"{childName}\" was not found in {node.InnerXml}"
        );
    }

    private void AddRootNodeIfEmpty()
    {
        if (FirstChild == null)
        {
            XmlNode rootNode = CreateNode(
                type: XmlNodeType.Element,
                name: "Root",
                namespaceURI: ""
            );
            AppendChild(newChild: rootNode);
        }
    }

    private void Load()
    {
        if (File.Exists(path: FilePath))
        {
            base.Load(filename: FilePath);
        }
    }

    private void AddOrReplace(XmlNode newNode)
    {
        Guid refItemId = new Guid(g: FindIdNode(documentationNode: newNode).InnerText);
        XmlNode nodeToUpdate = FirstChild
            .ChildNodes.Cast<XmlNode>()
            .FirstOrDefault(predicate: node =>
                new Guid(g: FindIdNode(documentationNode: node).InnerText) == refItemId
            );
        XmlNode importedNode = ImportNode(node: newNode, deep: true);
        if (nodeToUpdate == null)
        {
            FirstChild.AppendChild(newChild: importedNode);
        }
        else
        {
            FirstChild.ReplaceChild(newChild: importedNode, oldChild: nodeToUpdate);
        }
    }

    private XmlNode FindIdNode(XmlNode documentationNode) =>
        FindChildByName(node: documentationNode, childName: IdNodeName);

    private Guid GetId(XmlNode node) => new Guid(g: FindIdNode(documentationNode: node).InnerText);

    private XmlNode FindRefItemIdNode(XmlNode documentationNode) =>
        FindChildByName(node: documentationNode, childName: RefItemIdNodeName);

    private Guid GetRefItemId(XmlNode node) =>
        new Guid(g: FindRefItemIdNode(documentationNode: node).InnerText);
}
