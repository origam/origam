using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Origam.Extensions;
using ProtoBuf;

namespace Origam.DA.Service
{
    [ProtoContract]
    public class TrackerSerializationData
    {
        public List<OrigamFile> GetOrigamFiles(OrigamFileFactory origamFileFactory) 
            => TransformBack(origamFileFactory);

        public Dictionary<string, int> ItemTrackerStats => itemTrackerStats;
        
        [ProtoMember(2)]
        private IDictionary<int,ElementName> ElementIdDictionary { get; }

        [ProtoMember(1)] 
        private List<OrigamFileSerializedForm> serializationList;
        
        [ProtoMember(3)] 
        private readonly AutoIncrementedIntIndex<Guid> guidIndex =
            new AutoIncrementedIntIndex<Guid>();
        
        [ProtoMember(4)] 
        private readonly AutoIncrementedIntIndex<ElementName> parentFolderIndex =
            new AutoIncrementedIntIndex<ElementName>();

        [ProtoMember(5)]
        private readonly Dictionary<string, int> itemTrackerStats;
        
        private TrackerSerializationData()
        {
        }

        public TrackerSerializationData(IEnumerable<OrigamFile> origamFiles, 
            Dictionary<string, int> itemTrackerStats)
        {
            AutoIncrementedIntIndex<ElementName> elementNameIdIndex =
                new AutoIncrementedIntIndex<ElementName>();
            origamFiles
                .SelectMany(x=>x.ContainedObjects.Values)
                .ForEach(x=> elementNameIdIndex.AddValueAndGetId(x.ElementName));
            ToSerializationForms(origamFiles, elementNameIdIndex.ValueToId);
            ElementIdDictionary = elementNameIdIndex.IdToValue;
            this.itemTrackerStats = itemTrackerStats;
        }

        private void ToSerializationForms(
            IEnumerable<OrigamFile> origamFiles, IDictionary<ElementName,int> idElementDictionary)
        {
            serializationList = origamFiles
                .Select(orFile =>
                    new OrigamFileSerializedForm(
                        orFile,
                        guidIndex,
                        parentFolderIndex, 
                        idElementDictionary))
                .ToList();
        }

        private List<OrigamFile> TransformBack(OrigamFileFactory origamFileFactory)
        {
            if (serializationList == null)
            {
                serializationList = new List<OrigamFileSerializedForm>();
            }
            return serializationList
                .Select(serForm =>
                    serForm.GetOrigamFile(
                        guidIndex, 
                        parentFolderIndex,
                        ElementIdDictionary,
                        origamFileFactory))
                .ToList();
        }

        public override string ToString()
        {
            string spacer =
                "\n----------------------------------------------------------------------------\n";
            return "TrackerSerializationdata:\n" +
                   "ElementIdDictionary: " + ElementIdDictionary.Print() +spacer+
                   "serializationList: [" +
                   serializationList
                       .Select(x => x.ToString())
                       .Aggregate("", (x, y) => $"{x}\n{y}")
                   + "]\n" +spacer+
                   "guidIndex: " + guidIndex + "\n" +spacer+
                   "parentFolderIndex: " + parentFolderIndex;
        }

        [ProtoContract]
        private class OrigamFileSerializedForm
        {
            [ProtoMember(1)]
            private string RelativePath{ get; }
            [ProtoMember(2)]
            private string FileHash { get; }
            [ProtoMember(3)]
            private List<ObjectInfoSerializedForm> ContainedObjInfos{ get;} 
                = new List<ObjectInfoSerializedForm>(); 
            [ProtoMember(4)]
            private IDictionary<int, int> ParentFolderIdsNums { get; }
                = new Dictionary<int, int>();

            private OrigamFileSerializedForm()
            {
            }

            public OrigamFileSerializedForm(OrigamFile origamFile,
                AutoIncrementedIntIndex<Guid> guidIndex,
                AutoIncrementedIntIndex<ElementName> parentFolderIndex,
                IDictionary<ElementName,int> idElementDictionary)
            {
                origamFile.ParentFolderIds.CheckIsValid(origamFile.Path);
                RelativePath = origamFile.Path.Relative;
                FileHash = origamFile.FileHash;
                ContainedObjInfos = origamFile.ContainedObjects.Values
                    .Select(objInfo => new ObjectInfoSerializedForm(
                        objInfo,
                        guidIndex,
                        idElementDictionary))
                    .ToList();
                if (ContainedObjInfos == null)
                {
                    throw new Exception( $"origamFile: {origamFile.Path.Absolute} contains no objects");
                }

                ParentFolderIdsNums = origamFile.ParentFolderIds
                    .ToDictionary(
                        entry => parentFolderIndex.AddValueAndGetId(entry.Key),
                        entry => guidIndex.AddValueAndGetId(entry.Value));
            }

            public OrigamFile GetOrigamFile(AutoIncrementedIntIndex<Guid> guidIndex,
                AutoIncrementedIntIndex<ElementName> parentFolderIndex,
                IDictionary<int,ElementName> elementIdDictionary, 
                OrigamFileFactory origamFileFactory)
            {
                OrigamFile origamFile = origamFileFactory.New( 
                    relativePath: RelativePath,
                    fileHash: FileHash,
                    parentFolderIds: ParentFolderIdsNums
                        .ToDictionary(
                            entry => parentFolderIndex[entry.Key],
                            entry => guidIndex[entry.Value]));

                origamFile = AddObjectInfo(guidIndex, origamFile, elementIdDictionary);
                return origamFile;
            }

            private OrigamFile AddObjectInfo(AutoIncrementedIntIndex<Guid> guidIndex,
                OrigamFile origamFile, IDictionary<int,ElementName> elementIdDictionary)
            {
                foreach (ObjectInfoSerializedForm objInfoSf in ContainedObjInfos)
                {
                    Guid guid = guidIndex[objInfoSf.IdNumber];
                    PersistedObjectInfo objInfo =
                        objInfoSf.GetObjectInfo(origamFile, 
                            guidIndex, elementIdDictionary);

                    origamFile.ContainedObjects.Add(guid, objInfo);
                }
                return origamFile;
            }
            
            public override string ToString()
            {
                return "OrigamFileSerializedForm:\n" +
                       "\tRelativePath: " + RelativePath + "\n" +
                       "\tFileHash: " + FileHash + "\n" +
                       "\tParentFolderIdsNums: " + ParentFolderIdsNums.Print() +
                       "\tContainedObjInfos: [" +
                       ContainedObjInfos
                           .Select(x => x.ToString())
                           .Aggregate("\t\t", (x, y) => $"{x}\n\t\t{y}")
                       + "]";
            }
        }
        
        [ProtoContract]
        private class ObjectInfoSerializedForm
        {
            [ProtoMember(1)]
            private readonly int elementNameId;
            [ProtoMember(2)]
            public int IdNumber { get; }
            [ProtoMember(3)]
            private bool IsFolder { get; }
            [ProtoMember(4)]
            private int ParentIdNumber { get; }

            private ObjectInfoSerializedForm()
            {
            }

            public ObjectInfoSerializedForm(PersistedObjectInfo objInfo,
                AutoIncrementedIntIndex<Guid> guidIndex, 
                IDictionary<ElementName,int> idElementDictionary)
            {
                elementNameId = idElementDictionary[objInfo.ElementName];
                IdNumber = guidIndex.AddValueAndGetId(objInfo.Id);
                ParentIdNumber = guidIndex.AddValueAndGetId(objInfo.ParentId);
                IsFolder = objInfo.IsFolder;
            }

            public PersistedObjectInfo GetObjectInfo(OrigamFile origamFile,
                AutoIncrementedIntIndex<Guid> guidIndex,
                IDictionary<int,ElementName> elementIdDictionary)
            {
                return 
                    new PersistedObjectInfo(
                        elementName: elementIdDictionary[elementNameId], 
                        id:guidIndex[IdNumber],
                        parentId:guidIndex[ParentIdNumber],
                        isFolder:IsFolder,
                        origamFile:origamFile);
            }
            public override string ToString()
            {
                return "ObjectInfoSerializedForm: " +
                       " elementNameId: " + elementNameId +
                       ", IdNumber: " + IdNumber +
                       ", IsFolder: " + IsFolder +
                       ", ParentIdNumber: " + ParentIdNumber;
            }
        }
    }
}