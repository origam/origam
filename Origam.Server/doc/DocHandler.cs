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
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Xml;

using core = Origam.Workbench.Services.CoreServices;
using Origam;
using Origam.Schema;
using Origam.Workbench.Services;
using System.Web.SessionState;

using log4net;
using Origam.UI;

namespace Origam.Server.Doc
{
    public class DocHandler : IHttpHandler, IRequiresSessionState
    {
        private static readonly ILog perfLog = LogManager.GetLogger("Performance");
        IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        private IList<AbstractDoc> _docGenerators = new List<AbstractDoc>();

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/xhtml+xml";
            if (perfLog.IsInfoEnabled)
            {
                perfLog.Info("Doc");
            }
            IDocumentationService documentation = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;

            string sectionName = context.Request.Params.Get(DocTools.PARAM_SECTION);
            string filterType = context.Request.Params.Get(DocTools.URL_FILTER_TYPE);
            string filterValue = context.Request.Params.Get(DocTools.URL_FILTER_VALUE);
            string objectType = context.Request.Params.Get(DocTools.PARAM_OBJECT_TYPE);

            using (XmlWriter writer = XmlWriter.Create(context.Response.OutputStream))
            {
                InitGenerators(writer);

                if (sectionName == null)
                {
                    WriteFrameset(writer);
                }
                else
                {
                    switch (sectionName)
                    {
                        case DocTools.SECTION_PACKAGES:
                            WritePackages(context, writer);
                            break;
                        case DocTools.SECTION_TOC:
                            WriteToc(writer, filterType, filterValue);
                            break;
                        case DocTools.SECTION_CONTENT:
                            WriteContent(objectType, documentation, filterType, filterValue, writer);
                            break;
                        default:
                            context.Response.StatusCode = 404;
                            break;
                    }
                }
                _docGenerators.Clear();
            }
        }

        private void InitGenerators(XmlWriter writer)
        {
            _docGenerators.Add(new DocMenu(writer));
            _docGenerators.Add(new DocEntity(writer));
            _docGenerators.Add(new DocDataStructure(writer));
            _docGenerators.Add(new DocScreen(writer));
            _docGenerators.Add(new DocSequentialWorkflow(writer));
            _docGenerators.Add(new DocStateWorkflow(writer));
            _docGenerators.Add(new DocWorkQueueClass(writer));
            _docGenerators.Add(new DocTransformation(writer));
            _docGenerators.Add(new DocApi(writer));
        }

        private void WriteToc(XmlWriter writer, string filterType, string filterValue)
        {
            DocTools.WriteStartDocument(writer, "Packages", false);
            // body
            writer.WriteStartElement("body");
            switch (filterType)
            {
                case DocTools.FILTER_TYPE_MODEL:
                    AbstractDoc resultDoc = null;
                    foreach (AbstractDoc doc in _docGenerators)
                    {
                        if (doc.FilterName == filterValue)
                        {
                            resultDoc = doc;
                            break;
                        }
                    }
                    if (resultDoc == null)
                    {
                        throw new ArgumentOutOfRangeException("filterValue", filterValue, "Unknown filter value.");
                    }
                    DocTools.WriteLink(writer, GetSectionPath(DocTools.SECTION_CONTENT, DocTools.FILTER_TYPE_MODEL, filterValue), "Complete Doc", null, null, "_blank");
                    resultDoc.WriteToc();
                    break;
                case DocTools.FILTER_TYPE_PACKAGE:
                    foreach (AbstractDoc packageDoc in _docGenerators)
                    {
                        packageDoc.WriteToc(new Guid(filterValue));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("filterType", filterType, "Unknown filter type.");
            }
            // end body
            writer.WriteEndElement();
            DocTools.WriteEndDocument(writer);
        }

        private void WritePackages(HttpContext context, XmlWriter writer)
        {
            DocTools.WriteStartDocument(writer, "Packages", false);
            // body
            writer.WriteStartElement("body");
            // ul
            writer.WriteStartElement("ul");
            foreach (AbstractDoc doc in _docGenerators)
            {
                DocTools.WriteTocElement(
                    writer, 
                    DocTools.GetSectionPath(
                        DocTools.SECTION_TOC,
                        DocTools.FILTER_TYPE_MODEL,
                        doc.FilterName),
                        (doc is DocMenu ? "" : "All ") + doc.Name,
                    DocTools.SECTION_TOC,
                    "package");
            }
            // end ul
            writer.WriteEndElement();
            writer.WriteElementString(DocTools.SECTION_HEADING, "Packages");
            writer.WriteStartElement("ul");
            List<SchemaExtension> packages = ps.SchemaProvider.RetrieveList<SchemaExtension>( null);
            packages.Sort();
            foreach (SchemaExtension package in packages)
            {
                DocTools.WriteTocElement(writer, DocTools.GetSectionPath(DocTools.SECTION_TOC, DocTools.FILTER_TYPE_PACKAGE, package.PrimaryKey["Id"].ToString()), package.Name, DocTools.SECTION_TOC, "package");
            }
            // end ul
            writer.WriteEndElement();
            // end body
            writer.WriteEndElement();
        }

        private void WriteContent(string objectType, IDocumentationService documentation, string filterType,
            string filterValue, XmlWriter writer)
        {
            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();

            if (filterType == DocTools.FILTER_TYPE_MODEL)
            {
                List<DiagramConnection> connections = new List<DiagramConnection>();
                AbstractDoc generator = GetGenerator(filterValue);
                Dictionary<IBrowserNode2, object> list = new Dictionary<IBrowserNode2, object>();
                generator.GetList(list, generator.RootProvider(), auth, false, Guid.Empty);
                DocTools.WriteStartDocument(writer, "ORIGAM Documentation", generator.UsePrettyPrint);
                writer.WriteStartElement(DocTools.BODY);
                generator.WriteToc();
                WriteCompleteContent(writer, ps, documentation, auth, generator, list, connections);
                writer.WriteEndElement();
                DocTools.WriteEndDocument(writer, generator.UsePrettyPrint, generator.UseDiagrams, generator.ConnectorType, connections);
            }
            else if (objectType == null || filterValue == null)
            {
                DocTools.WriteStartDocument(writer, "ORIGAM Documentation", false);
                writer.WriteStartElement(DocTools.BODY);
                DocTools.WriteDivString(writer, "emptyContent", "<< Please select a category from the left menu.");
                // end body
                writer.WriteEndElement();
                // end document
                writer.WriteEndElement();
            }
            else
            {
                AbstractDoc generator = GetGenerator(objectType);
                DocTools.WriteStartDocument(writer, generator.Title(filterValue, documentation, ps), generator.UsePrettyPrint);
                List<DiagramConnection> connections = generator.WriteContent(DocTools.BODY, filterValue, writer, documentation, ps);
                DocTools.WriteEndDocument(writer, generator.UsePrettyPrint, generator.UseDiagrams, generator.ConnectorType, connections);
            }
        }

        private static void WriteCompleteContent(XmlWriter writer, IPersistenceService ps, IDocumentationService documentation,
            IOrigamAuthorizationProvider auth, AbstractDoc generator, Dictionary<IBrowserNode2, object> list, List<DiagramConnection> connections)
        {
            foreach (KeyValuePair<IBrowserNode2, object> child in list)
            {
                AbstractSchemaItem item = child.Key as AbstractSchemaItem;
                SchemaItemGroup folder = child.Key as SchemaItemGroup;

                if (generator.IsFolder(child.Key))
                {
                    Dictionary<IBrowserNode2, object> children = child.Value as Dictionary<IBrowserNode2, object>;
                    WriteCompleteContent(writer, ps, documentation, auth, generator, children, connections);
                }
                else
                {
                    List<DiagramConnection> newConnections = generator.WriteContent(DocTools.DIV, item.Id.ToString(), writer, documentation, ps);
                    if (newConnections != null)
                    {
                        connections.AddRange(newConnections);
                    }
                }
            }
        }

        private AbstractDoc GetGenerator(string objectType)
        {
            foreach (AbstractDoc g in _docGenerators)
            {
                if (g.FilterName == objectType)
                {
                    return g;
                }
            }
            return null;
        }

        private void WriteFrameset(XmlWriter writer)
        {
            DocTools.WriteStartDocument(writer, "ORIGAM Application Documentation", false);
            #region horizontal frameset
            writer.WriteStartElement("frameset");
            writer.WriteAttributeString("cols", "20%, 80%");
            writer.WriteAttributeString("bordercolor", "#999");
            #region vertical frameset
            writer.WriteStartElement("frameset");
            writer.WriteAttributeString("rows", "30%, 70%");
            writer.WriteAttributeString("bordercolor", "#999");
            DocTools.WriteFrame(writer, GetSectionPath(DocTools.SECTION_PACKAGES), "packageList", "Packages");
            DocTools.WriteFrame(writer, GetSectionPath(DocTools.SECTION_TOC, DocTools.FILTER_TYPE_MODEL, new DocMenu(null).FilterName), "toc", "Contents");
            writer.WriteEndElement();
            #endregion
            DocTools.WriteFrame(writer, GetSectionPath(DocTools.SECTION_CONTENT), "content", "Documentation Content");
            writer.WriteEndElement();
            #endregion
            DocTools.WriteEndDocument(writer);
        }

        private string GetSectionPath(string section, string filterType, string filterValue)
        {
            return DocTools.DOC_ROOT
                + "?" + DocTools.PARAM_SECTION + "=" + section
                + "&" + DocTools.URL_FILTER_TYPE + "=" + filterType
                + "&" + DocTools.URL_FILTER_VALUE + "=" + filterValue;
                ;
        }

        private string GetSectionPath(string section)
        {
            return DocTools.DOC_ROOT + "?" + DocTools.PARAM_SECTION + "=" + section;
        }
        #endregion
    }
}
