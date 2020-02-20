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

        public MetaModelUpGrader(Assembly scriptAssembly, IFileWriter fileWriter)
        {
            this.scriptAssembly = scriptAssembly;
            this.fileWriter = fileWriter;
        }

        public MetaModelUpGrader()
        {
            fileWriter = new FileWriter();
            scriptAssembly = GetType().Assembly;
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
            Type typeToUpgrade = FindType(classNode);
            Versions currentClassVersions = Versions.GetCurrentClassVersion(typeToUpgrade);
            Versions persistedClassVersions = Versions.GetPersistedClassVersion(classNode, typeToUpgrade);
            
            foreach (var pair in currentClassVersions)
            {
                Type type = pair.Key;
                Version currentVersion = pair.Value;
                if (!persistedClassVersions.ContainsKey(type))
                {
                    RunUpgradeScripts(classNode, xmlFileData, type,
                        new Version("1.0.0"), currentVersion);
                    continue;
                }

                if (persistedClassVersions[type] > currentVersion)
                {
                    throw new Exception($"Class version written in persisted object is greater than current version of the class. This should never happen, please check version of {classNode.Name} in {xmlFileData.FileInfo.FullName}");
                }

                if (persistedClassVersions[type] < currentVersion)
                {
                    RunUpgradeScripts(classNode, xmlFileData, type,
                        persistedClassVersions[type], currentVersion);
                }
            }

            string upgradedXmlString = OrigamDocumentSorter
                .CopyAndSort(xmlFileData.XmlDocument)
                .ToBeautifulString();
                
            fileWriter.Write(xmlFileData.FileInfo, upgradedXmlString);
        }

        private void RunUpgradeScripts(XmlNode classNode,
            XmlFileData xmlFileData, Type typeToUpgrade,
            Version persistedClassVersion,
            Version currentClassVersion)
        {
            Type upgradeScriptContainerClass = scriptAssembly.GetTypes()
               .SingleOrDefault(type =>
                   type.IsClass && type.BaseType.IsGenericType &&
                   type.BaseType.GetGenericTypeDefinition() == typeof(UpgradeScriptContainer<>) &&
                   type.BaseType.GetGenericArguments()[0] == typeToUpgrade)
           ?? throw new ClassUpgradeException($"Could not find exactly one ancestor of {typeof(UpgradeScriptContainer<>).Name}<T> with generic type of {typeToUpgrade.Name}");

            var upgradeScriptContainer = Activator.CreateInstance(upgradeScriptContainerClass);
            MethodInfo upgradeMethod = upgradeScriptContainerClass.GetMethod("Upgrade");
            upgradeMethod.Invoke(upgradeScriptContainer,
                new object[] {xmlFileData.XmlDocument, classNode, persistedClassVersion, currentClassVersion});
        }

        private Type FindType(XmlNode classNode)
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
            string assemblyName = typeName.Substring(0, typeName.LastIndexOf('.'));
            Type type = Type.GetType(typeName + "," + assemblyName);
            if (type == null)
            {
                throw new Exception($"Type of {classNode.Name} could not be found");
            }
            return type;
        }
    }
}