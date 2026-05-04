// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.ObjectModel;

namespace Origam.Windows.Editor
{
    [Serializable()]
    public class XmlSchemaCompletionCollection : Collection<XmlSchemaCompletion>
    {
        public XmlSchemaCompletionCollection() { }

        public XmlSchemaCompletionCollection(XmlSchemaCompletionCollection schemas)
        {
            AddRange(schemas: schemas);
        }

        public XmlSchemaCompletionCollection(XmlSchemaCompletion[] schemas)
        {
            AddRange(schema: schemas);
        }

        public XmlCompletionItemCollection GetNamespaceCompletion(string textUpToCursor)
        {
            string attrName = XmlParser.GetAttributeNameAtIndex(
                xml: textUpToCursor,
                index: textUpToCursor.Length
            );
            if (attrName == "xmlns" || attrName.StartsWith(value: "xmlns:"))
            {
                return GetNamespaceCompletion();
            }
            return new XmlCompletionItemCollection();
        }

        public XmlCompletionItemCollection GetNamespaceCompletion()
        {
            XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();

            foreach (XmlSchemaCompletion schema in this)
            {
                XmlCompletionItem completionItem = new XmlCompletionItem(
                    text: schema.NamespaceUri,
                    dataType: XmlCompletionItemType.NamespaceUri
                );
                if (!completionItems.Contains(item: completionItem))
                {
                    completionItems.Add(item: completionItem);
                }
            }

            return completionItems;
        }

        /// <summary>
        ///   Represents the <see cref='XmlSchemaCompletionData'/> entry with the specified namespace URI.
        /// </summary>
        /// <param name='namespaceUri'>The schema's namespace URI.</param>
        /// <value>The entry with the specified namespace URI.</value>
        public XmlSchemaCompletion this[string namespaceUri]
        {
            get { return GetItem(namespaceUri: namespaceUri); }
        }

        public bool Contains(string namespaceUri)
        {
            return GetItem(namespaceUri: namespaceUri) != null;
        }

        XmlSchemaCompletion GetItem(string namespaceUri)
        {
            foreach (XmlSchemaCompletion item in this)
            {
                if (item.NamespaceUri == namespaceUri)
                {
                    return item;
                }
            }
            return null;
        }

        public void AddRange(XmlSchemaCompletion[] schema)
        {
            for (int i = 0; i < schema.Length; i++)
            {
                Add(item: schema[i]);
            }
        }

        public void AddRange(XmlSchemaCompletionCollection schemas)
        {
            for (int i = 0; i < schemas.Count; i++)
            {
                Add(item: schemas[index: i]);
            }
        }

        public XmlSchemaCompletionCollection GetSchemas(string namespaceUri)
        {
            XmlSchemaCompletionCollection schemas = new XmlSchemaCompletionCollection();
            foreach (XmlSchemaCompletion schema in this)
            {
                if (schema.NamespaceUri == namespaceUri)
                {
                    schemas.Add(item: schema);
                }
            }
            return schemas;
        }

        public XmlSchemaCompletionCollection GetSchemas(
            XmlElementPath path,
            XmlSchemaCompletion defaultSchema
        )
        {
            string namespaceUri = path.GetRootNamespace();
            if (String.IsNullOrEmpty(value: namespaceUri))
            {
                return GetSchemaCollectionUsingDefaultSchema(
                    path: path,
                    defaultSchema: defaultSchema
                );
            }
            return GetSchemas(namespaceUri: namespaceUri);
        }

        XmlSchemaCompletionCollection GetSchemaCollectionUsingDefaultSchema(
            XmlElementPath path,
            XmlSchemaCompletion defaultSchema
        )
        {
            XmlSchemaCompletionCollection schemas = new XmlSchemaCompletionCollection();
            if (defaultSchema != null)
            {
                path.SetNamespaceForUnqualifiedNames(namespaceUri: defaultSchema.NamespaceUri);
                schemas.Add(item: defaultSchema);
            }
            return schemas;
        }

        public XmlCompletionItemCollection GetChildElementCompletion(
            XmlElementPath path,
            XmlSchemaCompletion defaultSchema
        )
        {
            XmlCompletionItemCollection items = new XmlCompletionItemCollection();
            foreach (
                XmlSchemaCompletion schema in GetSchemas(path: path, defaultSchema: defaultSchema)
            )
            {
                items.AddRange(item: schema.GetChildElementCompletion(path: path));
            }
            return items;
        }

        public XmlCompletionItemCollection GetElementCompletionForAllNamespaces(
            XmlElementPath path,
            XmlSchemaCompletion defaultSchema
        )
        {
            XmlElementPathsByNamespace pathsByNamespace = new XmlElementPathsByNamespace(
                path: path
            );
            return GetElementCompletion(
                pathsByNamespace: pathsByNamespace,
                defaultSchema: defaultSchema
            );
        }

        public XmlCompletionItemCollection GetElementCompletion(
            XmlElementPathsByNamespace pathsByNamespace,
            XmlSchemaCompletion defaultSchema
        )
        {
            XmlCompletionItemCollection items = new XmlCompletionItemCollection();
            foreach (XmlElementPath path in pathsByNamespace)
            {
                items.AddRange(
                    item: GetChildElementCompletion(path: path, defaultSchema: defaultSchema)
                );
            }

            XmlNamespaceCollection namespaceWithoutPaths = pathsByNamespace.NamespacesWithoutPaths;
            if (items.Count == 0)
            {
                if (
                    !IsDefaultSchemaNamespaceDefinedInPathsByNamespace(
                        namespaces: namespaceWithoutPaths,
                        defaultSchema: defaultSchema
                    )
                )
                {
                    namespaceWithoutPaths.Add(item: defaultSchema.Namespace);
                }
            }
            items.AddRange(item: GetRootElementCompletion(namespaces: namespaceWithoutPaths));
            return items;
        }

        bool IsDefaultSchemaNamespaceDefinedInPathsByNamespace(
            XmlNamespaceCollection namespaces,
            XmlSchemaCompletion defaultSchema
        )
        {
            if (defaultSchema != null)
            {
                return namespaces.Contains(item: defaultSchema.Namespace);
            }
            return true;
        }

        public XmlCompletionItemCollection GetRootElementCompletion(
            XmlNamespaceCollection namespaces
        )
        {
            XmlCompletionItemCollection items = new XmlCompletionItemCollection();
            foreach (XmlNamespace ns in namespaces)
            {
                foreach (XmlSchemaCompletion schema in GetSchemas(namespaceUri: ns.Name))
                {
                    items.AddRange(
                        item: schema.GetRootElementCompletion(namespacePrefix: ns.Prefix)
                    );
                }
            }
            return items;
        }

        public XmlCompletionItemCollection GetAttributeCompletion(
            XmlElementPath path,
            XmlSchemaCompletion defaultSchema
        )
        {
            XmlCompletionItemCollection items = new XmlCompletionItemCollection();
            foreach (
                XmlSchemaCompletion schema in GetSchemas(path: path, defaultSchema: defaultSchema)
            )
            {
                items.AddRange(item: schema.GetAttributeCompletion(path: path));
            }
            return items;
        }

        public XmlCompletionItemCollection GetElementCompletion(
            string textUpToCursor,
            XmlSchemaCompletion defaultSchema
        )
        {
            XmlElementPath parentPath = XmlParser.GetParentElementPath(xml: textUpToCursor);
            return GetElementCompletionForAllNamespaces(
                path: parentPath,
                defaultSchema: defaultSchema
            );
        }

        public XmlCompletionItemCollection GetAttributeCompletion(
            string textUpToCursor,
            XmlSchemaCompletion defaultSchema
        )
        {
            if (
                !XmlParser.IsInsideAttributeValue(xml: textUpToCursor, index: textUpToCursor.Length)
            )
            {
                XmlElementPath path = XmlParser.GetActiveElementStartPath(
                    xml: textUpToCursor,
                    index: textUpToCursor.Length
                );
                path.Compact();
                if (path.Elements.HasItems)
                {
                    return GetAttributeCompletion(path: path, defaultSchema: defaultSchema);
                }
            }
            return new XmlCompletionItemCollection();
        }

        public XmlCompletionItemCollection GetAttributeValueCompletion(
            char charTyped,
            string textUpToCursor,
            XmlSchemaCompletion defaultSchema
        )
        {
            if (XmlParser.IsAttributeValueChar(ch: charTyped))
            {
                string attributeName = XmlParser.GetAttributeName(
                    xml: textUpToCursor,
                    index: textUpToCursor.Length
                );
                if (attributeName.Length > 0)
                {
                    XmlElementPath elementPath = XmlParser.GetActiveElementStartPath(
                        xml: textUpToCursor,
                        index: textUpToCursor.Length
                    );
                    return GetAttributeValueCompletion(
                        path: elementPath,
                        attributeName: attributeName,
                        defaultSchema: defaultSchema
                    );
                }
            }
            return new XmlCompletionItemCollection();
        }

        public XmlCompletionItemCollection GetAttributeValueCompletion(
            string text,
            int offset,
            XmlSchemaCompletion defaultSchema
        )
        {
            if (XmlParser.IsInsideAttributeValue(xml: text, index: offset))
            {
                XmlElementPath path = XmlParser.GetActiveElementStartPath(xml: text, index: offset);
                string attributeName = XmlParser.GetAttributeNameAtIndex(xml: text, index: offset);
                return GetAttributeValueCompletion(
                    path: path,
                    attributeName: attributeName,
                    defaultSchema: defaultSchema
                );
            }
            return new XmlCompletionItemCollection();
        }

        public XmlCompletionItemCollection GetAttributeValueCompletion(
            XmlElementPath path,
            string attributeName,
            XmlSchemaCompletion defaultSchema
        )
        {
            path.Compact();
            XmlCompletionItemCollection items = new XmlCompletionItemCollection();
            foreach (
                XmlSchemaCompletion schema in GetSchemas(path: path, defaultSchema: defaultSchema)
            )
            {
                items.AddRange(
                    item: schema.GetAttributeValueCompletion(
                        path: path,
                        attributeName: attributeName
                    )
                );
            }
            return items;
        }
    }
}
