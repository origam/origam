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
using System.Xml;

namespace Origam.DA.Service;

public class ReferenceFileData: ObjectFileData
{
    public XmlFileData XmlFileData { get; }

    public ReferenceFileData(XmlFileData xmlFileData,
        IOrigamFileFactory origamFileFactory) : 
        base(new ParentFolders(), xmlFileData, origamFileFactory)
    {
            XmlFileData = xmlFileData;
            XmlNodeList xmlNodeList = xmlFileData
                                          .XmlDocument
                                          ?.SelectNodes("//x:groupReference",
                                              xmlFileData.NamespaceManager)
                                      ?? throw new Exception($"Could not find groupReference in: {xmlFileData.FileInfo.FullName}");
            foreach (XmlNode node in xmlNodeList)
            {
                string category = node?.Attributes?[$"x:{OrigamFile.TypeAttribute}"].Value 
                              ?? throw new Exception($"Could not read type form file: {xmlFileData.FileInfo.FullName} node: {node}");
                string idStr = node.Attributes?["x:refId"].Value
                               ?? throw new Exception($"Could not read id form file: {xmlFileData.FileInfo.FullName} node: {node}");

                var folderUri = category;
                ParentFolderIds[folderUri] = new Guid(idStr);
            }
        }
}