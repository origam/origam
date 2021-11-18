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
using Origam.Schema.EntityModel;
using System.Collections;

namespace Origam.Server.Doc
{
    public class DocTransformation : AbstractDoc
    {
        public DocTransformation(XmlWriter writer)
            : base(writer)
        {
        }

        public override string FilterName
        {
            get { return "transformation"; }
        }

        public override string Name
        {
            get { return "Transformations"; }
        }

        public override bool IsTocRecursive
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
            return ss.GetProvider(typeof(TransformationSchemaItemProvider));
        }

        public override List<DiagramConnection> WriteContent(string bodyElement, string elementId, XmlWriter writer, IDocumentationService documentation, IPersistenceService ps)
        {
            if (!CheckElementId(writer, elementId)) return null;

            AbstractTransformation transformation = ps.SchemaProvider.RetrieveInstance(typeof(AbstractTransformation), new ModelElementKey(new Guid(elementId))) as AbstractTransformation;
            #region body
            DocTools.WriteStartBody(bodyElement, writer, this.Title(elementId, documentation, ps), "Transformation", DocTools.ImagePath(transformation), transformation.Id);
            // summary
            WriteSummary(writer, documentation, transformation);
            // Packages
            WritePackages(writer, transformation);
            XslTransformation xslt = transformation as XslTransformation;
            if (xslt != null)
            {
                List<AbstractTransformation> dependentTransformations = new List<AbstractTransformation>();
                ArrayList dependencies =  xslt.GetDependencies(true);
                foreach (object item in dependencies)
                {
                    AbstractTransformation dependent = item as AbstractTransformation;
                    if (dependent != null)
                    {
                        dependentTransformations.Add(dependent);
                    }
                }

                if (dependentTransformations.Count > 0)
                {
                    DocTools.WriteSectionStart(writer, "Used Transformation Templates");
                    writer.WriteStartElement("ul");
                    foreach (XslTransformation dependent in dependentTransformations)
                    {
                        writer.WriteStartElement("li");
                        DocTools.WriteElementLink(writer, dependent, new DocTransformation(null).FilterName);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                DocTools.WriteSectionStart(writer, "Transformation");
                DocTools.WriteCodeString(writer, xslt.TextStore.Replace("\t", "  "));
                writer.WriteEndElement();
            }
            // examples
            WriteExample(writer, documentation, transformation.Id);
            // end body
            writer.WriteEndElement();
            #endregion
            return null;
        }
    }
}
