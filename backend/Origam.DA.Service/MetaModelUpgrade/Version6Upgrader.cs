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
using System.Linq;
using System.Xml.Linq;
using Origam.DA.Service.NamespaceMapping;

namespace Origam.DA.Service.MetaModelUpgrade
{
    public class Version6Upgrader
    {
        private readonly ScriptContainerLocator scriptLocator;
        private readonly OrigamXDocument document;
        private static XNamespace oldPersistenceNamespace =
            "http://schemas.origam.com/1.0.0/model-persistence";
        private static XNamespace newPersistenceNamespace = OrigamFile.ModelPersistenceUri;

        public Version6Upgrader(ScriptContainerLocator scriptLocator, OrigamXDocument document)
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
                {
                    category = OrigamFile.PackageCategory;
                    break;
                }

                case "http://schemas.origam.com/5.0.0/model-elementgroup":
                {
                    category = OrigamFile.GroupCategory;
                    break;
                }

                default:
                    throw new Exception(
                        $"Cannot convert node {node} to meta model version 6.0.0 because {typeAttribute.Value} cannot be mapped to category"
                    );
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

        private static void CopyAttributes(
            XElement node,
            IPropertyToNamespaceMapping namespaceMapping
        )
        {
            List<XAttribute> atList = node.Attributes()
                .Where(attr => attr.Name.LocalName != "xmlns")
                .ToList();

            node.Attributes().Remove();
            foreach (XAttribute attribute in atList)
            {
                XNamespace nameSpace =
                    attribute.Name.Namespace == oldPersistenceNamespace
                        ? newPersistenceNamespace
                        : namespaceMapping.GetNamespaceByXmlAttributeName(attribute.Name.LocalName);
                node.Add(
                    new XAttribute(nameSpace.GetName(attribute.Name.LocalName), attribute.Value)
                );
            }
        }

        private IPropertyToNamespaceMapping GetNamespaceMapping(XElement node)
        {
            XName name = oldPersistenceNamespace.GetName("type");
            XAttribute typeAttribute = node?.Attribute(name);
            if (string.IsNullOrWhiteSpace(typeAttribute?.Value))
            {
                throw new Exception($"Cannot get type from node: {node?.Name} in \n{document}");
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
                        new Version(6, 0, 0)
                    );
                }
            }

            return Version6PropertyToNamespaceMapping.CreateOrGet(type, scriptLocator).DeepCopy();
        }

        private void RemoveTypeAttribute(XElement node)
        {
            XName name = oldPersistenceNamespace.GetName("type");
            XAttribute typeAttribute = node?.Attribute(name);
            typeAttribute?.Remove();
        }

        private void RemoveOldDocumentNamespaces()
        {
            document
                .FileElement.Attributes()
                .Where(attr =>
                    attr.Value == "http://schemas.origam.com/5.0.0/model-element"
                    || attr.Value == "http://schemas.origam.com/1.0.0/package"
                )
                .Remove();
            document.FileElement.Name = newPersistenceNamespace.GetName(
                document.FileElement.Name.LocalName
            );
            document.FileElement.Attribute(XNamespace.Xmlns + "x").Value =
                newPersistenceNamespace.ToString();
        }
    }
}
