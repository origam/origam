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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Origam.DA.Common;
using MoreLinq;
using Origam.Extensions;

namespace Origam.DA.Service.MetaModelUpgrade
{
    public abstract class UpgradeScriptContainer
    {
        protected readonly List<UpgradeScript> upgradeScripts = new List<UpgradeScript>();
        private PropertyToNamespaceMapping namespaceMapping;
        public abstract string FullTypeName { get;}
        public abstract List<string> OldFullTypeNames { get;}
        
        Version LastVersionInContainer => upgradeScripts
            .OrderBy(script => script.ToVersion)
            .Last()
            .ToVersion;

        private PropertyToNamespaceMapping NamespaceMapping
        {
            get
            {
                if (namespaceMapping == null)
                {
                    Type classType = Reflector.GetTypeByName(FullTypeName);
                    namespaceMapping = new PropertyToNamespaceMapping(classType);
                }
                return namespaceMapping;
            }
        }
        
        public void Upgrade(OrigamXmlDocument doc, XmlNode classNode, Version fromVersion, Version toVersion)
        {
            Version endVersion = toVersion == Versions.Last 
                ? LastVersionInContainer 
                : toVersion;
            var scriptsToRun = upgradeScripts
                .Where(script => script.FromVersion >= fromVersion && script.ToVersion <= endVersion)
                .OrderBy(script => script.FromVersion)
                .ToList();

            if (scriptsToRun.Count == 0)
            {
                throw new Exception($"There is no script to upgrade class {FullTypeName} from version {fromVersion} to {endVersion}");
            }
            if (scriptsToRun[0].FromVersion != fromVersion)
            {
                throw new Exception($"Script to upgrade class {FullTypeName} from version {fromVersion} to the next version was not found");
            }
            if (scriptsToRun.Last().ToVersion != endVersion)
            {
                throw new Exception($"Script to upgrade class {FullTypeName} to version {endVersion} was not found");
            }

            CheckScriptsFormContinuousSequence(scriptsToRun);

            foreach (var upgradeScript in scriptsToRun)
            {
                upgradeScript.Upgrade(classNode, doc);
            }
            SetVersion(doc, endVersion);
        }


        private void CheckScriptsFormContinuousSequence(List<UpgradeScript> scriptsToRun)
        {
            for (int i = 0; i < scriptsToRun.Count - 1; i++)
            {
                if (scriptsToRun[i].ToVersion != scriptsToRun[i + 1].FromVersion)
                {
                    throw new ClassUpgradeException(
                        $"There is no script to upgrade class {FullTypeName} from version {scriptsToRun[i].ToVersion} to {scriptsToRun[i + 1].FromVersion}");
                }
            }
        }

        private void SetVersion(OrigamXmlDocument document, Version toVersion)
        {
            var updatedNamespace = OrigamNameSpace
                .Create(FullTypeName, toVersion)
                .StringValue;
            string oldNamespace = GetThisClassNamespace(document);
            if (oldNamespace == null)
            {
                document.AddNamespace(  
                    namespaceMapping.NodeNamespaceName, 
                    namespaceMapping.NodeNamespace.ToString());
            }
            else
            {
                document.AddNamespaceForRenaming(oldNamespace, updatedNamespace);
            }
        }
        
        protected void AddAttribute(XmlNode node, string attributeName, string attributeValue)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            XmlElement element = (XmlElement) node;
            var thisTypeNamespace = 
                GetThisClassNamespace((OrigamXmlDocument) node.OwnerDocument)
                ?? NamespaceMapping.GetNamespaceByXmlAttributeName(attributeName);

            if (element.GetAttributeNode(attributeName,thisTypeNamespace.ToString() ) != null)
            {
                throw new ClassUpgradeException($"Cannot add new attribute \"{attributeName}\" because it already exist. Node:\n{node.OuterXml}");
            }
            element.SetAttribute(attributeName,thisTypeNamespace.ToString(), attributeValue);   
        }

        private string GetThisClassNamespace(OrigamXmlDocument document)
        {
            return document.Namespaces
                .FirstOrDefault(nameSpace => nameSpace.FullTypeName == FullTypeName)
                ?.StringValue;
        }
    }
    
    [DebuggerDisplay("Form: {FromVersion}, To: {ToVersion}")]
    public class UpgradeScript
    {
        public Version FromVersion { get; }
        public Version ToVersion { get;}

        private readonly Action<XmlNode, OrigamXmlDocument> transformation;

        public UpgradeScript(Version fromVersion, Version toVersion,
            Action<XmlNode, OrigamXmlDocument> transformation)
        {
            this.transformation = transformation;
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        public void Upgrade(XmlNode classNode, OrigamXmlDocument doc)
        {
            transformation(classNode, doc);
        } 
    }
}