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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using CSharpFunctionalExtensions;
using MoreLinq;
using Origam.Extensions;

namespace Origam.DA.Service.MetaModelUpgrade
{
    public class MetaModelUpGrader
    {
        private readonly Assembly scriptAssembly;
        private readonly IFileWriter fileWriter;

        private static List<UpgradeScriptContainer> scriptContainers;

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
                xmlFileData.XmlDocument
                    .GetAllNodes()
                    .Where(node => node.Name != "x:file" && node.Name != "xml")
                    .ForEach(classNode => TryUpgrade(classNode, xmlFileData));
            }

            return false;
        }

        private void TryUpgrade(XmlNode classNode, XmlFileData xmlFileData)
        {
            string classToUpgrade = GetClassName(classNode);
            Versions currentClassVersions = Versions.GetCurrentClassVersion(classToUpgrade);
            Versions persistedClassVersions = Versions.GetPersistedClassVersion(classNode, classToUpgrade);
            
            foreach (var pair in currentClassVersions)
            {
                string className = pair.Key;
                Version currentVersion = pair.Value;
                if (!persistedClassVersions.ContainsKey(className))
                {
                    RunUpgradeScripts(classNode, xmlFileData, className,
                        new Version("1.0.0"), currentVersion);
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

            string upgradedXmlString = OrigamDocumentSorter
                .CopyAndSort(xmlFileData.XmlDocument)
                .ToBeautifulString();
                
            fileWriter.Write(xmlFileData.FileInfo, upgradedXmlString);
        }

        private void RunUpgradeScripts(XmlNode classNode,
            XmlFileData xmlFileData, string className,
            Version persistedClassVersion,
            Version currentClassVersion)
        {
            var upgradeScriptContainer = scriptContainers
                 .SingleOrDefault(container => container.ClassName == className) 
                 ?? throw new ClassUpgradeException($"Could not find exactly one ancestor of {typeof(UpgradeScriptContainer).Name}<T> with generic type of {className}");
            upgradeScriptContainer.Upgrade(xmlFileData.XmlDocument, classNode, persistedClassVersion, currentClassVersion);
        }

        private string GetClassName(XmlNode classNode)
        {
            if (classNode == null)
            {
                throw new ArgumentNullException(nameof(classNode));
            }

            var typeName = classNode.Attributes["x:type"]?.Value;

            if (typeName == null)
            {
                throw new Exception($"Cannot get meta version of class {classNode.Name} because it does not have \"x:type\" attribute");
            }
            return typeName;
        }
    }
}