using System;
using System.Xml;

namespace Origam.DA.Service
{
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
            foreach (object node in xmlNodeList)
            {
                string name = (node as XmlNode)?.Attributes?[$"x:{OrigamFile.TypeAttribute}"].Value 
                              ?? throw new Exception($"Could not read type form file: {xmlFileData.FileInfo.FullName} node: {node}");
                string idStr = (node as XmlNode).Attributes?["x:refId"].Value
                               ?? throw new Exception($"Could not read id form file: {xmlFileData.FileInfo.FullName} node: {node}");

                var folderUri = ElementNameFactory.Create(name);
                ParentFolderIds[folderUri] = new Guid(idStr);
            }
        }
    }
}