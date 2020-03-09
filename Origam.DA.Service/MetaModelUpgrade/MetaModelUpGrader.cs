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
using System.Xml.Linq;
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

        public bool TryUpgrade(List<XFileData> xmlData)
        {
            foreach (XFileData xFileData in xmlData)
            {
                bool isVersion5 = xFileData.Document.FileElement
                    .Attributes()
                    .Any(attr => attr.Value == "http://schemas.origam.com/5.0.0/model-element");
                if (isVersion5)
                {
                    new Version6UpGrader(xFileData.Document).Run();
                }
                xFileData.Document
                    .ClassNodes
                    .ForEach(classNode => TryUpgrade(classNode, xFileData));
                
                WriteToFile(xFileData);
            }

            return false;
        }

        private void WriteToFile(XFileData xFileData)
        {
            // xFileData.XmlDocument.FixNamespaces();
            // string upgradedXmlString = OrigamDocumentSorter
            //     .CopyAndSort(xFileData.XmlDocument)
            //     .ToBeautifulString();

            // fileWriter.Write(xFileData.FileInfo, upgradedXmlString);
        }

        private void TryUpgrade(XElement classNode, XFileData xFileData)
        {
            IEnumerable<OrigamNameSpace> origamNameSpaces = GetOrigamNameSpaces(classNode);
            
            string nodeClass = OrigamNameSpace.Create(classNode?.Name.NamespaceName).FullTypeName;
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
                    RunUpgradeScripts(classNode, xFileData, className,
                        firstVersion, currentVersion);
                    continue;
                }

                if (persistedClassVersions[className] > currentVersion)
                {
                    throw new Exception($"Class version written in persisted object is greater than current version of the class. This should never happen, please check version of {classNode.Name} in {xFileData.File.FullName}");
                }

                if (persistedClassVersions[className] < currentVersion)
                {
                    RunUpgradeScripts(classNode, xFileData, className,
                        persistedClassVersions[className], currentVersion);
                }
            }
        }

        private static IEnumerable<OrigamNameSpace> GetOrigamNameSpaces(XElement classNode)
        {
            return classNode.Attributes()
                .Select(attr => attr.Name.NamespaceName)
                .Distinct()
                .Select(OrigamNameSpace.Create);
        }

        private void RunUpgradeScripts(XElement classNode,
            XFileData xFileData, string className,
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
            upgradeScriptContainer.Upgrade(xFileData.Document, classNode, persistedClassVersion, currentClassVersion);
        }
    }
    public class Version6UpGrader
    {
        private readonly OrigamXDocument xDocument;
        private static XNamespace oldPersistenceNamespace = "http://schemas.origam.com/1.0.0/model-persistence";
        private static XNamespace newPersistenceNamespace = "http://schemas.origam.com/model-persistence/1.0.0";

        public Version6UpGrader(OrigamXDocument xDocument)
        {
            this.xDocument = xDocument;
        }

        public void Run()
        {
            RemoveOldDocumentNamespaces();

            foreach (XElement node in xDocument.ClassNodes)
            {
                var type = RemoveTypeAttribute(node);

                var namespaceMapping = new PropertyToNamespaceMapping(
                    instanceType: type, 
                    xmlNamespaceMapper: Version6NamespaceMapper);
                namespaceMapping.AddNamespacesToDocument(xDocument);
                
                node.Name = namespaceMapping.NodeNamespace.GetName(node.Name.LocalName);
                CopyAttributes(node, namespaceMapping);
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

        private static void CopyAttributes(XElement node,
            PropertyToNamespaceMapping namespaceMapping)
        {
            List<XAttribute> atList = node.Attributes().ToList();  
            node.Attributes().Remove();  
            foreach (XAttribute attribute in atList){
                XNamespace nameSpace = attribute.Name.Namespace == oldPersistenceNamespace
                    ? newPersistenceNamespace
                    : namespaceMapping.GetNamespaceByXmlAttributeName(attribute.Name.LocalName);
                node.Add(new XAttribute(nameSpace.GetName(attribute.Name.LocalName), attribute.Value));
            }
        }

        private Type RemoveTypeAttribute(XElement node)
        {
            XName name = oldPersistenceNamespace.GetName("type");
            XAttribute typeAttribute = node?.Attribute(name);
            if (string.IsNullOrWhiteSpace(typeAttribute?.Value))
            {
                throw new Exception(
                    $"Cannot get type from node: {node?.Name} in \n{xDocument}");
            }
            
            Type type = Reflector.GetTypeByName(typeAttribute.Value);
            typeAttribute.Remove();
            return type;
        }

        private void RemoveOldDocumentNamespaces()
        {
            xDocument.FileElement
                .Attributes()
                .Where(
                    attr => attr.Value ==
                            "http://schemas.origam.com/5.0.0/model-element" ||
                            attr.Value == "http://schemas.origam.com/1.0.0/package")
                .Remove();
            xDocument.FileElement.Name = newPersistenceNamespace.GetName(xDocument.FileElement.Name.LocalName);
            xDocument.FileElement.Attribute(XNamespace.Xmlns + "x").Value = newPersistenceNamespace.ToString();
        }
    }
}