#region license
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
using System.Text;
using System.Xml;
using Origam.Workbench.Services;
using System.Text.RegularExpressions;
using Origam.Schema;
using Origam.UI;
using System.Web;
using Newtonsoft.Json;


namespace Origam.Server.Doc
{
    public class DocTools
    {
        public const string BODY = "body";
        public const string DIV = "div";
        public const string DOC_HEADING = "h1";
        public const string SECTION_HEADING = "h2";
        public const string SECTION_PACKAGES = "packages";
        public const string SECTION_TOC = "toc";
        public const string SECTION_CONTENT = "content";
        public const string DOC_ROOT = "doc.aspx";
        public const string PARAM_SECTION = "section";
        public const string PARAM_ELEMENT = "elementId";
        public const string PARAM_OBJECT_TYPE = "objectType";
        public const string URL_FILTER_TYPE = "filterType";
        public const string URL_FILTER_VALUE = "filterValue";
        public const string FILTER_TYPE_PACKAGE = "package";
        public const string FILTER_TYPE_MODEL = "model";
        public const string DOC_MODEL_IMAGE_PATH = "assets/doc/model";

        public static void WriteStartDocument(XmlWriter writer, string title, bool prettify)
        {
            writer.WriteStartDocument();
            writer.WriteDocType("html", null, null, null);
            writer.WriteStartElement("html", "http://www.w3.org/1999/xhtml");
            writer.WriteAttributeString("xmlns", "v", null, "urn:schemas-microsoft-com:vml");
            // head
            writer.WriteStartElement("head");
            writer.WriteElementString("title", title);
            if (prettify)
            {
                WriteCss(writer, "css/prettify.css");
            }
            WriteCss(writer, "css/doc.css");
            // end head
            writer.WriteEndElement();
        }

        public static void WriteStartBody(string bodyElement, XmlWriter writer, string title, string subtitle, string icon, Guid id)
        {
            writer.WriteStartElement(bodyElement);
            writer.WriteAttributeString("class", "body");
            DocTools.WriteDivString(writer, "modelElementId", id.ToString());
            DocTools.WriteDivStart(writer, "subtitle");
            DocTools.WriteImage(writer, icon);
            writer.WriteString(subtitle);
            writer.WriteEndElement();
            writer.WriteElementString(DOC_HEADING, title);
        }

        public static void WriteEndDocument(XmlWriter writer)
        {
            WriteEndDocument(writer, false);
        }

        public static void WriteEndDocument(XmlWriter writer, bool prettify)
        {
            WriteEndDocument(writer, prettify, false, DiagramConnectorType.Flowchart, null);
        }

        public static void WriteEndDocument(XmlWriter writer, bool prettify, bool diagrams,
            DiagramConnectorType connectorType, List<DiagramConnection> connections)
        {
            if (prettify || diagrams)
            {
                WriteScript(writer, "js/jquery-1.9.1.min.js");
            }

            if (prettify)
            {
                WriteScript(writer, "js/prettify.js");
                WriteScript(writer, "js/lang-sql.js");
                writer.WriteStartElement("script");
                writer.WriteAttributeString("type", "text/javascript");
                writer.WriteString("(function(jQuery){jQuery( document ).ready( function() {prettyPrint();} );}(jQuery))");
                writer.WriteEndElement();
            }
            if (diagrams)
            {
                WriteScript(writer, "js/jquery-ui.min.js");
                WriteScript(writer, "js/jquery.jsPlumb-1.5.5.min.js");

                string diagramJs =
                    "jsPlumb.ready(function () {" +
                        "var instance = jsPlumb.getInstance({" +
                            "Endpoint: [\"Dot\", { radius: 3}]," +
                            "HoverPaintStyle: { strokeStyle: \"red\", lineWidth: 5 }," +
                            "Container: \"diagram\"," +
                            "Connector:[ \"" + connectorType.ToString() + "\", { cornerRadius:2 } ]," +
                            "PaintStyle:{ strokeStyle:\"#5c96bc\", lineWidth:2, outlineColor:\"transparent\", outlineWidth:4 }," +
                            "ConnectionOverlays: [" +
                                "[\"Arrow\", {location: 1, id: \"arrow\", length: 14, foldback: 0.8" +
                                "}]," +
                                "]" +
                            "});" +
                        "instance.importDefaults({ConnectionsDetachable: false});" +
                        "instance.doWhileSuspended(function() {";

                int connectionNumber = 0;
                foreach (DiagramConnection connection in connections)
                {
                    diagramJs +=
                        "var ep" + connectionNumber.ToString() + " = instance.addEndpoint(\"" +
                            connection.Source + "\", {anchor:[" +
                            connection.SourceAnchor() + "],});" +
                        "instance.connect({" +
                        "source: ep" + connectionNumber.ToString() + "," +
                        "target: \"" + connection.Target + "\"," +
                        "connector: [\"" + connectorType.ToString() + "\",{curviness: " + connection.Curviness.ToString() + "}]," +
                        "anchors: [" + connection.SourceAnchor() + ", " + connection.TargetAnchor() + "],";

                    if (!string.IsNullOrEmpty(connection.Label))
                    {
                        diagramJs += "overlays:[" +
                            "[\"Label\", { label: \"" + connection.Label + "\", id: \"" + connection.Id + "\", cssClass: \"label\"}]" +
                            "]";
                    }
                    diagramJs += "});";
                    connectionNumber++;
                }

                diagramJs += "});";
                diagramJs += "});";

                writer.WriteStartElement("script");
                writer.WriteAttributeString("type", "text/javascript");
                writer.WriteString(diagramJs);
                writer.WriteEndElement();
            }
            // end html
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        public static void WriteScript(XmlWriter writer, string script)
        {
            writer.WriteStartElement("script");
            writer.WriteAttributeString("src", script);
            writer.WriteAttributeString("type", "text/javascript");
            writer.WriteEndElement();
        }

        public static void WriteCss(XmlWriter writer, string css)
        {
            writer.WriteStartElement("link");
            writer.WriteAttributeString("href", css);
            writer.WriteAttributeString("rel", "stylesheet");
            writer.WriteAttributeString("type", "text/css");
            writer.WriteEndElement();
        }

        public static void WriteDivStart(XmlWriter writer, string className)
        {
            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", className);
        }

        public static void WriteSectionStart(XmlWriter writer, string sectionName)
        {
            WriteDivStart(writer, "section");
            writer.WriteElementString(SECTION_HEADING, sectionName);
        }

        public static void WriteDivString(XmlWriter writer, string className, string text)
        {
            WriteDivStart(writer, className);
            writer.WriteString(text);
            writer.WriteEndElement();
        }

        public static void WriteCodeStart(XmlWriter writer, string className)
        {
            writer.WriteStartElement("pre");
            writer.WriteAttributeString("class", className);
        }

        public static void WriteCodeString(XmlWriter writer, string text)
        {
            WriteCodeStart(writer, "example prettyprint");
            writer.WriteString(text);
            writer.WriteEndElement();
        }

        public static string LongHelp(IDocumentationService documentation, Guid id)
        {
            return documentation.GetDocumentation(id, DocumentationType.USER_LONG_HELP);
        }

        public static string ShortHelp(IDocumentationService documentation, Guid id)
        {
            return documentation.GetDocumentation(id, DocumentationType.USER_SHORT_HELP);
        }

        public static string Example(IDocumentationService documentation, Guid id)
        {
            return documentation.GetDocumentation(id, DocumentationType.EXAMPLE);
        }

        public static List<string> ExampleJson(IDocumentationService documentation, Guid id)
        {
            return GetDocumentationMultiple(documentation, id, DocumentationType.EXAMPLE_JSON);
        }        

        public static List<string> ExampleXml(IDocumentationService documentation, Guid id)
        {
            return GetDocumentationMultiple(documentation, id, DocumentationType.EXAMPLE_XML);
        }

        public static string GetConstantValue(Guid constantId)
        {
            IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
            return (string)parameterService.GetParameterValue(constantId, OrigamDataType.String);
        }

        private static List<string> GetDocumentationMultiple(IDocumentationService documentation, Guid id, DocumentationType category)
        {
            List<string> result = new List<string>();
            DocumentationComplete doc = documentation.LoadDocumentation(id);
            foreach (DocumentationComplete.DocumentationRow row in doc.Documentation.Rows)
            {
                if (row.Category == category.ToString())
                {
                    result.Add((string)row.Data);
                }
            }
            return result;
        }

        public static string SyntaxHighlightJson(string original)
        {
            string pretty = original;
            try
            {
                pretty = GetPrettyPrintedJson(original);
            }
            catch { }

            MatchEvaluator eval = new MatchEvaluator(PrettJsonEvaluator);
            return Regex.Replace(
              pretty,
              @"(¤(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\¤])*¤(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)".Replace('¤', '"'), eval);
        }

        public static string SyntaxHighlightUrl(string original)
        {
            MatchEvaluator eval = new MatchEvaluator(PrettUrlEvaluator);
            return Regex.Replace(
              original,
              @"\{[^\}]*\}",
              eval);
        }

        public static string PrettUrlEvaluator(Match match)
        {
            return "<span class=\"urlParameter\">" + match + "</span>";
        }

        public static string PrettJsonEvaluator(Match match)
        {
            string cls = "lit";
            if (Regex.IsMatch(match.Value, @"^¤".Replace('¤', '"')))
            {
                if (Regex.IsMatch(match.Value, ":$"))
                {
                    cls = "atn";
                }
                else
                {
                    cls = "atv";
                }
            }
            else if (Regex.IsMatch(match.Value, "true|false|null"))
            {
                cls = "kwd";
            }
            return "<span class=\"" + cls + "\">" + match + "</span>";
        }

        public static string GetPrettyPrintedJson(string json)
        {
            object parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Newtonsoft.Json.Formatting.Indented);
        }

        public static string GetPrettyPrintedXml(string xml)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                StringBuilder sb = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "  ";
                settings.NewLineChars = "\r\n";
                settings.NewLineHandling = NewLineHandling.Replace;
                settings.Encoding = Encoding.UTF8;
                settings.OmitXmlDeclaration = true;
                using (XmlWriter writer = XmlWriter.Create(sb, settings))
                {
                    doc.Save(writer);
                }
                return sb.ToString();
            }
            catch { }

            return xml;
        }

        public static string Name(IDocumentationService documentation, Guid id, string name)
        {
            string shortHelp = ShortHelp(documentation, id);
            if (shortHelp != "" && shortHelp != null)
            {
                name = shortHelp;
            }
            return name;
        }

        public static void WriteFrame(XmlWriter writer, string src, string name, string title)
        {
            writer.WriteStartElement("frame");
            writer.WriteAttributeString("src", src);
            writer.WriteAttributeString("name", name);
            writer.WriteAttributeString("title", title);
            writer.WriteEndElement();
        }

        public static void WriteTocElement(XmlWriter writer, ISchemaItem item, string target, string extraClass, string filterName)
        {
            string cssClass = "tocref";
            if (extraClass != null)
            {
                cssClass += " " + extraClass;
            }
            writer.WriteStartElement("li");
            writer.WriteAttributeString("class", cssClass);
            WriteElementLink(writer, item, true, filterName, null, target);            
            writer.WriteEndElement();
        }

        public static void WriteTocElement(XmlWriter writer, string href, string name, string target, string extraClass)
        {
            string cssClass = "tocref";
            if (extraClass != null)
            {
                cssClass += " " + extraClass;
            }
            writer.WriteStartElement("li");
            writer.WriteAttributeString("class", cssClass);
            WriteLink(writer, href, name, null, null, target);
            writer.WriteEndElement();
        }

        public static void WriteElementLink(XmlWriter writer, ISchemaItem item, string filterName)
        {
            WriteElementLink(writer, item, true, filterName, null, null);
        }

        public static void WriteElementLink(XmlWriter writer, ISchemaItem item, bool absolute, string filterName)
        {
            WriteElementLink(writer, item, absolute, filterName, null, null);
        }

        public static void WriteElementLink(XmlWriter writer, ISchemaItem item, bool absolute, string filterName, string cssClass, string target)
        {
            string href;
            if (absolute)
            {
                href = GetElementPath(item, filterName);
            }
            else
            {
                href = "#" + item.PrimaryKey["Id"].ToString();
            }
            WriteLink(writer, href, item.NodeText, ImagePath(item), cssClass, target); 
        }

        public static string ImagePath(IBrowserNode2 item)
        {
            return DOC_MODEL_IMAGE_PATH + item.Icon + ".png";
        }

        public static void WriteLink(XmlWriter writer, string href, string name, string imgSrc, string cssClass, string target)
        {
            writer.WriteStartElement("a");
            if (cssClass != null) writer.WriteAttributeString("class", cssClass);
            writer.WriteAttributeString("href", href);
            if (target != null) writer.WriteAttributeString("target", target);
            if (imgSrc != null)
            {
                WriteImage(writer, imgSrc);
            }
            writer.WriteString(name);
            writer.WriteEndElement();
        }

        public static void WriteImage(XmlWriter writer, string imgSrc)
        {
            writer.WriteStartElement("img");
            writer.WriteAttributeString("src", imgSrc);
            writer.WriteEndElement();
        }

        public static string ImageLink(ISchemaItem item, string filterName)
        {
            return "<a href=\"" + HttpUtility.HtmlEncode(DocTools.GetElementPath(item, filterName)) + "\">"
                + "<img src=\"" + DocTools.ImagePath(item) + "\"/>"
                + item.NodeText + "</a>";
        }

        public static string Image(ISchemaItem item, string className)
        {
            return "<img class=\"" + className + "\" src=\"" + DocTools.ImagePath(item) + "\"/>";
        }

        public static string GetElementPath(ISchemaItem item, string filterName)
        {
            return GetSectionPath(DocTools.SECTION_CONTENT, DocTools.PARAM_ELEMENT, item.PrimaryKey["Id"].ToString())
                                    + "&" + DocTools.PARAM_OBJECT_TYPE + "=" + filterName;
        }

        public static string GetSectionPath(string section, string filterType, string filterValue)
        {
            return DocTools.DOC_ROOT
                + "?" + PARAM_SECTION + "=" + section
                + "&" + URL_FILTER_TYPE + "=" + filterType
                + "&" + URL_FILTER_VALUE + "=" + filterValue;
        }

        public static void WriteTableHeader(XmlWriter writer, List<string> columnNames)
        {
            writer.WriteStartElement("tr");
            foreach (string columnName in columnNames)
            {
                writer.WriteElementString("th", columnName);
            }
            writer.WriteEndElement();
        }

        public static void WriteError(XmlWriter writer, Exception ex)
        {
            DocTools.WriteDivString(writer, "error", ex.Message);
        }
    }
}
