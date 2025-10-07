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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service.NamespaceMapping;
using Origam.Extensions;

namespace Origam.DA.Service
{
    public class InstanceWriter
    {
        private readonly IExternalFileManager externalFileManger;
        private readonly OrigamXmlDocument xmlDocument;

        public InstanceWriter(
            IExternalFileManager externalFileManger,
            OrigamXmlDocument xmlDocument
        )
        {
            this.externalFileManger = externalFileManger;
            this.xmlDocument = xmlDocument;
        }

        public void Write(IFilePersistent instance)
        {
            var namespaceMapping = PropertyToNamespaceMapping.Get(instance.GetType()).DeepCopy();
            namespaceMapping.AddNamespacesToDocumentAndAdjustMappings(xmlDocument);
            xmlDocument.UseTopNamespacePrefixesEverywhere();

            XmlElement elementToWriteTo = GetElementToWriteTo(instance, namespaceMapping);
            bool isLocalChild = elementToWriteTo.ParentNode.GetDepth() != 1;
            WriteToNode(elementToWriteTo, instance, namespaceMapping, isLocalChild);
        }

        private XmlElement GetElementToWriteTo(
            IFilePersistent instance,
            PropertyToNamespaceMapping namespaceMapping
        )
        {
            XmlElement existingElement = (XmlElement)
                xmlDocument.GetAllNodes().FirstOrDefault(x => XmlUtils.ReadId(x) == instance.Id);
            if (existingElement == null)
            {
                return FindElementToWriteTo(xmlDocument, instance, 0, namespaceMapping);
            }

            Guid? parentNodeId = XmlUtils.ReadId(existingElement.ParentNode);
            bool parentNodeIdInXmlDiffersFromParentIdInInstance =
                parentNodeId.HasValue && parentNodeId.Value != instance.FileParentId
                || !parentNodeId.HasValue && instance.FileParentId != Guid.Empty;
            if (parentNodeIdInXmlDiffersFromParentIdInInstance)
            {
                MoveElementToNewLocation(existingElement, instance, namespaceMapping);
            }
            return existingElement;
        }

        private void MoveElementToNewLocation(
            XmlElement element,
            IFilePersistent instance,
            PropertyToNamespaceMapping namespaceMapping
        )
        {
            element.ParentNode?.RemoveChild(element);
            XmlElement newElement = FindElementToWriteTo(
                xmlDocument,
                instance,
                0,
                namespaceMapping
            );
            XmlNode parentNode = newElement.ParentNode;
            parentNode.RemoveChild(newElement);
            parentNode.AppendChild(element);
        }

        private XmlElement FindElementToWriteTo(
            XmlNode node,
            IFilePersistent instance,
            int depth,
            PropertyToNamespaceMapping namespaceMapping
        )
        {
            Guid? parentId = XmlUtils.ReadId(node);
            foreach (XmlNode child in node.ChildNodes)
            {
                XmlElement element = child as XmlElement;
                Guid? id = XmlUtils.ReadId(child);
                if (element != null && id.HasValue && id.Value == instance.Id)
                {
                    return element;
                }
                else
                {
                    XmlElement foundEl = FindElementToWriteTo(
                        child,
                        instance,
                        depth + 1,
                        namespaceMapping
                    );
                    if (foundEl != null)
                        return foundEl;
                }
            }
            if ((parentId.HasValue && parentId.Value == instance.FileParentId) || depth == 0)
            {
                if (depth == 0)
                {
                    node = node.LastChild;
                }
                // node does not exist, we add
                string category = CategoryFactory.Create(instance.GetType());
                XmlElement element = node.OwnerDocument.CreateElement(
                    namespaceMapping.NodeNamespaceName,
                    category,
                    namespaceMapping.NodeNamespace.ToString()
                );
                node.AppendChild(element);
                return element;
            }
            return null;
        }

        private void WriteToNode(
            XmlElement node,
            IFilePersistent instance,
            PropertyToNamespaceMapping namespaceMapping,
            bool localChild
        )
        {
            node.RemoveAllAttributes();
            // Set all the remaining properties
            WriteXmlAttributes(node, instance, namespaceMapping);
            // references
            WriteXmlReferenceAttributes(node, instance, namespaceMapping);
            WriteXmlExternalFiles(node, instance, namespaceMapping);
            // persistence attributes
            node.SetAttribute(
                OrigamFile.IdAttribute,
                OrigamFile.ModelPersistenceUri.ToString(),
                instance.PrimaryKey["Id"].ToString()
            );
            if (instance.IsFolder)
            {
                node.SetAttribute(
                    OrigamFile.IsFolderAttribute,
                    OrigamFile.ModelPersistenceUri.ToString(),
                    XmlConvert.ToString(instance.IsFolder)
                );
            }
            if (instance.FileParentId != Guid.Empty && !localChild)
            {
                node.SetAttribute(
                    OrigamFile.ParentIdAttribute,
                    OrigamFile.ModelPersistenceUri,
                    instance.FileParentId.ToString()
                );
            }
        }

        private static void WriteXmlReferenceAttributes(
            XmlElement node,
            IFilePersistent instance,
            PropertyToNamespaceMapping namespaceMapping
        )
        {
            List<MemberAttributeInfo> references = Reflector.FindMembers(
                instance.GetType(),
                typeof(XmlReferenceAttribute)
            );
            foreach (MemberAttributeInfo mi in references)
            {
                XmlReferenceAttribute attribute = mi.Attribute as XmlReferenceAttribute;
                PropertyInfo mpi = mi.MemberInfo as PropertyInfo;
                FieldInfo mfi = mi.MemberInfo as FieldInfo;
                object value = null;
                if (mpi != null)
                {
                    value = mpi.GetValue(instance);
                }
                else if (mfi != null)
                {
                    value = mfi.GetValue(instance);
                }
                IFilePersistent persistentValue = value as IFilePersistent;
                if (persistentValue == null && value != null)
                {
                    throw new Exception(
                        $"Reference must be {typeof(IFilePersistent)} interface ({mi.MemberInfo.Name})"
                    );
                }
                if (persistentValue != null)
                {
                    string subPath = persistentValue.Path ?? "";
                    if (subPath != "")
                    {
                        subPath += "/";
                    }

                    string path =
                        persistentValue.RelativeFilePath.Replace("\\", "/")
                        + "#"
                        + subPath
                        + persistentValue.PrimaryKey["Id"];
                    node.SetAttribute(
                        localName: attribute.AttributeName,
                        namespaceURI: namespaceMapping.GetNamespaceByPropertyName(
                            mi.MemberInfo.Name
                        ),
                        value: path
                    );
                }
            }
        }

        private void WriteXmlAttributes(
            XmlElement node,
            IFilePersistent instance,
            PropertyToNamespaceMapping namespaceMapping
        )
        {
            List<MemberAttributeInfo> members = Reflector.FindMembers(
                instance.GetType(),
                typeof(XmlAttributeAttribute)
            );
            foreach (MemberAttributeInfo memberInfo in members)
            {
                XmlAttributeAttribute attribute = (XmlAttributeAttribute)memberInfo.Attribute;
                object value = GetValueToWrite(instance, memberInfo);

                if (ShouldBeSkipped(value))
                    continue;
                if (Guid.Empty.Equals(value))
                    continue;
                node.SetAttribute(
                    localName: attribute.AttributeName,
                    namespaceURI: namespaceMapping.GetNamespaceByPropertyName(
                        memberInfo.MemberInfo.Name
                    ),
                    value: XmlTools.ConvertToString(value)
                );
            }
        }

        private bool ShouldBeSkipped(object value)
        {
            if (ReferenceEquals(value, null))
                return true;
            if (value is Enum)
                return false;
            if (value is bool)
                return false;
            if (value is string strValue)
                return string.IsNullOrEmpty(strValue);
            return value.IsDefault();
        }

        private void WriteXmlExternalFiles(
            XmlElement node,
            IFilePersistent instance,
            PropertyToNamespaceMapping namespaceMapping
        )
        {
            List<MemberAttributeInfo> references = Reflector.FindMembers(
                instance.GetType(),
                typeof(XmlExternalFileReference)
            );
            foreach (MemberAttributeInfo mi in references)
            {
                var attribute = (XmlExternalFileReference)mi.Attribute;

                MemberInfo containerMemberInfo = instance
                    .GetType()
                    .GetField(
                        attribute.ContainerName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    );

                if (containerMemberInfo == null)
                {
                    throw new Exception(
                        $"Could not find field {attribute.ContainerName} in Class {instance.GetType()}"
                    );
                }

                object containerObj = ((FieldInfo)containerMemberInfo).GetValue(instance);

                if (!(containerObj is IPropertyContainer))
                {
                    throw new Exception(
                        $"Could not find field \"value\" in field {attribute.ContainerName} of class {instance.GetType()}. Make sure that the field {attribute.ContainerName} exists and is of type {typeof(PropertyContainer<>)}"
                    );
                }
                IPropertyContainer container = (IPropertyContainer)containerObj;

                string externalFileLink = externalFileManger.AddAndReturnLink(
                    fieldName: attribute.ContainerName,
                    objectId: (Guid)instance.PrimaryKey["Id"],
                    data: container.GetValue(),
                    fileExtension: attribute.Extension
                );

                node.SetAttribute(
                    localName: attribute.ContainerName,
                    namespaceURI: namespaceMapping.GetNamespaceByPropertyName(mi.MemberInfo.Name),
                    value: externalFileLink
                );
            }
        }

        private static object GetValueToWrite(IFilePersistent instance, MemberAttributeInfo mi)
        {
            PropertyInfo propertyInfo = mi.MemberInfo as PropertyInfo;
            FieldInfo fieldInfo = mi.MemberInfo as FieldInfo;
            object value = null;
            if (propertyInfo != null)
            {
                value = propertyInfo.GetValue(instance);
            }
            else if (fieldInfo != null)
            {
                value = fieldInfo.GetValue(instance);
            }
            return value;
        }
    }
}
