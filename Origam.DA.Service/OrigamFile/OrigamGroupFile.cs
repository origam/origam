using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Origam.DA.ObjectPersistence;

namespace Origam.DA.Service
{
    class OrigamGroupFile : OrigamFile
    {
        public OrigamGroupFile(OrigamPath path, IDictionary<ElementName, Guid> parentFolderIds,
            OrigamFileManager origamFileManager, OrigamPathFactory origamPathFactory,
            FileEventQueue fileEventQueue, bool isAFullyWrittenFile = false) 
            : base(path, parentFolderIds, origamFileManager,origamPathFactory, fileEventQueue, isAFullyWrittenFile)
        {
        }

        public OrigamGroupFile(OrigamPath path, IDictionary<ElementName, Guid> parentFolderIds,
            OrigamFileManager origamFileManager, OrigamPathFactory origamPathFactory,
            FileEventQueue fileEventQueue, string fileHash) 
            : base(path, parentFolderIds, origamFileManager,origamPathFactory, fileEventQueue, fileHash)
        {
        }
        
        protected override void MakeNewReferenceFileIfNeeded(DirectoryInfo directory)
        {
        }
        
        public override void WriteInstance(IFilePersistent instance,
            ElementName elementName)
        {
            XmlNode contentNode = DeferredSaveDocument.ChildNodes[1];
            bool anotherGroupPresent = contentNode.ChildNodes
                .Cast<XmlNode>()
                .Where(node => node.Name == "group")
                .Any(node => Guid.Parse(node.Attributes["x:id"].Value) != instance.Id);

            if (anotherGroupPresent)
            {
                throw new InvalidOperationException("Single .origamGroup file can contain only one group");
            }
            base.WriteInstance(instance, elementName);
        }
    }
}