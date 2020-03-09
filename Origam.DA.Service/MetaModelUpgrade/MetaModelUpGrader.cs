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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using MoreLinq;
using Origam.DA.Common;
using Origam.Extensions;

namespace Origam.DA.Service.MetaModelUpgrade
{
    public class MetaModelUpGrader
    {
        private readonly Assembly scriptAssembly;
        private readonly IFileWriter fileWriter;

        private static List<UpgradeScriptContainer> scriptContainers;
        private readonly Version firstVersion = new Version("6.0.0");

        private static void InstantiateScriptContainers(Assembly scriptAssembly)
        {
            if (scriptContainers != null) return;
            scriptContainers = scriptAssembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(UpgradeScriptContainer)))
                .Select(Activator.CreateInstance)
                .Cast<UpgradeScriptContainer>()
                .ToList();
        }

        public MetaModelUpGrader(Assembly scriptAssembly, IFileWriter fileWriter)
        {
            this.scriptAssembly = scriptAssembly;
            this.fileWriter = fileWriter;
            InstantiateScriptContainers(scriptAssembly);
        }

        public MetaModelUpGrader()
        {
            fileWriter = new FileWriter();
            scriptAssembly = GetType().Assembly;
            InstantiateScriptContainers(scriptAssembly);
        }

        public bool TryUpgrade(List<XmlFileData> xmlData)
        {
            foreach (XmlFileData xmlFileData in xmlData)
            {
                bool isVersion5 = xmlFileData.XmlDocument.FileElement
                    .Attributes
                    .Cast<XmlAttribute>()
                    .Any(attr => attr.Value == "http://schemas.origam.com/5.0.0/model-element");
                if (isVersion5)
                {
                    new Version6UpGrader(xmlFileData.XmlDocument).Run();
                }
                xmlFileData.XmlDocument
                    .ClassNodes
                    .ForEach(classNode => TryUpgrade(classNode, xmlFileData));
                
                WriteToFile(xmlFileData);
            }

            return false;
        }

        private void WriteToFile(XmlFileData xmlFileData)
        {
            xmlFileData.XmlDocument.FixNamespaces();
            string upgradedXmlString = OrigamDocumentSorter
                .CopyAndSort(xmlFileData.XmlDocument)
                .ToBeautifulString();

            fileWriter.Write(xmlFileData.FileInfo, upgradedXmlString);
        }

        private void TryUpgrade(XmlNode classNode, XmlFileData xmlFileData)
        {
            IEnumerable<OrigamNameSpace> origamNameSpaces = GetOrigamNameSpaces(classNode);
            
            string nodeClass = OrigamNameSpace.Create(classNode?.NamespaceURI).FullTypeName;
            Versions persistedClassVersions = new Versions(origamNameSpaces);
            Versions currentClassVersions =
                Versions.GetCurrentClassVersions(nodeClass, persistedClassVersions);
            
            foreach (var pair in currentClassVersions)
            {
                string className = pair.Key;
                Version currentVersion = pair.Value;
                if (currentVersion == firstVersion)
                {
                    continue;
                }

                if (!persistedClassVersions.ContainsKey(className))
                {
                    RunUpgradeScripts(classNode, xmlFileData, className,
                        firstVersion, currentVersion);
                    continue;
                }

                if (persistedClassVersions[className] > currentVersion)
                {
                    throw new Exception($"Class version written in persisted object is greater than current version of the class. This should never happen, please check version of {classNode.Name} in {xmlFileData.FileInfo.FullName}");
                }

                if (persistedClassVersions[className] < currentVersion)
                {
                    RunUpgradeScripts(classNode, xmlFileData, className,
                        persistedClassVersions[className], currentVersion);
                }
            }
        }

        private static IEnumerable<OrigamNameSpace> GetOrigamNameSpaces(XmlNode classNode)
        {
            return classNode.Attributes
                .Cast<XmlAttribute>()
                .Select(attr => attr.NamespaceURI)
                .Distinct()
                .Select(OrigamNameSpace.Create);
        }

        private void RunUpgradeScripts(XmlNode classNode,
            XmlFileData xmlFileData, string className,
            Version persistedClassVersion,
            Version currentClassVersion)
        {
            var containers = scriptContainers
                .Where(container =>
                    container.FullTypeName == className ||
                    container.OldFullTypeNames != null &&
                    container.OldFullTypeNames.Contains(className))
                .ToArray();
            if (containers.Length != 1)
            {
                throw new ClassUpgradeException($"Could not find exactly one ancestor of {typeof(UpgradeScriptContainer).Name} which upgrades type of \"{className}\"");
            }
            var upgradeScriptContainer = containers[0];
            upgradeScriptContainer.Upgrade(xmlFileData.XmlDocument, classNode, persistedClassVersion, currentClassVersion);
        }
    }
    public class Version6UpGrader
    {
        private readonly OrigamXmlDocument xmlDocument;
        private static string oldPersistenceNamespace = "http://schemas.origam.com/1.0.0/model-persistence";
        private static string newPersistenceNamespace = "http://schemas.origam.com/model-persistence/1.0.0";

        public Version6UpGrader(OrigamXmlDocument xmlDocument)
        {
            this.xmlDocument = xmlDocument;
        }

        public void Run()
        {
            RemoveOldDocumentNamespaces();

            foreach (XmlElement oldNode in xmlDocument.ClassNodes)
            {
                var type = RemoveTypeAttribute(oldNode);

                var namespaceMapping = new PropertyToNamespaceMapping(
                    instanceType: type, 
                    xmlNamespaceMapper: Version6NamespaceMapper);
                namespaceMapping.AddNamespacesToDocument(xmlDocument);

                XmlElement newNode = xmlDocument.CreateElement
                    (oldNode.Name, namespaceMapping.NodeNamespace);
                newNode.Prefix = namespaceMapping.NodeNamespaceName;
                CopyAttributes(oldNode, newNode, namespaceMapping);
                
                oldNode.ParentNode.AppendChild(newNode);
                oldNode.ParentNode.RemoveChild(oldNode);
            }
        }

        private string Version6NamespaceMapper(Type type)
        {
            string xmlNameSpaceWithCurrentVersion = XmlNamespaceTools.GetXmlNameSpace(type);
            return 
                string.Join(
                    "/", 
                    xmlNameSpaceWithCurrentVersion
                            .Split("/")
                            .SkipLast(1)
                            .Concat(new []{"6.0.0"})
                    );
        }

        private static void CopyAttributes(XmlElement oldNode, XmlElement newNode,
            PropertyToNamespaceMapping namespaceMapping)
        {
            foreach (var attribute in oldNode.Attributes.ToArray<XmlAttribute>())
            {
                string namespaceUri = attribute.NamespaceURI == oldPersistenceNamespace
                    ? newPersistenceNamespace
                    : namespaceMapping.GetNamespaceByXmlAttributeName(attribute.Name);
                
                newNode.SetAttribute(
                    attribute.LocalName,
                    namespaceUri,
                    attribute.Value);
            }
        }

        private Type RemoveTypeAttribute(XmlElement node)
        {
            if (string.IsNullOrWhiteSpace(node?.Attributes?["x:type"]?.Value))
            {
                throw new Exception(
                    $"Cannot get type from node: {node.Name} in \n{xmlDocument.OuterXml}");
            }

            XmlAttribute typeAttribute = node.Attributes["x:type"];
            Type type = Reflector.GetTypeByName(typeAttribute.Value);
            node.Attributes.Remove(typeAttribute);
            return type;
        }

        private void RemoveOldDocumentNamespaces()
        {
            xmlDocument.FileElement.Attributes
                .Cast<XmlAttribute>()
                .Where(
                    attr => attr.Value ==
                            "http://schemas.origam.com/5.0.0/model-element" ||
                            attr.Value == "http://schemas.origam.com/1.0.0/package")
                .ToArray()
                .ForEach(attr => xmlDocument.FileElement.Attributes.Remove(attr));
            
            xmlDocument.FileElement.SetAttribute("xmlns:x", newPersistenceNamespace);
        }
    }
}