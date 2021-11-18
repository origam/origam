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
using System.Reflection;
using System.IO;
using Origam.Utils;

namespace Origam.Server.Doc
{
    public class DocScreen : AbstractDoc
    {
        public DocScreen(XmlWriter writer)
            : base(writer)
        {
        }

        public override string FilterName
        {
            get { return "screen"; }
        }

        public override string Name
        {
            get { return "Screens"; }
        }

        public override bool CanExtendItems
        {
            get
            {
                return false;
            }
        }

        public override ISchemaItemProvider RootProvider()
        {
            SchemaService ss = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            return ss.GetProvider(typeof(FormSchemaItemProvider));
        }

        public override List<DiagramConnection> WriteContent(string bodyElement, string elementId, XmlWriter writer, IDocumentationService documentation, IPersistenceService ps)
        {
            if (!CheckElementId(writer, elementId)) return null;

            FormControlSet form = ps.SchemaProvider.RetrieveInstance(typeof(FormControlSet), new ModelElementKey(new Guid(elementId))) as FormControlSet;
            #region body
            DocTools.WriteStartBody(bodyElement, writer, this.Title(elementId, documentation, ps), "Screen", DocTools.ImagePath(form), form.Id);
            // summary
            WriteSummary(writer, documentation, form);
            // Packages
            WritePackages(writer, form);
            // Data Source
            WriteDataSource(writer, form);
            // Screen sections
            new DocProcessor(writer, documentation, ps).Screen(form, getAssemblyXslt());
            // end body
            writer.WriteEndElement();
            #endregion
            return null;
        }

        public static void WriteDataSource(XmlWriter writer, FormControlSet form)
        {
            DocTools.WriteSectionStart(writer, "Data Source");
            writer.WriteString("Data Structure ");
            DocTools.WriteElementLink(writer, form.DataStructure, new DocDataStructure(null).FilterName);
            writer.WriteEndElement();
        }
        public string getAssemblyXslt()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("Origam.Server.doc.styledoc.xsl");
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
