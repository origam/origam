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

#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using Origam.Workbench.Services;
using Origam.Schema.GuiModel;
using System.Collections;
using Origam.Schema.WorkflowModel;
using Origam;

namespace Origam.Server.Doc
{
    public class DocApi : AbstractDoc
    {
        public DocApi(XmlWriter writer)
            : base(writer)
        {
        }

        public override string FilterName
        {
            get { return "api"; }
        }

        public override string Name
        {
            get { return "APIs"; }
        }

        public override bool UseShortHelpAsName
        {
            get
            {
                return true;
            }
        }

        public override bool UsePrettyPrint
        {
            get
            {
                return true;
            }
        }

        public override ISchemaItemProvider RootProvider()
        {
            SchemaService ss = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            return ss.GetProvider(typeof(PagesSchemaItemProvider));
        }

        public override string Title(string elementId, IDocumentationService documentation, IPersistenceService ps)
        {
            AbstractPage page = ps.SchemaProvider.RetrieveInstance(typeof(AbstractPage), new ModelElementKey(new Guid(elementId))) as AbstractPage;
            return DocTools.Name(documentation, page.Id, page.Name);
        }

        public override List<DiagramConnection> WriteContent(string bodyElement, string elementId, XmlWriter writer, IDocumentationService documentation, IPersistenceService ps)
        {
            if (!CheckElementId(writer, elementId)) return null;

            MarkdownSharp.Markdown md = new MarkdownSharp.Markdown();
            AbstractPage page = ps.SchemaProvider.RetrieveInstance(typeof(AbstractPage), new ModelElementKey(new Guid(elementId))) as AbstractPage;
            XsltDataPage xsltPage = page as XsltDataPage;

            #region body
            DocTools.WriteStartBody(bodyElement, writer, this.Title(elementId, documentation, ps), "API", DocTools.ImagePath(page), page.Id);
            // summary
            WriteSummary(writer, documentation, page);
            // Packages
            WritePackages(writer, page);
            // Roles
            WriteRoles(writer, page);
            // Data Source
            WriteDataSource(writer, page);
            // Transformations
            WriteTransformations(writer, xsltPage);
            // Return Type
            DocTools.WriteSectionStart(writer, "Return Type");
            writer.WriteElementString("p", page.MimeType);
            writer.WriteEndElement();
            // URL
            writer.WriteElementString(DocTools.SECTION_HEADING, "URL");
            #region code
            DocTools.WriteCodeStart(writer, "url");
            writer.WriteRaw(DocTools.SyntaxHighlightUrl(page.Url));
            #endregion
            writer.WriteEndElement();
            // Verbs
            string verbs = Verbs(page);
            writer.WriteElementString(DocTools.SECTION_HEADING, "Verbs");
            writer.WriteElementString("p", verbs);

            Hashtable exampleParams = new Hashtable();
            Dictionary<string, List<PageParameterMapping>> mappings = new Dictionary<string, List<PageParameterMapping>>();
            foreach (PageParameterMapping inputParameter in page.ChildItemsByType(PageParameterMapping.CategoryConst))
            {
                if (inputParameter.MappedParameter == null) continue;
                if (!mappings.ContainsKey(inputParameter.MappedParameter))
                {
                    mappings.Add(inputParameter.MappedParameter, new List<PageParameterMapping>());
                }
                mappings[inputParameter.MappedParameter].Add(inputParameter);
            }

            if (mappings.Count > 0)
            {
                // Parameters
                writer.WriteElementString(DocTools.SECTION_HEADING, "Parameters");
                foreach (KeyValuePair<string, List<PageParameterMapping>> mappingKey in mappings)
                {
                    DocTools.WriteDivStart(writer, "parameter");
                    DocTools.WriteDivString(writer, "parameterName", 
                        (mappingKey.Key != "") ? mappingKey.Key : bodyElement);                    
                    foreach (PageParameterMapping mapping in mappingKey.Value)
                    {
                        writer.WriteRaw(md.Transform(DocTools.LongHelp(documentation, mapping.Id)));
                        if (mapping.DefaultValue != null)
                        {
                            writer.WriteElementString("p", String.Format("Default: {0} ({1})",
                                mapping.DefaultValue.Name,
                                DocTools.GetConstantValue(mapping.DataConstantId)));
                        }
                        string example = WriteExample(writer, documentation, mapping.Id);
                        if (mappingKey.Key != "") exampleParams[mapping.MappedParameter] = example;
                    }
                    writer.WriteEndElement();   // </div class="parameter">
                }
            }

            try
            {
                string url = HttpTools.BuildUrl(page.Url, exampleParams, false, null, false);
                DocTools.WriteSectionStart(writer, "Test");
                writer.WriteStartElement("a");
                writer.WriteAttributeString("href", url);
                writer.WriteString(url);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            catch {}
            

            // Example
            WriteExample(writer, documentation, page.Id);
            // end body
            writer.WriteEndElement();
            #endregion
            return null;
        }

        private static void WriteTransformations(XmlWriter writer, XsltDataPage xsltPage)
        {
            if (xsltPage != null)
            {
                // Transformation
                if (xsltPage.Transformation != null)
                {
                    DocTools.WriteSectionStart(writer, "Transformation");
                    writer.WriteStartElement("p");
                    writer.WriteString("Transformation ");
                    DocTools.WriteElementLink(writer, xsltPage.Transformation, new DocTransformation(null).FilterName);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
                // LogTransformation
                if (xsltPage.LogTransformation != null)
                {
                    DocTools.WriteSectionStart(writer, "Log Transformation");
                    writer.WriteStartElement("p");
                    writer.WriteString("Transformation ");
                    DocTools.WriteElementLink(writer, xsltPage.LogTransformation, new DocTransformation(null).FilterName);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
            }
        }

        private void WriteDataSource(XmlWriter writer, AbstractPage page)
        {
            DocTools.WriteSectionStart(writer, "Data Source");

            XsltDataPage xsltPage = page as XsltDataPage;
            WorkflowPage wfPage = page as WorkflowPage;
            FileDownloadPage filePage = page as FileDownloadPage;
            if (xsltPage != null)
            {
                writer.WriteStartElement("p");
                if (xsltPage.DataStructure == null)
                {
                    writer.WriteElementString("b", "None");
                }
                else
                {
                    writer.WriteString("Data Structure ");
                    DocTools.WriteElementLink(writer, xsltPage.DataStructure, new DocDataStructure(null).FilterName);
                    if (xsltPage.Method != null)
                    {
                        writer.WriteString(", Method " + xsltPage.Method);
                    }
                }
                writer.WriteEndElement();
            }
            else if (wfPage != null)
            {
                writer.WriteStartElement("p");
                writer.WriteString("Sequential Workflow ");
                DocTools.WriteElementLink(writer, wfPage.Workflow, new DocSequentialWorkflow(null).FilterName);
                writer.WriteEndElement();
            }
            else if (filePage != null)
            {
                writer.WriteStartElement("p");
                writer.WriteString("File Download ");
                DocTools.WriteElementLink(writer, filePage.DataStructure, new DocDataStructure(null).FilterName);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static string Verbs(AbstractPage page)
        {
            string verbs = "";
            XsltDataPage dataPage = page as XsltDataPage;
            FileDownloadPage downloadPage = page as FileDownloadPage;
            WorkflowPage workflowPage = page as WorkflowPage;
            if (dataPage != null)
            {
                verbs = "GET, POST";
                if (dataPage.AllowPUT) verbs += ", PUT";
                if (dataPage.AllowDELETE) verbs += ", DELETE";
            }
            else if (workflowPage != null)
            {
                verbs = "GET, POST";
            }
            else if (downloadPage != null)
            {
                verbs = "GET";
            }
            return verbs;
        }
    }
}
