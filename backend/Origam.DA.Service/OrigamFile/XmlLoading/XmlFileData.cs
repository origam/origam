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

using System.IO;
using System.Xml;
using Origam.DA.Common;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.DA.Service.NamespaceMapping;
using Origam.Extensions;
using Origam.Schema;

namespace Origam.DA.Service;

public class XmlFileData
{
    public OrigamXmlDocument XmlDocument { get; }
    public XmlNamespaceManager NamespaceManager{ get; }
    public FileInfo FileInfo { get;}

    public XmlFileData(OrigamXmlDocument xmlDocument, FileInfo fileInfo)
    {
            XmlDocument = xmlDocument;
            FileInfo = fileInfo;
            NamespaceManager = new XmlNamespaceManager(XmlDocument.NameTable);       
            NamespaceManager.AddNamespace("x",OrigamFile.ModelPersistenceUri);
            NamespaceManager.AddNamespace(
                XmlNamespaceTools.GetXmlNamespaceName(typeof(Package)),OrigamFile.PackageUri);
            NamespaceManager.AddNamespace(
                XmlNamespaceTools.GetXmlNamespaceName(typeof(SchemaItemGroup)),OrigamFile.GroupUri);
        }

    public XmlFileData(XFileData xFileData)
        :this(
            new OrigamXmlDocument(xFileData.Document.XDocument),
            xFileData.File)
    {
           
        }
}