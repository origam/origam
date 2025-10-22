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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using MoreLinq;
using Origam.DA.Common;
using Origam.DA.Service.NamespaceMapping;
using Origam.Extensions;

namespace Origam.DA.Service.MetaModelUpgrade
{
    public abstract class UpgradeScriptContainer
    {
        protected readonly List<UpgradeScript> upgradeScripts = new List<UpgradeScript>();
        private PropertyToNamespaceMapping namespaceMapping;
        public abstract string FullTypeName { get; }
        public abstract List<string> OldFullTypeNames { get; }
        public abstract string[] OldPropertyXmlNames { get; }

        Version LastVersionInContainer =>
            upgradeScripts.OrderBy(script => script.ToVersion).Last().ToVersion;
        Version FirstVersionInContainer =>
            upgradeScripts.OrderBy(script => script.FromVersion).First().FromVersion;

        private PropertyToNamespaceMapping NamespaceMapping
        {
            get
            {
                if (namespaceMapping == null)
                {
                    Type classType = Reflector.GetTypeByName(FullTypeName);
                    namespaceMapping = PropertyToNamespaceMapping.Get(classType);
                }
                return namespaceMapping;
            }
        }

        public bool Upgrade(
            DocumentContainer documentContainer,
            XElement classNode,
            Version fromVersion,
            Version toVersion
        )
        {
            Version endVersion = toVersion == Versions.Last ? LastVersionInContainer : toVersion;
            var scriptsToRun = upgradeScripts
                .Where(script =>
                    script.FromVersion >= fromVersion && script.ToVersion <= endVersion
                )
                .OrderBy(script => script.FromVersion)
                .ToList();

            if (scriptsToRun.Count == 0)
            {
                throw new Exception(
                    $"There is no script to upgrade class {FullTypeName} from version {fromVersion} to {endVersion}"
                );
            }
            if (scriptsToRun[0].FromVersion != fromVersion)
            {
                if (
                    fromVersion == ClassMetaVersionAttribute.FirstVersion
                    && FirstVersionInContainer == ClassMetaVersionAttribute.FormerFirstVersion
                )
                {
                    // The first version used to be 6.0.0, the script container was probably created with that assumption.
                    return Upgrade(
                        documentContainer,
                        classNode,
                        ClassMetaVersionAttribute.FormerFirstVersion,
                        toVersion
                    );
                }

                throw new Exception(
                    $"Script to upgrade class {FullTypeName} from version {fromVersion} to the next version was not found"
                );
            }
            if (scriptsToRun.Last().ToVersion != endVersion)
            {
                throw new Exception(
                    $"Script to upgrade class {FullTypeName} to version {endVersion} was not found"
                );
            }

            CheckScriptsFormContinuousSequence(scriptsToRun);

            foreach (var upgradeScript in scriptsToRun)
            {
                upgradeScript.Upgrade(classNode, documentContainer.Document);
            }
            documentContainer.ScheduleNamespaceUpgrade(this, endVersion);
            return true;
        }

        private void CheckScriptsFormContinuousSequence(List<UpgradeScript> scriptsToRun)
        {
            for (int i = 0; i < scriptsToRun.Count - 1; i++)
            {
                if (scriptsToRun[i].ToVersion != scriptsToRun[i + 1].FromVersion)
                {
                    throw new ClassUpgradeException(
                        $"There is no script to upgrade class {FullTypeName} from version {scriptsToRun[i].ToVersion} to {scriptsToRun[i + 1].FromVersion}"
                    );
                }
            }
        }

        public void SetVersion(OrigamXDocument document, Version toVersion)
        {
            XNamespace updatedNamespace = OrigamNameSpace
                .CreateOrGet(FullTypeName, toVersion)
                .StringValue;
            XNamespace oldNamespace = GetThisClassNamespace(document.XDocument);
            if (oldNamespace == null)
            {
                document.AddNamespace(
                    namespaceMapping.NodeNamespaceName,
                    namespaceMapping.NodeNamespace.ToString()
                );
            }
            else
            {
                document.RenameNamespace(oldNamespace, updatedNamespace);
            }
        }

        protected void AddAttribute(XElement node, string attributeName, string attributeValue)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            XNamespace thisTypeNamespace =
                GetThisClassNamespace(node.Document)
                ?? NamespaceMapping.GetNamespaceByXmlAttributeName(attributeName);

            XName xName = thisTypeNamespace.GetName(attributeName);
            if (node.Attribute(xName) != null)
            {
                throw new ClassUpgradeException(
                    $"Cannot add new attribute \"{attributeName}\" because it already exist. Node:\n{node}"
                );
            }
            node.Add(new XAttribute(xName, attributeValue));
        }

        protected void RemoveAttribute(XElement node, string attributeName)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            XNamespace thisTypeNamespace = GetThisClassNamespace(node.Document);
            node.Attribute(thisTypeNamespace + attributeName)?.Remove();
        }

        protected XNamespace GetPersistenceNamespace(XElement element)
        {
            return element.Attributes().First(attr => attr.Name.LocalName == "id").Name.Namespace;
        }

        protected XNamespace GetAbstractSchemaItemNamespace(XElement element)
        {
            return element.Attributes().First(attr => attr.Name.LocalName == "name").Name.Namespace;
        }

        private XNamespace GetThisClassNamespace(XDocument document)
        {
            return OrigamXDocument
                .GetNamespaces(document)
                .FirstOrDefault(nameSpace =>
                    nameSpace.FullTypeName == FullTypeName
                    || (
                        OldFullTypeNames != null
                        && OldFullTypeNames.Contains(nameSpace.FullTypeName)
                    )
                )
                ?.StringValue;
        }

        internal void AddEmptyUpgrade(string fromVersion, string toVersion)
        {
            upgradeScripts.Add(
                new UpgradeScript(new Version(fromVersion), new Version(toVersion), EmptyUpgrade())
            );
        }

        internal static Action<XElement, OrigamXDocument> EmptyUpgrade()
        {
            return (node, doc) => { };
        }
    }

    [DebuggerDisplay("Form: {FromVersion}, To: {ToVersion}")]
    public class UpgradeScript
    {
        public Version FromVersion { get; }
        public Version ToVersion { get; }

        private readonly Action<XElement, OrigamXDocument> transformation;

        public UpgradeScript(
            Version fromVersion,
            Version toVersion,
            Action<XElement, OrigamXDocument> transformation
        )
        {
            this.transformation = transformation;
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        public void Upgrade(XElement classNode, OrigamXDocument doc)
        {
            transformation(classNode, doc);
        }
    }
}
