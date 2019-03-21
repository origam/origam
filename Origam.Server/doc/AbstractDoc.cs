#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
﻿#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Xml;
using Origam.Schema;
using Origam.UI;
using Origam;
using Origam.Workbench.Services;
using System.Collections;

namespace Origam.Server.Doc
{
    public abstract class AbstractDoc
    {
        private XmlWriter _writer;
        private bool _useAuthorization = false;

        public AbstractDoc(XmlWriter writer)
        {
            _writer = writer;
        }

        public abstract ISchemaItemProvider RootProvider();
        public abstract string FilterName { get; }
        public abstract string Name { get; }
        
        public virtual string Title(string elementId, IDocumentationService documentation, IPersistenceService ps)
        {
            AbstractSchemaItem item = ps.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(new Guid(elementId))) as AbstractSchemaItem;
            return item.NodeText;
        }

        public XmlWriter Writer
        {
            get
            {
                return _writer;
            }
        }

        public virtual bool UseShortHelpAsName
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsTocRecursive
        {
            get
            {
                return false;
            }
        }

        public virtual bool UsePrettyPrint
        {
            get
            {
                return false;
            }
        }

        public virtual bool UseDiagrams
        {
            get
            {
                return false;
            }
        }

        public virtual DiagramConnectorType ConnectorType
        {
            get
            {
                return DiagramConnectorType.Flowchart;
            }
        }

        public virtual bool IsFolder(object obj)
        {
            return obj is SchemaItemGroup;
        }

        /// <summary>
        /// By setting false we can speed up filtering by package because
        /// no recursive search will be performed on children of items.
        /// </summary>
        public virtual bool CanExtendItems
        {
            get
            {
                return true;
            }
        }

        public virtual List<DiagramConnection> WriteContent(string bodyElement, string elementId, XmlWriter writer, IDocumentationService documentation, IPersistenceService ps)
        {
            writer.WriteStartDocument();
            writer.WriteElementString("p", "Not implemented");
            writer.WriteEndDocument();
            return null;
        }

        public void WriteToc()
        {
            WriteToc(Guid.Empty);
        }

        public void WriteToc(Guid packageId)
        {
            IDocumentationService documentation = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;
            IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
            bool filterByPackage = (packageId != Guid.Empty);

            Dictionary<IBrowserNode2, object> list = new Dictionary<IBrowserNode2, object>();
            GetList(list, this.RootProvider(), auth, filterByPackage, packageId);

            if (list.Count > 0)
            {
                if (filterByPackage)
                {
                    Writer.WriteElementString(DocTools.SECTION_HEADING, this.Name);
                }
                if (list.Count == 1)
                {
                    foreach (KeyValuePair<IBrowserNode2, object> first in list)
                    {
                        if (first.Key is SchemaItemGroup)
                        {
                            // the only root element in the list is a group - we skip it
                            // because it is most probably the package name again
                            list = first.Value as Dictionary<IBrowserNode2, object>;
                            break;
                        }
                    }
                }
                GetToc(list, documentation, filterByPackage, packageId);
            }
        }

        public void GetList(Dictionary<IBrowserNode2, object> list, IBrowserNode2 parentNode, IOrigamAuthorizationProvider auth, bool filterByPackage, Guid packageId)
        {
            ArrayList childNodes = new ArrayList(parentNode.ChildNodes());
            childNodes.Sort();

            foreach (IBrowserNode2 child in childNodes)
            {
                IAuthorizationContextContainer authorizableItem = child as IAuthorizationContextContainer;
                AbstractSchemaItem item = child as AbstractSchemaItem;
                SchemaItemGroup folder = child as SchemaItemGroup;

                if (IsFolder(child))
                {
                    Dictionary<IBrowserNode2, object> childList = new Dictionary<IBrowserNode2, object>();
                    GetList(childList, child, auth, filterByPackage, packageId);
                    if (childList.Count > 0)
                    {
                        list.Add(child, childList);
                    }
                }
                else if (item != null)
                {
                    // display documentation only for accessible items
                    if (_useAuthorization == false ||
                        authorizableItem != null && auth.Authorize(SecurityManager.CurrentPrincipal, authorizableItem.AuthorizationContext)
                        )
                    {
                        bool include = true;
                        if (filterByPackage)
                        {
                            include = Include(item, packageId);
                        }

                        if (include)
                        {
                            list.Add(child, null);
                        }
                    }
                }
            }
        }

        private bool Include(AbstractSchemaItem item, Guid packageId)
        {
            if (item.DerivedFrom == null && item.SchemaExtensionId.Equals(packageId))
            {
                return true;
            }
            if (!this.CanExtendItems)
            {
                return false;
            }
            foreach (AbstractSchemaItem child in item.ChildItems)
            {
                if(Include(child, packageId))
                {
                    return true;
                }
            }
            return false;
        }

        private void GetToc(Dictionary<IBrowserNode2, object> list, IDocumentationService documentation, bool filterByPackage, Guid packageId)
        {
            Writer.WriteStartElement("ul");
            foreach (KeyValuePair<IBrowserNode2, object> child in list)
            {
                IAuthorizationContextContainer authorizableItem = child.Key as IAuthorizationContextContainer;
                AbstractSchemaItem item = child.Key as AbstractSchemaItem;
                SchemaItemGroup folder = child.Key as SchemaItemGroup;

                if (IsFolder(child.Key))
                {
                    Writer.WriteStartElement("li");
                    Writer.WriteAttributeString("class", "tocref");
                    DocTools.WriteImage(Writer, DocTools.ImagePath(child.Key));
                    Writer.WriteString(child.Key.NodeText);
                    Writer.WriteEndElement();
                    Dictionary<IBrowserNode2, object> children = child.Value as Dictionary<IBrowserNode2, object>;
                    GetToc(children, documentation, filterByPackage, packageId);
                }
                else if (item != null)
                {
                    string name = item.NodeText;
                    string extraClass = null;
                    if (UseShortHelpAsName)
                    {
                        name = DocTools.Name(documentation, item.Id, name);
                    }
                    if (filterByPackage && ! item.SchemaExtensionId.Equals(packageId))
                    {
                        // this item is not from the current package so it has been extended
                        name += " (extended)";
                        extraClass = "extended";
                    }
                    DocTools.WriteTocElement(Writer,
                        item,
                        DocTools.SECTION_CONTENT,
                        extraClass,
                        this.FilterName);
                }
            }
            Writer.WriteEndElement();
        }

        public static bool CheckElementId(XmlWriter writer, string elementId)
        {
            if (elementId == null)
            {
                writer.WriteElementString("p", "ElementId cannot be empty.");
                return false;
            }

            return true;
        }

        public void WriteAncestors(XmlWriter writer, AbstractSchemaItem item)
        {
            if (item.Ancestors.Count > 0)
            {
                DocTools.WriteSectionStart(writer, "Ancestors");
                writer.WriteStartElement("ul");
                foreach (SchemaItemAncestor ancestor in item.Ancestors)
                {
                    writer.WriteStartElement("li");
                    DocTools.WriteElementLink(writer, ancestor.Ancestor, this.FilterName);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        public void WriteRoles(XmlWriter writer, AbstractSchemaItem item)
        {
            IAuthorizationContextContainer authContext = item as IAuthorizationContextContainer;

            if (authContext != null)
            {
                DocTools.WriteSectionStart(writer, "Application Roles");
                if (authContext.AuthorizationContext == "*")
                {
                    writer.WriteElementString("p", "This object is public. Everybody, even without any application role assigned can access this object.");
                }
                else if (authContext.AuthorizationContext == null || authContext.AuthorizationContext.Length == 0)
                {
                    writer.WriteElementString("p", "This object doesn't have any role assigned and therefore is not available.");
                }
                else
                {
                    writer.WriteElementString("p", "This object is accessible to users with the following application roles assigned:");
                    writer.WriteStartElement("ul");
                    foreach (string role in authContext.AuthorizationContext.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        writer.WriteElementString("li", role);
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        public void WritePackages(XmlWriter writer, AbstractSchemaItem item)
        {
            DocTools.WriteSectionStart(writer, "Packages");
            writer.WriteElementString("p", "Defined in package " + item.SchemaExtension.Name);

            Dictionary<string, List<AbstractSchemaItem>> extensions = new Dictionary<string, List<AbstractSchemaItem>>();
            foreach (AbstractSchemaItem child in item.ChildItems)
            {
                if (! child.SchemaExtension.PrimaryKey.Equals(item.SchemaExtension.PrimaryKey)
                    && child.DerivedFrom == null)
                {
                    string packageName = child.SchemaExtension.Name;
                    List<AbstractSchemaItem> list;
                    if (extensions.ContainsKey(packageName))
                    {
                        list = extensions[packageName];
                    }
                    else
                    {
                        list = new List<AbstractSchemaItem>();
                        extensions.Add(child.SchemaExtension.Name, list);
                    }
                    list.Add(child);
                }
            }

            if (extensions.Count > 0)
            {
                foreach (string package in extensions.Keys)
                {
                    writer.WriteElementString("p", "Added in package " + package);
                    writer.WriteStartElement("ul");
                    foreach (AbstractSchemaItem extension in extensions[package])
                    {
                        writer.WriteStartElement("li");
                        writer.WriteString(extension.ItemType + " ");
                        DocTools.WriteElementLink(writer, extension, false, this.FilterName);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        public static void WriteSummary(XmlWriter writer, IDocumentationService documentation, ISchemaItem item)
        {
            string longHelp = DocTools.LongHelp(documentation, (Guid)item.PrimaryKey["Id"]);
            if (longHelp != null)
            {
                MarkdownSharp.Markdown md = new MarkdownSharp.Markdown();
                DocTools.WriteDivStart(writer, "summary");
                writer.WriteRaw(md.Transform(longHelp));
                writer.WriteEndElement();   // div
            }
        }

        public void WriteDiagram(XmlWriter writer, List<DiagramElement> elements)
        {
            WriteDiagram(writer, elements, true);
        }

        public void WriteDiagram(XmlWriter writer, List<DiagramElement> elements, bool isMain)
        {
            if (elements.Count == 0) return;

            int maxTop = 0;
            int height = 0;
            foreach (DiagramElement element in elements)
            {
                if (element.Top > maxTop)
                {
                    maxTop = element.Top;
                    height = element.Height;
                }
            }

            if (isMain)
            {
                DocTools.WriteSectionStart(writer, "Diagram");
            }
            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", "diagram");
            writer.WriteAttributeString("id", "diagram");
            writer.WriteAttributeString("style", "height: " + (maxTop + height + 20).ToString() + "px;");
            foreach (DiagramElement element in elements)
            {
                writer.WriteStartElement("div");
                writer.WriteAttributeString("id", element.Id);
                writer.WriteAttributeString("class", "diagramElement " + element.Class);
                writer.WriteAttributeString("style", "position: absolute; " +
                    "left: " + (element.Left) + "px; " +
                    "top: " + element.Top + "px; " +
                    (element.Height == 0 ? "" : "height: " + element.Height.ToString()) + "px; " +
                    (element.Width == 0 ? "" : "width: " + element.Width.ToString()) + "px; "
                );
                if (element.Link == null)
                {
                    writer.WriteRaw(element.Label);
                }
                else
                {
                    DocTools.WriteElementLink(writer, element.Link, element.LinkFilter);
                }

                if (element.Children.Count > 0)
                {
                    WriteDiagram(writer, element.Children, false);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            if (isMain)
            {
                // Diagram section end
                writer.WriteEndElement();
            }
        }

        public static Dictionary<int, List<DiagramElement>> GroupDiagramLevels(List<DiagramElement> elements)
        {
            Dictionary<int, List<DiagramElement>> dict = new Dictionary<int, List<DiagramElement>>();
            // group by level
            foreach (DiagramElement element in elements)
            {
                if (!dict.ContainsKey(element.Top))
                {
                    dict.Add(element.Top, new List<DiagramElement>());
                }
                dict[element.Top].Add(element);
            }
            return dict;
        }
        
        public static string WriteExample(XmlWriter writer, IDocumentationService documentation, Guid exampleId)
        {
            string example = DocTools.Example(documentation, exampleId);
            string resultJson = "";
            string resultXml = "";

            if (example != "")
            {
                writer.WriteElementString(DocTools.SECTION_HEADING, "Example");
                writer.WriteElementString("pre", example);
            }
            // Example Json
            foreach (string exampleJson in DocTools.ExampleJson(documentation, exampleId))
            {
                writer.WriteElementString(DocTools.SECTION_HEADING, "JSON Example");
                DocTools.WriteCodeStart(writer, "example");
                writer.WriteRaw(DocTools.SyntaxHighlightJson(exampleJson));
                writer.WriteEndElement();
                resultJson = exampleJson;
            }
            // Example XML
            foreach (string exampleXml in DocTools.ExampleXml(documentation, exampleId))
            {
                writer.WriteElementString(DocTools.SECTION_HEADING, "XML Example");
                DocTools.WriteCodeString(writer, DocTools.GetPrettyPrintedXml(exampleXml));
                resultXml = exampleXml;
            }

            if (example != "") return example;
            if (resultJson != "") return resultJson;
            if (resultXml != "") return resultXml;
            return null;
        }  
    }
}
