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
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace Origam.Windows.Editor;

/// <summary>
/// Holds the completion (intellisense) data for an xml schema.
/// </summary>
/// <remarks>
/// The XmlSchema class throws an exception if we attempt to load
/// the xhtml1-strict.xsd schema.  It does not like the fact that
/// this schema redefines the xml namespace, even though this is
/// allowed by the w3.org specification.
/// </remarks>
public class XmlSchemaCompletion
{
    XmlSchema schema;
    bool readOnly;
    XmlNamespace xmlNamespace = new XmlNamespace();

    /// <summary>
    /// Stores attributes that have been prohibited whilst the code
    /// generates the attribute completion data.
    /// </summary>
    XmlSchemaObjectCollection prohibitedAttributes = new XmlSchemaObjectCollection();

    public XmlSchemaCompletion() { }

    public string DefaultNamespacePrefix
    {
        get { return xmlNamespace.Prefix; }
        set { xmlNamespace.Prefix = value; }
    }

    /// <summary>
    /// Creates completion data from the schema passed in
    /// via the reader object.
    /// </summary>
    public XmlSchemaCompletion(TextReader reader)
    {
        ReadSchema(baseUri: String.Empty, reader: reader);
    }

    /// <summary>
    /// Gets the schema.
    /// </summary>
    public XmlSchema Schema
    {
        get { return schema; }
    }

    /// <summary>
    /// Read only schemas are those that are installed with
    /// SharpDevelop.
    /// </summary>
    public bool IsReadOnly
    {
        get { return readOnly; }
        set { readOnly = value; }
    }

    /// <summary>
    /// Gets the namespace URI for the schema.
    /// </summary>
    public string NamespaceUri
    {
        get { return xmlNamespace.Name; }
    }
    public bool HasNamespaceUri
    {
        get { return !String.IsNullOrWhiteSpace(value: NamespaceUri); }
    }
    public XmlNamespace Namespace
    {
        get { return xmlNamespace; }
    }

    /// <summary>
    /// Converts the filename into a valid Uri.
    /// </summary>
    public static string GetUri(string fileName)
    {
        if (fileName != null)
        {
            if (fileName.Length > 0)
            {
                return String.Concat(
                    str0: "file:///",
                    str1: fileName.Replace(oldChar: '\\', newChar: '/')
                );
            }
        }
        return String.Empty;
    }

    public XmlCompletionItemCollection GetRootElementCompletion()
    {
        return GetRootElementCompletion(namespacePrefix: DefaultNamespacePrefix);
    }

    public XmlCompletionItemCollection GetRootElementCompletion(string namespacePrefix)
    {
        XmlCompletionItemCollection items = new XmlCompletionItemCollection();
        foreach (XmlSchemaElement element in schema.Elements.Values)
        {
            if (element.Name != null)
            {
                AddElement(
                    completionItems: items,
                    name: element.Name,
                    prefix: namespacePrefix,
                    annotation: element.Annotation
                );
            }
            // Do not add reference element.
        }
        return items;
    }

    /// <summary>
    /// Gets the attribute completion data for the xml element that exists
    /// at the end of the specified path.
    /// </summary>
    public XmlCompletionItemCollection GetAttributeCompletion(XmlElementPath path)
    {
        // Locate matching element.
        XmlSchemaElement element = FindElement(path: path);
        // Get completion data.
        if (element != null)
        {
            prohibitedAttributes.Clear();
            return GetAttributeCompletion(
                element: element,
                namespacesInScope: path.NamespacesInScope
            );
        }
        return new XmlCompletionItemCollection();
    }

    /// <summary>
    /// Gets the child element completion data for the xml element that exists
    /// at the end of the specified path.
    /// </summary>
    public XmlCompletionItemCollection GetChildElementCompletion(XmlElementPath path)
    {
        XmlSchemaElement element = FindElement(path: path);
        if (element != null)
        {
            return GetChildElementCompletion(
                element: element,
                prefix: path.Elements.GetLastPrefix()
            );
        }
        return new XmlCompletionItemCollection();
    }

    public XmlCompletionItemCollection GetAttributeValueCompletion(
        XmlElementPath path,
        string attributeName
    )
    {
        XmlSchemaElement element = FindElement(path: path);
        if (element != null)
        {
            return GetAttributeValueCompletion(element: element, name: attributeName);
        }
        return new XmlCompletionItemCollection();
    }

    /// <summary>
    /// Finds the element that exists at the specified path.
    /// </summary>
    /// <remarks>This method is not used when generating completion data,
    /// but is a useful method when locating an element so we can jump
    /// to its schema definition.</remarks>
    /// <returns><see langword="null"/> if no element can be found.</returns>
    public XmlSchemaElement FindElement(XmlElementPath path)
    {
        XmlSchemaElement element = null;
        for (int i = 0; i < path.Elements.Count; ++i)
        {
            QualifiedName name = path.Elements[index: i];
            if (i == 0)
            {
                // Look for root element.
                element = FindRootElement(name: name);
                if (element == null)
                {
                    return null;
                }
            }
            else
            {
                element = FindChildElement(element: element, name: name);
                if (element == null)
                {
                    return null;
                }
            }
        }
        return element;
    }

    /// <summary>
    /// Finds an element in the schema.
    /// </summary>
    /// <remarks>
    /// Only looks at the elements that are defined in the
    /// root of the schema so it will not find any elements
    /// that are defined inside any complex types.
    /// </remarks>
    public XmlSchemaElement FindRootElement(QualifiedName name)
    {
        foreach (XmlSchemaElement element in schema.Elements.Values)
        {
            if (name.Equals(obj: element.QualifiedName))
            {
                return element;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the complex type with the specified name.
    /// </summary>
    public XmlSchemaComplexType FindComplexType(QualifiedName name)
    {
        XmlQualifiedName qualifiedName = new XmlQualifiedName(name: name.Name, ns: name.Namespace);
        return FindNamedType(schema: schema, name: qualifiedName);
    }

    /// <summary>
    /// Finds the specified attribute name given the element.
    /// </summary>
    /// <remarks>This method is not used when generating completion data,
    /// but is a useful method when locating an attribute so we can jump
    /// to its schema definition.</remarks>
    /// <returns><see langword="null"/> if no attribute can be found.</returns>
    public XmlSchemaAttribute FindAttribute(XmlSchemaElement element, string name)
    {
        XmlSchemaComplexType complexType = GetElementAsComplexType(element: element);
        if (complexType != null)
        {
            return FindAttribute(complexType: complexType, name: name);
        }
        return null;
    }

    /// <summary>
    /// Finds the attribute group with the specified name.
    /// </summary>
    public XmlSchemaAttributeGroup FindAttributeGroup(string name)
    {
        return FindAttributeGroup(schema: schema, name: name);
    }

    /// <summary>
    /// Finds the simple type with the specified name.
    /// </summary>
    public XmlSchemaSimpleType FindSimpleType(string name)
    {
        XmlQualifiedName qualifiedName = new XmlQualifiedName(name: name, ns: xmlNamespace.Name);
        return FindSimpleType(name: qualifiedName);
    }

    /// <summary>
    /// Finds the specified attribute in the schema. This method only checks
    /// the attributes defined in the root of the schema.
    /// </summary>
    public XmlSchemaAttribute FindAttribute(string name)
    {
        foreach (XmlSchemaAttribute attribute in schema.Attributes.Values)
        {
            if (attribute.Name == name)
            {
                return attribute;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the schema group with the specified name.
    /// </summary>
    public XmlSchemaGroup FindGroup(string name)
    {
        if (name != null)
        {
            foreach (XmlSchemaObject schemaObject in schema.Groups.Values)
            {
                XmlSchemaGroup schemaGroup = schemaObject as XmlSchemaGroup;
                if (schemaGroup != null)
                {
                    if (schemaGroup.Name == name)
                    {
                        return schemaGroup;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Takes the name and creates a qualified name using the namespace of this
    /// schema.
    /// </summary>
    /// <remarks>If the name is of the form myprefix:mytype then the correct
    /// namespace is determined from the prefix. If the name is not of this
    /// form then no prefix is added.</remarks>
    public QualifiedName CreateQualifiedName(string name)
    {
        QualifiedName qualifiedName = QualifiedName.FromString(name: name);
        if (qualifiedName.HasPrefix)
        {
            foreach (XmlQualifiedName xmlQualifiedName in schema.Namespaces.ToArray())
            {
                if (xmlQualifiedName.Name == qualifiedName.Prefix)
                {
                    qualifiedName.Namespace = xmlQualifiedName.Namespace;
                    return qualifiedName;
                }
            }
        }
        // Default behaviour just return the name with the namespace uri.
        qualifiedName.Namespace = xmlNamespace.Name;
        return qualifiedName;
    }

    /// <summary>
    /// Converts the element to a complex type if possible.
    /// </summary>
    public XmlSchemaComplexType GetElementAsComplexType(XmlSchemaElement element)
    {
        XmlSchemaComplexType complexType = element.SchemaType as XmlSchemaComplexType;
        if (complexType == null)
        {
            if (element.SchemaTypeName.IsEmpty)
            {
                return GetComplexTypeFromSubstitutionGroup(element: element);
            }
            return FindNamedType(schema: schema, name: element.SchemaTypeName);
        }
        return complexType;
    }

    XmlSchemaComplexType GetComplexTypeFromSubstitutionGroup(XmlSchemaElement element)
    {
        if (!element.SubstitutionGroup.IsEmpty)
        {
            XmlSchemaElement substitutedElement = FindElement(name: element.SubstitutionGroup);
            if (substitutedElement != null)
            {
                return GetElementAsComplexType(element: substitutedElement);
            }
        }
        return null;
    }

    /// <summary>
    /// Handler for schema validation errors.
    /// </summary>
    void SchemaValidation(object source, ValidationEventArgs e)
    {
        // Do nothing.
    }

    /// <summary>
    /// Loads the schema.
    /// </summary>
    void ReadSchema(XmlReader reader)
    {
        try
        {
            schema = XmlSchema.Read(reader: reader, validationEventHandler: SchemaValidation);
            if (schema != null)
            {
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.ValidationEventHandler += SchemaValidation;
                schemaSet.Add(schema: schema);
                schemaSet.Compile();
                xmlNamespace.Name = schema.TargetNamespace;
            }
        }
        finally
        {
            reader.Close();
        }
    }

    void ReadSchema(string baseUri, TextReader reader)
    {
        XmlTextReader xmlReader = new XmlTextReader(url: baseUri, input: reader);
        // Setting the resolver to null allows us to
        // load the xhtml1-strict.xsd without any exceptions if
        // the referenced dtds exist in the same folder as the .xsd
        // file.  If this is not set to null the dtd files are looked
        // for in the assembly's folder.
        xmlReader.XmlResolver = null;
        ReadSchema(reader: xmlReader);
    }

    /// <summary>
    /// Finds an element in the schema.
    /// </summary>
    /// <remarks>
    /// Only looks at the elements that are defined in the
    /// root of the schema so it will not find any elements
    /// that are defined inside any complex types.
    /// </remarks>
    XmlSchemaElement FindElement(XmlQualifiedName name)
    {
        foreach (XmlSchemaElement element in schema.Elements.Values)
        {
            if (name.Equals(other: element.QualifiedName))
            {
                return element;
            }
        }
        return null;
    }

    XmlCompletionItemCollection GetChildElementCompletion(XmlSchemaElement element, string prefix)
    {
        XmlSchemaComplexType complexType = GetElementAsComplexType(element: element);
        if (complexType != null)
        {
            return GetChildElementCompletion(complexType: complexType, prefix: prefix);
        }
        return new XmlCompletionItemCollection();
    }

    XmlCompletionItemCollection GetChildElementCompletion(
        XmlSchemaComplexType complexType,
        string prefix
    )
    {
        XmlSchemaSequence sequence = complexType.Particle as XmlSchemaSequence;
        XmlSchemaChoice choice = complexType.Particle as XmlSchemaChoice;
        XmlSchemaGroupRef groupRef = complexType.Particle as XmlSchemaGroupRef;
        XmlSchemaComplexContent complexContent =
            complexType.ContentModel as XmlSchemaComplexContent;
        XmlSchemaAll all = complexType.Particle as XmlSchemaAll;
        if (sequence != null)
        {
            return GetChildElementCompletion(items: sequence.Items, prefix: prefix);
        }

        if (choice != null)
        {
            return GetChildElementCompletion(items: choice.Items, prefix: prefix);
        }

        if (complexContent != null)
        {
            return GetChildElementCompletion(complexContent: complexContent, prefix: prefix);
        }

        if (groupRef != null)
        {
            return GetChildElementCompletion(groupRef: groupRef, prefix: prefix);
        }

        if (all != null)
        {
            return GetChildElementCompletion(items: all.Items, prefix: prefix);
        }
        return new XmlCompletionItemCollection();
    }

    XmlCompletionItemCollection GetChildElementCompletion(
        XmlSchemaObjectCollection items,
        string prefix
    )
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        foreach (XmlSchemaObject schemaObject in items)
        {
            XmlSchemaElement childElement = schemaObject as XmlSchemaElement;
            XmlSchemaSequence childSequence = schemaObject as XmlSchemaSequence;
            XmlSchemaChoice childChoice = schemaObject as XmlSchemaChoice;
            XmlSchemaGroupRef groupRef = schemaObject as XmlSchemaGroupRef;
            if (childElement != null)
            {
                string name = childElement.Name;
                if (name == null)
                {
                    name = childElement.RefName.Name;
                    XmlSchemaElement element = FindElement(name: childElement.RefName);
                    if (element != null)
                    {
                        if (element.IsAbstract)
                        {
                            AddSubstitionGroupElements(
                                completionItems: completionItems,
                                groupName: element.QualifiedName,
                                prefix: prefix
                            );
                        }
                        else
                        {
                            AddElement(
                                completionItems: completionItems,
                                name: name,
                                prefix: prefix,
                                annotation: element.Annotation
                            );
                        }
                    }
                    else
                    {
                        AddElement(
                            completionItems: completionItems,
                            name: name,
                            prefix: prefix,
                            annotation: childElement.Annotation
                        );
                    }
                }
                else
                {
                    AddElement(
                        completionItems: completionItems,
                        name: name,
                        prefix: prefix,
                        annotation: childElement.Annotation
                    );
                }
            }
            else if (childSequence != null)
            {
                AddElements(
                    lhs: completionItems,
                    rhs: GetChildElementCompletion(items: childSequence.Items, prefix: prefix)
                );
            }
            else if (childChoice != null)
            {
                AddElements(
                    lhs: completionItems,
                    rhs: GetChildElementCompletion(items: childChoice.Items, prefix: prefix)
                );
            }
            else if (groupRef != null)
            {
                AddElements(
                    lhs: completionItems,
                    rhs: GetChildElementCompletion(groupRef: groupRef, prefix: prefix)
                );
            }
        }
        return completionItems;
    }

    XmlCompletionItemCollection GetChildElementCompletion(
        XmlSchemaComplexContent complexContent,
        string prefix
    )
    {
        XmlSchemaComplexContentExtension extension =
            complexContent.Content as XmlSchemaComplexContentExtension;
        if (extension != null)
        {
            return GetChildElementCompletion(extension: extension, prefix: prefix);
        }
        XmlSchemaComplexContentRestriction restriction =
            complexContent.Content as XmlSchemaComplexContentRestriction;

        if (restriction != null)
        {
            return GetChildElementCompletion(restriction: restriction, prefix: prefix);
        }
        return new XmlCompletionItemCollection();
    }

    XmlCompletionItemCollection GetChildElementCompletion(
        XmlSchemaComplexContentExtension extension,
        string prefix
    )
    {
        XmlCompletionItemCollection completionItems;
        XmlSchemaComplexType complexType = FindNamedType(
            schema: schema,
            name: extension.BaseTypeName
        );
        if (complexType != null)
        {
            completionItems = GetChildElementCompletion(complexType: complexType, prefix: prefix);
        }
        else
        {
            completionItems = new XmlCompletionItemCollection();
        }
        // Add any elements.
        if (extension.Particle != null)
        {
            XmlSchemaSequence sequence = extension.Particle as XmlSchemaSequence;
            XmlSchemaChoice choice = extension.Particle as XmlSchemaChoice;
            XmlSchemaGroupRef groupRef = extension.Particle as XmlSchemaGroupRef;
            if (sequence != null)
            {
                completionItems.AddRange(
                    item: GetChildElementCompletion(items: sequence.Items, prefix: prefix)
                );
            }
            else if (choice != null)
            {
                completionItems.AddRange(
                    item: GetChildElementCompletion(items: choice.Items, prefix: prefix)
                );
            }
            else if (groupRef != null)
            {
                completionItems.AddRange(
                    item: GetChildElementCompletion(groupRef: groupRef, prefix: prefix)
                );
            }
        }
        return completionItems;
    }

    XmlCompletionItemCollection GetChildElementCompletion(XmlSchemaGroupRef groupRef, string prefix)
    {
        XmlSchemaGroup schemaGroup = FindGroup(name: groupRef.RefName.Name);
        if (schemaGroup != null)
        {
            XmlSchemaSequence sequence = schemaGroup.Particle as XmlSchemaSequence;
            XmlSchemaChoice choice = schemaGroup.Particle as XmlSchemaChoice;
            if (sequence != null)
            {
                return GetChildElementCompletion(items: sequence.Items, prefix: prefix);
            }

            if (choice != null)
            {
                return GetChildElementCompletion(items: choice.Items, prefix: prefix);
            }
        }
        return new XmlCompletionItemCollection();
    }

    XmlCompletionItemCollection GetChildElementCompletion(
        XmlSchemaComplexContentRestriction restriction,
        string prefix
    )
    {
        // Add any elements.
        if (restriction.Particle != null)
        {
            XmlSchemaSequence sequence = restriction.Particle as XmlSchemaSequence;
            XmlSchemaChoice choice = restriction.Particle as XmlSchemaChoice;
            XmlSchemaGroupRef groupRef = restriction.Particle as XmlSchemaGroupRef;
            if (sequence != null)
            {
                return GetChildElementCompletion(items: sequence.Items, prefix: prefix);
            }

            if (choice != null)
            {
                return GetChildElementCompletion(items: choice.Items, prefix: prefix);
            }

            if (groupRef != null)
            {
                return GetChildElementCompletion(groupRef: groupRef, prefix: prefix);
            }
        }
        return new XmlCompletionItemCollection();
    }

    /// <summary>
    /// Adds an element completion data to the collection if it does not
    /// already exist.
    /// </summary>
    static void AddElement(
        XmlCompletionItemCollection completionItems,
        string name,
        string prefix,
        string documentation
    )
    {
        if (!completionItems.Contains(name: name))
        {
            if (prefix.Length > 0)
            {
                name = String.Concat(str0: prefix, str1: ":", str2: name);
            }
            XmlCompletionItem item = new XmlCompletionItem(text: name, description: documentation);
            completionItems.Add(item: item);
        }
    }

    static string GetDocumentation(XmlSchemaAnnotation annotation)
    {
        return new SchemaDocumentation(annotation: annotation).ToString();
    }

    /// <summary>
    /// Adds an element completion data to the collection if it does not
    /// already exist.
    /// </summary>
    static void AddElement(
        XmlCompletionItemCollection completionItems,
        string name,
        string prefix,
        XmlSchemaAnnotation annotation
    )
    {
        string documentation = GetDocumentation(annotation: annotation);
        AddElement(
            completionItems: completionItems,
            name: name,
            prefix: prefix,
            documentation: documentation
        );
    }

    /// <summary>
    /// Adds elements to the collection if it does not already exist.
    /// </summary>
    static void AddElements(XmlCompletionItemCollection lhs, XmlCompletionItemCollection rhs)
    {
        foreach (XmlCompletionItem item in rhs)
        {
            if (!lhs.Contains(name: item.Text))
            {
                lhs.Add(item: item);
            }
        }
    }

    XmlCompletionItemCollection GetAttributeCompletion(
        XmlSchemaElement element,
        XmlNamespaceCollection namespacesInScope
    )
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        XmlSchemaComplexType complexType = GetElementAsComplexType(element: element);
        if (complexType != null)
        {
            completionItems.AddRange(
                item: GetAttributeCompletion(
                    complexType: complexType,
                    namespacesInScope: namespacesInScope
                )
            );
        }
        return completionItems;
    }

    XmlCompletionItemCollection GetAttributeCompletion(
        XmlSchemaComplexContentRestriction restriction,
        XmlNamespaceCollection namespacesInScope
    )
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        completionItems.AddRange(
            item: GetAttributeCompletion(
                attributes: restriction.Attributes,
                namespacesInScope: namespacesInScope
            )
        );
        completionItems.AddRange(
            item: GetBaseComplexTypeAttributeCompletion(
                baseTypeName: restriction.BaseTypeName,
                namespacesInScope: namespacesInScope
            )
        );
        return completionItems;
    }

    XmlCompletionItemCollection GetAttributeCompletion(
        XmlSchemaComplexType complexType,
        XmlNamespaceCollection namespacesInScope
    )
    {
        XmlCompletionItemCollection completionItems = GetAttributeCompletion(
            attributes: complexType.Attributes,
            namespacesInScope: namespacesInScope
        );
        // Add any complex content attributes.
        XmlSchemaComplexContent complexContent =
            complexType.ContentModel as XmlSchemaComplexContent;
        if (complexContent != null)
        {
            XmlSchemaComplexContentExtension extension =
                complexContent.Content as XmlSchemaComplexContentExtension;
            XmlSchemaComplexContentRestriction restriction =
                complexContent.Content as XmlSchemaComplexContentRestriction;
            if (extension != null)
            {
                completionItems.AddRange(
                    item: GetAttributeCompletion(
                        extension: extension,
                        namespacesInScope: namespacesInScope
                    )
                );
            }
            else if (restriction != null)
            {
                completionItems.AddRange(
                    item: GetAttributeCompletion(
                        restriction: restriction,
                        namespacesInScope: namespacesInScope
                    )
                );
            }
        }
        else
        {
            XmlSchemaSimpleContent simpleContent =
                complexType.ContentModel as XmlSchemaSimpleContent;
            if (simpleContent != null)
            {
                completionItems.AddRange(
                    item: GetAttributeCompletion(
                        simpleContent: simpleContent,
                        namespacesInScope: namespacesInScope
                    )
                );
            }
        }
        return completionItems;
    }

    XmlCompletionItemCollection GetAttributeCompletion(
        XmlSchemaComplexContentExtension extension,
        XmlNamespaceCollection namespacesInScope
    )
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        completionItems.AddRange(
            item: GetAttributeCompletion(
                attributes: extension.Attributes,
                namespacesInScope: namespacesInScope
            )
        );
        completionItems.AddRange(
            item: GetBaseComplexTypeAttributeCompletion(
                baseTypeName: extension.BaseTypeName,
                namespacesInScope: namespacesInScope
            )
        );
        return completionItems;
    }

    XmlCompletionItemCollection GetAttributeCompletion(
        XmlSchemaSimpleContent simpleContent,
        XmlNamespaceCollection namespacesInScope
    )
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        XmlSchemaSimpleContentExtension extension =
            simpleContent.Content as XmlSchemaSimpleContentExtension;
        if (extension != null)
        {
            completionItems.AddRange(
                item: GetAttributeCompletion(
                    extension: extension,
                    namespacesInScope: namespacesInScope
                )
            );
        }
        return completionItems;
    }

    XmlCompletionItemCollection GetAttributeCompletion(
        XmlSchemaSimpleContentExtension extension,
        XmlNamespaceCollection namespacesInScope
    )
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        completionItems.AddRange(
            item: GetAttributeCompletion(
                attributes: extension.Attributes,
                namespacesInScope: namespacesInScope
            )
        );
        completionItems.AddRange(
            item: GetBaseComplexTypeAttributeCompletion(
                baseTypeName: extension.BaseTypeName,
                namespacesInScope: namespacesInScope
            )
        );
        return completionItems;
    }

    XmlCompletionItemCollection GetBaseComplexTypeAttributeCompletion(
        XmlQualifiedName baseTypeName,
        XmlNamespaceCollection namespacesInScope
    )
    {
        XmlSchemaComplexType baseComplexType = FindNamedType(schema: schema, name: baseTypeName);
        if (baseComplexType != null)
        {
            return GetAttributeCompletion(
                complexType: baseComplexType,
                namespacesInScope: namespacesInScope
            );
        }
        return new XmlCompletionItemCollection();
    }

    XmlCompletionItemCollection GetAttributeCompletion(
        XmlSchemaObjectCollection attributes,
        XmlNamespaceCollection namespacesInScope
    )
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        foreach (XmlSchemaObject schemaObject in attributes)
        {
            XmlSchemaAttribute attribute = schemaObject as XmlSchemaAttribute;
            XmlSchemaAttributeGroupRef attributeGroupRef =
                schemaObject as XmlSchemaAttributeGroupRef;
            if (attribute != null)
            {
                if (!IsProhibitedAttribute(attribute: attribute))
                {
                    AddAttribute(
                        completionItems: completionItems,
                        attribute: attribute,
                        namespacesInScope: namespacesInScope
                    );
                }
                else
                {
                    prohibitedAttributes.Add(item: attribute);
                }
            }
            else if (attributeGroupRef != null)
            {
                completionItems.AddRange(
                    item: GetAttributeCompletion(
                        groupRef: attributeGroupRef,
                        namespacesInScope: namespacesInScope
                    )
                );
            }
        }
        return completionItems;
    }

    /// <summary>
    /// Checks that the attribute is prohibited or has been flagged
    /// as prohibited previously.
    /// </summary>
    bool IsProhibitedAttribute(XmlSchemaAttribute attribute)
    {
        if (attribute.Use == XmlSchemaUse.Prohibited)
        {
            return true;
        }

        foreach (XmlSchemaAttribute prohibitedAttribute in prohibitedAttributes)
        {
            if (prohibitedAttribute.QualifiedName == attribute.QualifiedName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Adds an attribute to the completion data collection.
    /// </summary>
    /// <remarks>
    /// Note the special handling of xml:lang attributes.
    /// </remarks>
    void AddAttribute(
        XmlCompletionItemCollection completionItems,
        XmlSchemaAttribute attribute,
        XmlNamespaceCollection namespacesInScope
    )
    {
        string name = attribute.Name;
        if (name == null)
        {
            if (attribute.RefName.Namespace == "http://www.w3.org/XML/1998/namespace")
            {
                name = String.Concat(str0: "xml:", str1: attribute.RefName.Name);
            }
            else
            {
                string prefix = namespacesInScope.GetPrefix(
                    namespaceToMatch: attribute.RefName.Namespace
                );
                if (!String.IsNullOrEmpty(value: prefix))
                {
                    name = String.Concat(str0: prefix, str1: ":", str2: attribute.RefName.Name);
                }
            }
        }
        if (name != null)
        {
            string documentation = GetDocumentation(annotation: attribute.Annotation);
            XmlCompletionItem item = new XmlCompletionItem(
                text: name,
                description: documentation,
                dataType: XmlCompletionItemType.XmlAttribute
            );
            completionItems.Add(item: item);
        }
    }

    /// <summary>
    /// Gets attribute completion data from a group ref.
    /// </summary>
    XmlCompletionItemCollection GetAttributeCompletion(
        XmlSchemaAttributeGroupRef groupRef,
        XmlNamespaceCollection namespacesInScope
    )
    {
        XmlSchemaAttributeGroup attributeGroup = FindAttributeGroup(
            schema: schema,
            name: groupRef.RefName.Name
        );
        if (attributeGroup != null)
        {
            return GetAttributeCompletion(
                attributes: attributeGroup.Attributes,
                namespacesInScope: namespacesInScope
            );
        }
        return new XmlCompletionItemCollection();
    }

    static XmlSchemaComplexType FindNamedType(XmlSchema schema, XmlQualifiedName name)
    {
        if (name != null)
        {
            foreach (XmlSchemaObject schemaObject in schema.Items)
            {
                XmlSchemaComplexType complexType = schemaObject as XmlSchemaComplexType;
                if (complexType != null)
                {
                    if (complexType.QualifiedName == name)
                    {
                        return complexType;
                    }
                }
            }
            // Try included schemas.
            foreach (XmlSchemaExternal external in schema.Includes)
            {
                XmlSchemaInclude include = external as XmlSchemaInclude;
                if (include != null)
                {
                    if (include.Schema != null)
                    {
                        XmlSchemaComplexType matchedComplexType = FindNamedType(
                            schema: include.Schema,
                            name: name
                        );
                        if (matchedComplexType != null)
                        {
                            return matchedComplexType;
                        }
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Finds an element that matches the specified <paramref name="name"/>
    /// from the children of the given <paramref name="element"/>.
    /// </summary>
    XmlSchemaElement FindChildElement(XmlSchemaElement element, QualifiedName name)
    {
        XmlSchemaComplexType complexType = GetElementAsComplexType(element: element);
        if (complexType != null)
        {
            return FindChildElement(complexType: complexType, name: name);
        }
        return null;
    }

    XmlSchemaElement FindChildElement(XmlSchemaComplexType complexType, QualifiedName name)
    {
        XmlSchemaSequence sequence = complexType.Particle as XmlSchemaSequence;
        XmlSchemaChoice choice = complexType.Particle as XmlSchemaChoice;
        XmlSchemaGroupRef groupRef = complexType.Particle as XmlSchemaGroupRef;
        XmlSchemaAll all = complexType.Particle as XmlSchemaAll;
        XmlSchemaComplexContent complexContent =
            complexType.ContentModel as XmlSchemaComplexContent;
        if (sequence != null)
        {
            return FindElement(items: sequence.Items, name: name);
        }

        if (choice != null)
        {
            return FindElement(items: choice.Items, name: name);
        }

        if (complexContent != null)
        {
            XmlSchemaComplexContentExtension extension =
                complexContent.Content as XmlSchemaComplexContentExtension;
            XmlSchemaComplexContentRestriction restriction =
                complexContent.Content as XmlSchemaComplexContentRestriction;
            if (extension != null)
            {
                return FindChildElement(extension: extension, name: name);
            }

            if (restriction != null)
            {
                return FindChildElement(restriction: restriction, name: name);
            }
        }
        else if (groupRef != null)
        {
            return FindElement(groupRef: groupRef, name: name);
        }
        else if (all != null)
        {
            return FindElement(items: all.Items, name: name);
        }
        return null;
    }

    /// <summary>
    /// Finds the named child element contained in the extension element.
    /// </summary>
    XmlSchemaElement FindChildElement(
        XmlSchemaComplexContentExtension extension,
        QualifiedName name
    )
    {
        XmlSchemaComplexType complexType = FindNamedType(
            schema: schema,
            name: extension.BaseTypeName
        );
        if (complexType != null)
        {
            XmlSchemaElement matchedElement = FindChildElement(
                complexType: complexType,
                name: name
            );
            if (matchedElement == null)
            {
                XmlSchemaSequence sequence = extension.Particle as XmlSchemaSequence;
                XmlSchemaChoice choice = extension.Particle as XmlSchemaChoice;
                XmlSchemaGroupRef groupRef = extension.Particle as XmlSchemaGroupRef;
                if (sequence != null)
                {
                    return FindElement(items: sequence.Items, name: name);
                }

                if (choice != null)
                {
                    return FindElement(items: choice.Items, name: name);
                }

                if (groupRef != null)
                {
                    return FindElement(groupRef: groupRef, name: name);
                }
            }
            else
            {
                return matchedElement;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the named child element contained in the restriction element.
    /// </summary>
    XmlSchemaElement FindChildElement(
        XmlSchemaComplexContentRestriction restriction,
        QualifiedName name
    )
    {
        XmlSchemaSequence sequence = restriction.Particle as XmlSchemaSequence;
        XmlSchemaGroupRef groupRef = restriction.Particle as XmlSchemaGroupRef;
        if (sequence != null)
        {
            return FindElement(items: sequence.Items, name: name);
        }

        if (groupRef != null)
        {
            return FindElement(groupRef: groupRef, name: name);
        }
        return null;
    }

    /// <summary>
    /// Finds the element in the collection of schema objects.
    /// </summary>
    XmlSchemaElement FindElement(XmlSchemaObjectCollection items, QualifiedName name)
    {
        foreach (XmlSchemaObject schemaObject in items)
        {
            XmlSchemaElement element = schemaObject as XmlSchemaElement;
            XmlSchemaSequence sequence = schemaObject as XmlSchemaSequence;
            XmlSchemaChoice choice = schemaObject as XmlSchemaChoice;
            XmlSchemaGroupRef groupRef = schemaObject as XmlSchemaGroupRef;
            XmlSchemaElement matchedElement = null;
            if (element != null)
            {
                if (element.Name != null)
                {
                    if (name.Name == element.Name)
                    {
                        return element;
                    }
                }
                else if (element.RefName != null)
                {
                    if (name.Name == element.RefName.Name)
                    {
                        matchedElement = FindElement(name: element.RefName);
                    }
                    else
                    {
                        // Abstract element?
                        XmlSchemaElement abstractElement = FindElement(name: element.RefName);
                        if (abstractElement != null && abstractElement.IsAbstract)
                        {
                            matchedElement = FindSubstitutionGroupElement(
                                groupName: abstractElement.QualifiedName,
                                name: name
                            );
                        }
                    }
                }
            }
            else if (sequence != null)
            {
                matchedElement = FindElement(items: sequence.Items, name: name);
            }
            else if (choice != null)
            {
                matchedElement = FindElement(items: choice.Items, name: name);
            }
            else if (groupRef != null)
            {
                matchedElement = FindElement(groupRef: groupRef, name: name);
            }
            // Did we find a match?
            if (matchedElement != null)
            {
                return matchedElement;
            }
        }
        return null;
    }

    XmlSchemaElement FindElement(XmlSchemaGroupRef groupRef, QualifiedName name)
    {
        XmlSchemaGroup schemaGroup = FindGroup(name: groupRef.RefName.Name);
        if (schemaGroup != null)
        {
            XmlSchemaSequence sequence = schemaGroup.Particle as XmlSchemaSequence;
            XmlSchemaChoice choice = schemaGroup.Particle as XmlSchemaChoice;
            if (sequence != null)
            {
                return FindElement(items: sequence.Items, name: name);
            }

            if (choice != null)
            {
                return FindElement(items: choice.Items, name: name);
            }
        }
        return null;
    }

    static XmlSchemaAttributeGroup FindAttributeGroup(XmlSchema schema, string name)
    {
        if (name != null)
        {
            foreach (XmlSchemaObject schemaObject in schema.Items)
            {
                XmlSchemaAttributeGroup attributeGroup = schemaObject as XmlSchemaAttributeGroup;
                if (attributeGroup != null)
                {
                    if (attributeGroup.Name == name)
                    {
                        return attributeGroup;
                    }
                }
            }
            // Try included schemas.
            foreach (XmlSchemaExternal external in schema.Includes)
            {
                XmlSchemaInclude include = external as XmlSchemaInclude;
                if (include != null)
                {
                    if (include.Schema != null)
                    {
                        return FindAttributeGroup(schema: include.Schema, name: name);
                    }
                }
            }
        }
        return null;
    }

    XmlCompletionItemCollection GetAttributeValueCompletion(XmlSchemaElement element, string name)
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        XmlSchemaComplexType complexType = GetElementAsComplexType(element: element);
        if (complexType != null)
        {
            XmlSchemaAttribute attribute = FindAttribute(complexType: complexType, name: name);
            if (attribute != null)
            {
                completionItems.AddRange(item: GetAttributeValueCompletion(attribute: attribute));
            }
        }
        return completionItems;
    }

    XmlCompletionItemCollection GetAttributeValueCompletion(XmlSchemaAttribute attribute)
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        if (attribute.SchemaType != null)
        {
            XmlSchemaSimpleTypeRestriction simpleTypeRestriction =
                attribute.SchemaType.Content as XmlSchemaSimpleTypeRestriction;
            if (simpleTypeRestriction != null)
            {
                completionItems.AddRange(
                    item: GetAttributeValueCompletion(simpleTypeRestriction: simpleTypeRestriction)
                );
            }
        }
        else if (attribute.AttributeSchemaType != null)
        {
            XmlSchemaSimpleType simpleType = attribute.AttributeSchemaType as XmlSchemaSimpleType;
            if (simpleType != null)
            {
                if (simpleType.Datatype.TypeCode == XmlTypeCode.Boolean)
                {
                    completionItems.AddRange(item: GetBooleanAttributeValueCompletion());
                }
                else
                {
                    completionItems.AddRange(
                        item: GetAttributeValueCompletion(simpleType: simpleType)
                    );
                }
            }
        }
        return completionItems;
    }

    static XmlCompletionItemCollection GetAttributeValueCompletion(
        XmlSchemaSimpleTypeRestriction simpleTypeRestriction
    )
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        foreach (XmlSchemaObject schemaObject in simpleTypeRestriction.Facets)
        {
            XmlSchemaEnumerationFacet enumFacet = schemaObject as XmlSchemaEnumerationFacet;
            if (enumFacet != null)
            {
                AddAttributeValue(
                    completionItems: completionItems,
                    valueText: enumFacet.Value,
                    annotation: enumFacet.Annotation
                );
            }
        }
        return completionItems;
    }

    XmlCompletionItemCollection GetAttributeValueCompletion(XmlSchemaSimpleTypeUnion union)
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        foreach (XmlSchemaObject schemaObject in union.BaseTypes)
        {
            XmlSchemaSimpleType simpleType = schemaObject as XmlSchemaSimpleType;
            if (simpleType != null)
            {
                completionItems.AddRange(item: GetAttributeValueCompletion(simpleType: simpleType));
            }
        }
        if (union.BaseMemberTypes != null)
        {
            foreach (XmlSchemaSimpleType simpleType in union.BaseMemberTypes)
            {
                completionItems.AddRange(item: GetAttributeValueCompletion(simpleType: simpleType));
            }
        }
        return completionItems;
    }

    XmlCompletionItemCollection GetAttributeValueCompletion(XmlSchemaSimpleType simpleType)
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        XmlSchemaSimpleTypeRestriction simpleTypeRestriction =
            simpleType.Content as XmlSchemaSimpleTypeRestriction;
        XmlSchemaSimpleTypeUnion union = simpleType.Content as XmlSchemaSimpleTypeUnion;
        XmlSchemaSimpleTypeList list = simpleType.Content as XmlSchemaSimpleTypeList;
        if (simpleTypeRestriction != null)
        {
            completionItems.AddRange(
                item: GetAttributeValueCompletion(simpleTypeRestriction: simpleTypeRestriction)
            );
        }
        else if (union != null)
        {
            completionItems.AddRange(item: GetAttributeValueCompletion(union: union));
        }
        else if (list != null)
        {
            completionItems.AddRange(item: GetAttributeValueCompletion(list: list));
        }
        return completionItems;
    }

    XmlCompletionItemCollection GetAttributeValueCompletion(XmlSchemaSimpleTypeList list)
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        if (list.ItemType != null)
        {
            completionItems.AddRange(item: GetAttributeValueCompletion(simpleType: list.ItemType));
        }
        else if (list.ItemTypeName != null)
        {
            XmlSchemaSimpleType simpleType = FindSimpleType(name: list.ItemTypeName);
            if (simpleType != null)
            {
                completionItems.AddRange(item: GetAttributeValueCompletion(simpleType: simpleType));
            }
        }
        return completionItems;
    }

    /// <summary>
    /// Gets the set of attribute values for an xs:boolean type.
    /// </summary>
    static XmlCompletionItemCollection GetBooleanAttributeValueCompletion()
    {
        XmlCompletionItemCollection completionItems = new XmlCompletionItemCollection();
        AddAttributeValue(completionItems: completionItems, valueText: "0");
        AddAttributeValue(completionItems: completionItems, valueText: "1");
        AddAttributeValue(completionItems: completionItems, valueText: "true");
        AddAttributeValue(completionItems: completionItems, valueText: "false");
        return completionItems;
    }

    XmlSchemaAttribute FindAttribute(XmlSchemaComplexType complexType, string name)
    {
        XmlSchemaAttribute matchedAttribute = FindAttribute(
            schemaObjects: complexType.Attributes,
            name: name
        );
        if (matchedAttribute == null)
        {
            XmlSchemaComplexContent complexContent =
                complexType.ContentModel as XmlSchemaComplexContent;
            if (complexContent != null)
            {
                return FindAttribute(complexContent: complexContent, name: name);
            }
        }
        return matchedAttribute;
    }

    XmlSchemaAttribute FindAttribute(XmlSchemaObjectCollection schemaObjects, string name)
    {
        foreach (XmlSchemaObject schemaObject in schemaObjects)
        {
            XmlSchemaAttribute attribute = schemaObject as XmlSchemaAttribute;
            XmlSchemaAttributeGroupRef groupRef = schemaObject as XmlSchemaAttributeGroupRef;
            if (attribute != null)
            {
                if (attribute.Name == name)
                {
                    return attribute;
                }
            }
            else if (groupRef != null)
            {
                XmlSchemaAttribute matchedAttribute = FindAttribute(groupRef: groupRef, name: name);
                if (matchedAttribute != null)
                {
                    return matchedAttribute;
                }
            }
        }
        return null;
    }

    XmlSchemaAttribute FindAttribute(XmlSchemaAttributeGroupRef groupRef, string name)
    {
        if (groupRef.RefName != null)
        {
            XmlSchemaAttributeGroup attributeGroup = FindAttributeGroup(
                schema: schema,
                name: groupRef.RefName.Name
            );
            if (attributeGroup != null)
            {
                return FindAttribute(schemaObjects: attributeGroup.Attributes, name: name);
            }
        }
        return null;
    }

    XmlSchemaAttribute FindAttribute(XmlSchemaComplexContent complexContent, string name)
    {
        XmlSchemaComplexContentExtension extension =
            complexContent.Content as XmlSchemaComplexContentExtension;
        XmlSchemaComplexContentRestriction restriction =
            complexContent.Content as XmlSchemaComplexContentRestriction;
        if (extension != null)
        {
            return FindAttribute(extension: extension, name: name);
        }

        if (restriction != null)
        {
            return FindAttribute(restriction: restriction, name: name);
        }
        return null;
    }

    XmlSchemaAttribute FindAttribute(XmlSchemaComplexContentExtension extension, string name)
    {
        return FindAttribute(schemaObjects: extension.Attributes, name: name);
    }

    XmlSchemaAttribute FindAttribute(XmlSchemaComplexContentRestriction restriction, string name)
    {
        XmlSchemaAttribute matchedAttribute = FindAttribute(
            schemaObjects: restriction.Attributes,
            name: name
        );
        if (matchedAttribute == null)
        {
            XmlSchemaComplexType complexType = FindNamedType(
                schema: schema,
                name: restriction.BaseTypeName
            );
            if (complexType != null)
            {
                return FindAttribute(complexType: complexType, name: name);
            }
        }
        return matchedAttribute;
    }

    /// <summary>
    /// Adds an attribute value to the completion data collection.
    /// </summary>
    static void AddAttributeValue(XmlCompletionItemCollection completionItems, string valueText)
    {
        XmlCompletionItem item = new XmlCompletionItem(
            text: valueText,
            dataType: XmlCompletionItemType.XmlAttributeValue
        );
        completionItems.Add(item: item);
    }

    /// <summary>
    /// Adds an attribute value to the completion data collection.
    /// </summary>
    static void AddAttributeValue(
        XmlCompletionItemCollection completionItems,
        string valueText,
        XmlSchemaAnnotation annotation
    )
    {
        string documentation = GetDocumentation(annotation: annotation);
        XmlCompletionItem item = new XmlCompletionItem(
            text: valueText,
            description: documentation,
            dataType: XmlCompletionItemType.XmlAttributeValue
        );
        completionItems.Add(item: item);
    }

    /// <summary>
    /// Adds an attribute value to the completion data collection.
    /// </summary>
    static void AddAttributeValue(
        XmlCompletionItemCollection completionItems,
        string valueText,
        string description
    )
    {
        XmlCompletionItem item = new XmlCompletionItem(
            text: valueText,
            description: description,
            dataType: XmlCompletionItemType.XmlAttributeValue
        );
        completionItems.Add(item: item);
    }

    XmlSchemaSimpleType FindSimpleType(XmlQualifiedName name)
    {
        foreach (XmlSchemaObject schemaObject in schema.SchemaTypes.Values)
        {
            XmlSchemaSimpleType simpleType = schemaObject as XmlSchemaSimpleType;
            if (simpleType != null)
            {
                if (simpleType.QualifiedName == name)
                {
                    return simpleType;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Adds any elements that have the specified substitution group.
    /// </summary>
    void AddSubstitionGroupElements(
        XmlCompletionItemCollection completionItems,
        XmlQualifiedName groupName,
        string prefix
    )
    {
        foreach (XmlSchemaElement element in schema.Elements.Values)
        {
            if (element.SubstitutionGroup == groupName)
            {
                AddElement(
                    completionItems: completionItems,
                    name: element.Name,
                    prefix: prefix,
                    annotation: element.Annotation
                );
            }
        }
    }

    /// <summary>
    /// Looks for the substitution group element of the specified name.
    /// </summary>
    XmlSchemaElement FindSubstitutionGroupElement(XmlQualifiedName groupName, QualifiedName name)
    {
        foreach (XmlSchemaElement element in schema.Elements.Values)
        {
            if (element.SubstitutionGroup == groupName)
            {
                if (element.Name != null)
                {
                    if (element.Name == name.Name)
                    {
                        return element;
                    }
                }
            }
        }
        return null;
    }
}
