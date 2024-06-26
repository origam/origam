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
using System.Xml.Linq;
using Origam.DA.Common;
using Origam.Extensions;

namespace Origam.DA.Service.MetaModelUpgrade;
public class MetaModelAnalyzer
{
    private readonly IMetaModelUpgrader metaModelUpgrader;
    private readonly IFileWriter fileWriter;
    
    public MetaModelAnalyzer(IFileWriter fileWriter, IMetaModelUpgrader metaModelUpgrader)
    {
        this.fileWriter = fileWriter;
        this.metaModelUpgrader = metaModelUpgrader;
    }
    public MetaModelAnalyzer(IMetaModelUpgrader metaModelUpgrader)
    {
        this.metaModelUpgrader = metaModelUpgrader;
        fileWriter = new FileWriter();
    }
    
    public bool TryUpgrade(XFileData xFileData)
    {
        bool isVersion5 = IsVersion5(xFileData);
        if (isVersion5)
        {
            metaModelUpgrader.UpgradeToVersion6(xFileData.Document);
        }
        var documentContainer = new DocumentContainer(xFileData);
        List<bool> upgradeFlags = xFileData.Document
            .ClassNodes
            .ToArray()
            .Select(classNode => TryUpgrade(classNode, documentContainer))
            .ToList();
        documentContainer.ExecuteNamespaceUpgrade();
        
        if (isVersion5 || upgradeFlags.Any(x => x))
        {
            xFileData.Document.FixNamespaces();
            WriteFile(xFileData);
            return true;
        }
        return false;
    }
    private bool IsVersion5(XFileData xFileData)
    {
       return xFileData.Document
           .FileElement
           .Attributes()
           .Select(attr => attr.Value.Trim().ToLower())
           .Any(value => 
               value == "http://schemas.origam.com/5.0.0/model-element" ||
               value == "http://schemas.origam.com/1.0.0/model-persistence" ||
               value == "http://schemas.origam.com/1.0.0/packagepackage");
    }
    private void WriteFile(XFileData xFileData)
    {
        if (xFileData.Document.IsEmpty)
        {
            fileWriter.Delete(xFileData.File);
        }
        else
        {
            var xmlDocument = new OrigamXmlDocument(xFileData.Document.XDocument);
            string upgradedXmlString = OrigamDocumentSorter
                .CopyAndSort(xmlDocument)
                .ToBeautifulString();
            fileWriter.Write(xFileData.File, upgradedXmlString);
        }
    }
    private bool TryUpgrade(XElement classNode, DocumentContainer documentContainer)
    {
        IEnumerable<OrigamNameSpace> origamNameSpaces = GetOrigamNameSpaces(classNode);
        
        string fullTypeName = OrigamNameSpace.CreateOrGet(classNode?.Name.NamespaceName).FullTypeName;
        Versions persistedClassVersions = new Versions(origamNameSpaces);
        Versions currentClassVersions = GetCurrentClassVersions(fullTypeName, persistedClassVersions);
        bool scriptsRun = false;
        foreach (string className in currentClassVersions.TypeNames)
        {
            Version currentVersion = currentClassVersions[className];
            if (!persistedClassVersions.Contains(className))
            {
                if (currentVersion != ClassMetaVersionAttribute.FirstVersion &&
                    currentVersion != ClassMetaVersionAttribute.FormerFirstVersion)
                {
                    scriptsRun = metaModelUpgrader.RunUpgradeScripts(classNode, documentContainer, className,
                        ClassMetaVersionAttribute.FirstVersion, currentVersion);
                }
                continue;
            }
            if (persistedClassVersions[className] == currentVersion)
            {
                continue;
            }
            if (persistedClassVersions[className] > currentVersion)
            {
                throw new Exception(
                    $"Error when processing file \"{documentContainer.File.FullName}\". " +
                    $"The persisted object \"{classNode.Name.LocalName}\" has a newer version of the" +
                    $" class \"{documentContainer.File.FullName}\" (version: {persistedClassVersions[className]}) " +
                    $"than is the current version of the class: {currentVersion}. Are you trying to open a model with an older Architect/Server?");
            }
            if (persistedClassVersions[className] < currentVersion)
            {
                scriptsRun = metaModelUpgrader.RunUpgradeScripts(classNode, documentContainer, className,
                    persistedClassVersions[className], currentVersion);
            }
        }
        return scriptsRun;
    }
    private static Versions GetCurrentClassVersions(string fullTypeName,
        Versions persistedClassVersions)
    {
        Versions currentClassVersions =
            Versions.GetCurrentClassVersions(fullTypeName);
        if (!currentClassVersions.IsDead)
        {
            var deadClasses = persistedClassVersions
                .TypeNames
                .Where(typeName => !currentClassVersions.Contains(typeName));
            currentClassVersions = new Versions(currentClassVersions, deadClasses);
        }
        return currentClassVersions;
    }
    private static IEnumerable<OrigamNameSpace> GetOrigamNameSpaces(XElement classNode)
    {
        return classNode.Attributes()
            .Select(attr => attr.Name.NamespaceName)
            .Concat(new[]{classNode.Name.NamespaceName})
            .Where(name => 
                OrigamNameSpace.IsOrigamNamespace(name) && 
                name != OrigamFile.ModelPersistenceUri)
            .Distinct()
            .Select(OrigamNameSpace.CreateOrGet);
    }
}
public class DocumentContainer
{
    private readonly XFileData fileData;
    public OrigamXDocument Document => fileData.Document;
    public FileInfo File => fileData.File;
    private readonly Dictionary<UpgradeScriptContainer, Version> scriptVersionPairs 
        = new Dictionary<UpgradeScriptContainer, Version>();
    public DocumentContainer(XFileData fileData)
    {
        this.fileData = fileData;
    }
    public void ScheduleNamespaceUpgrade(UpgradeScriptContainer scriptContainer, Version toVersion)
    {
        scriptVersionPairs[scriptContainer] = toVersion;
    }
    public void ExecuteNamespaceUpgrade()
    {
        foreach (var scriptVersionPair in scriptVersionPairs)
        {
            Version toVersion = scriptVersionPair.Value;
            var upgradeScriptContainer = scriptVersionPair.Key;
            upgradeScriptContainer.SetVersion(Document, toVersion);
        }
    }
}
