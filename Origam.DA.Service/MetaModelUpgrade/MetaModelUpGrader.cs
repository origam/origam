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
using MoreLinq;
using Origam.DA.Common;
using Origam.Extensions;

namespace Origam.DA.Service.MetaModelUpgrade
{
    public class MetaModelUpGrader
    {
        private readonly Assembly scriptAssembly;

        public MetaModelUpGrader(Assembly scriptAssembly)
        {
            this.scriptAssembly = scriptAssembly;
        }

        public MetaModelUpGrader()
        {
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
            Version currentClassVersion = GetCurrentClassVersion(classNode, typeToUpgrade);
            Version persistedClassVersion = GetPersistedClassVersion(classNode);
            
            if (currentClassVersion == persistedClassVersion) return ;
            if (currentClassVersion < persistedClassVersion) throw new Exception($"Class version written in persisted object is greater than current version of the class. This should never happen, please check version of {classNode.Name} in {xmlFileData.FileInfo.FullName}");
            
            RunUpgradeScripts(classNode, xmlFileData,typeToUpgrade,  persistedClassVersion, currentClassVersion);
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

        private Version GetPersistedClassVersion(XmlNode classNode)
        {
            if (classNode == null)
            {
                throw new ArgumentNullException(nameof(classNode));
            }
            
            string version = classNode.Attributes["version"]?.Value;
            return version == null 
                ? new Version("1.0.0") 
                : new Version(version);
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

        private Version GetCurrentClassVersion(XmlNode classNode, Type type)
        {
            var attribute = type.GetCustomAttribute(typeof(ClassMetaVersionAttribute)) as ClassMetaVersionAttribute;
            if (attribute == null)
            {
                throw new Exception($"Cannot get meta version of class {type.Name} because it does not have {nameof(ClassMetaVersionAttribute)} on it");
            }

            return attribute.Value;
        }
    }
}