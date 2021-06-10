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
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using MoreLinq;
using Origam.DA.Common;
using Origam.DA.Service.NamespaceMapping;
using Origam.Extensions;
using Origam.Schema;

namespace Origam.DA.Service.MetaModelUpgrade
{
    public class MetaModelUpgrader
    {
        private readonly ScriptContainerLocator scriptLocator;
        private readonly IFileWriter fileWriter;

        private readonly Version firstVersion = new Version("6.0.0");
        public MetaModelUpgrader(Assembly scriptAssembly, IFileWriter fileWriter)
        {
            scriptLocator = new ScriptContainerLocator(scriptAssembly);
            this.fileWriter = fileWriter;
        }

        public MetaModelUpgrader(IFileWriter fileWriter)
        {
            this.fileWriter = fileWriter;
            scriptLocator = new ScriptContainerLocator(GetType().Assembly);
        }

        public MetaModelUpgrader()
        {
            fileWriter = new FileWriter();
            scriptLocator = new ScriptContainerLocator(GetType().Assembly);
        }
        
        public bool TryUpgrade(XFileData xFileData)
        {
            bool isVersion5 = IsVersion5(xFileData);
            if (isVersion5)
            {
                new Version6Upgrader(scriptLocator, xFileData.Document).Run();
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
                    if (currentVersion != firstVersion)
                    {
                        RunUpgradeScripts(classNode, documentContainer, className,
                            firstVersion, currentVersion); 
                        scriptsRun = true;
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
                    RunUpgradeScripts(classNode, documentContainer, className,
                        persistedClassVersions[className], currentVersion);
                    scriptsRun = true;
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
                .Where(name => name != OrigamFile.ModelPersistenceUri)
                .Distinct()
                .Select(OrigamNameSpace.CreateOrGet);
        }

        private void RunUpgradeScripts(XElement classNode,
            DocumentContainer documentContainer, string className,
            Version persistedClassVersion,
            Version currentClassVersion)
        {
            // do not try to upgrade first versions
            if (currentClassVersion.Equals(new Version("1.0.0")))
            {
                return;
            }
            var upgradeScriptContainer = scriptLocator.TryFindByTypeName(className);
            if (upgradeScriptContainer == null)
            {
                if (currentClassVersion == Versions.Last)
                {
                    throw new ClassUpgradeException($" No {nameof(ClassMetaVersionAttribute)} was found in on \"{className}\" and no upgrade scripts for that class were found either. May be you meant to add the attribute?");
                }
                throw new ClassUpgradeException($"Could not find ancestor of {typeof(UpgradeScriptContainer).Name} which upgrades type of \"{className}\"");
            }
            
            upgradeScriptContainer.Upgrade(documentContainer, classNode, persistedClassVersion, currentClassVersion);
        }
    }
    
    public class Version6Upgrader
    {
        private readonly ScriptContainerLocator scriptLocator;
        private readonly OrigamXDocument document;
        private static XNamespace oldPersistenceNamespace = "http://schemas.origam.com/1.0.0/model-persistence";
        private static XNamespace newPersistenceNamespace = OrigamFile.ModelPersistenceUri;

        public Version6Upgrader(ScriptContainerLocator scriptLocator,
            OrigamXDocument document)
        {
            this.scriptLocator = scriptLocator;
            this.document = document;
        }

        public void Run()
        {
            RemoveOldDocumentNamespaces();

            foreach (XElement node in document.ClassNodes)
            {
                if (node.Name.LocalName == "groupReference")
                {
                    UpgradeGroupReferenceNodeNode(node);
                }
                else
                {
                    UpgradeNode(node);
                }
            }
        }

        private void UpgradeGroupReferenceNodeNode(XElement node)
        {
            var typeAttribute = node.Attribute(oldPersistenceNamespace.GetName("type"));
            string category;
            switch (typeAttribute.Value)
            {
                case "http://schemas.origam.com/1.0.0/packagepackage":
                    category = OrigamFile.PackageCategory;
                    break;
                case "http://schemas.origam.com/5.0.0/model-elementgroup":
                    category = OrigamFile.GroupCategory;
                    break;
                default:
                    throw new Exception($"Cannot convert node {node} to meta model version 6.0.0 because {typeAttribute.Value} cannot be mapped to category");
            }
            typeAttribute.Remove();
            node.SetAttributeValue(newPersistenceNamespace.GetName("type"), category);

            var refIdAttribute = node.Attribute(oldPersistenceNamespace.GetName("refId"));
            refIdAttribute.Remove();
            node.SetAttributeValue(newPersistenceNamespace.GetName("refId"), refIdAttribute.Value);

            node.Name = newPersistenceNamespace.GetName(node.Name.LocalName);
        }

        private void UpgradeNode(XElement node)
        {
            IPropertyToNamespaceMapping namespaceMapping = GetNamespaceMapping(node);
            namespaceMapping.AddNamespacesToDocumentAndAdjustMappings(document);

            RemoveTypeAttribute(node);
            node.Name = namespaceMapping.NodeNamespace.GetName(node.Name.LocalName);
            CopyAttributes(node, namespaceMapping);
        }
        
        private static void CopyAttributes(XElement node,
            IPropertyToNamespaceMapping namespaceMapping)
        {
            List<XAttribute> atList = node
                .Attributes()
                .Where(attr => attr.Name.LocalName != "xmlns")
                .ToList();  
            
            node.Attributes().Remove();  
            foreach (XAttribute attribute in atList){
                XNamespace nameSpace = attribute.Name.Namespace == oldPersistenceNamespace
                    ? newPersistenceNamespace
                    : namespaceMapping.GetNamespaceByXmlAttributeName(attribute.Name.LocalName);
                node.Add(new XAttribute(nameSpace.GetName(attribute.Name.LocalName), attribute.Value));
            }
        }

        private IPropertyToNamespaceMapping GetNamespaceMapping(XElement node)
        {
            XName name = oldPersistenceNamespace.GetName("type");
            XAttribute typeAttribute = node?.Attribute(name);
            if (string.IsNullOrWhiteSpace(typeAttribute?.Value))
            {
                throw new Exception(
                    $"Cannot get type from node: {node?.Name} in \n{document}");
            }
            
            Type type = Reflector.GetTypeByName(typeAttribute.Value);
            bool classIsDearOrRenamed = type == null;
            if (classIsDearOrRenamed)
            {
                var scriptContainer = scriptLocator.FindByTypeName(typeAttribute.Value);
                type = Reflector.GetTypeByName(scriptContainer.FullTypeName);
                
                bool classIsDead = type == null;
                if (classIsDead)
                {
                    return new DeadClassPropertyToNamespaceMapping(
                        scriptContainer.FullTypeName,
                        new Version(6,0,0));
                }
            }

            return Version6PropertyToNamespaceMapping
                .CreateOrGet(type, scriptLocator)
                .DeepCopy() ;
        }  
        
        private void RemoveTypeAttribute(XElement node)
        {
            XName name = oldPersistenceNamespace.GetName("type");
            XAttribute typeAttribute = node?.Attribute(name);
            typeAttribute?.Remove();
        }

        private void RemoveOldDocumentNamespaces()
        {
            document.FileElement
                .Attributes()
                .Where(
                    attr => attr.Value ==
                            "http://schemas.origam.com/5.0.0/model-element" ||
                            attr.Value == "http://schemas.origam.com/1.0.0/package")
                .Remove();
            document.FileElement.Name = newPersistenceNamespace.GetName(document.FileElement.Name.LocalName);
            document.FileElement.Attribute(XNamespace.Xmlns + "x").Value = newPersistenceNamespace.ToString();
        }
    }

    public class UpgradeProgressInfo
    {
        public int TotalFiles { get;}
        public int FilesDone { get; }

        public UpgradeProgressInfo(int totalFiles, int filesDone)
        {
            TotalFiles = totalFiles;
            FilesDone = filesDone;
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
}