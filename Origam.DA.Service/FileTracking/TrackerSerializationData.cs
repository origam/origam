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
        public List<ITrackeableFile> GetOrigamFiles(OrigamFileFactory origamFileFactory) 
            => TransformBack(origamFileFactory);
        
        public Dictionary<string, int> ItemTrackerStats => itemTrackerStats;
        
        [ProtoMember(2)]
        private IDictionary<int,string> CategoryIdDictionary { get; }

        [ProtoMember(1)] 
        private List<OrigamFileSerializedForm> serializationList;
        
        [ProtoMember(3)] 
        private readonly AutoIncrementedIntIndex<Guid> guidIndex =
            new AutoIncrementedIntIndex<Guid>();
        
        [ProtoMember(4)] 
        private readonly AutoIncrementedIntIndex<string> parentFolderIndex =
            new AutoIncrementedIntIndex<string>();

        [ProtoMember(5)]
        private readonly Dictionary<string, int> itemTrackerStats;

        [ProtoMember(6)]
        private readonly AutoIncrementedIntIndex<TypeInfo> typeIndex 
            = new AutoIncrementedIntIndex<TypeInfo>();

        public IEnumerable<TypeInfo> PersistedTypeInfos =>
            typeIndex.IdToValue.Values;
       
        private TrackerSerializationData()
        {
        }

        public TrackerSerializationData(IEnumerable<ITrackeableFile> origamFiles, 
            Dictionary<string, int> itemTrackerStats)
        {
            AutoIncrementedIntIndex<string> elementNameIdIndex =
                new AutoIncrementedIntIndex<string>();
            origamFiles
                .SelectMany(x=>x.ContainedObjects.Values)
                .ForEach(x=> elementNameIdIndex.AddValueAndGetId(x.Category));
            ToSerializationForms(origamFiles, elementNameIdIndex.ValueToId);
            CategoryIdDictionary = elementNameIdIndex.IdToValue;
            this.itemTrackerStats = itemTrackerStats;
        }

        private void ToSerializationForms(
            IEnumerable<ITrackeableFile> origamFiles, IDictionary<string,int> idElementDictionary)
        {
            serializationList = origamFiles
                .Select(orFile =>
                    new OrigamFileSerializedForm(
                        orFile,
                        guidIndex,
                        parentFolderIndex, 
                        idElementDictionary,
                        typeIndex))
                .ToList();
        }

        private List<ITrackeableFile> TransformBack(OrigamFileFactory origamFileFactory)
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
                        CategoryIdDictionary,
                        origamFileFactory,
                        typeIndex))
                .ToList();
        }

        public override string ToString()
        {
            string spacer =
                "\n----------------------------------------------------------------------------\n";
            return "TrackerSerializationdata:\n" +
                   "ElementIdDictionary: " + CategoryIdDictionary.Print() +spacer+
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

            public OrigamFileSerializedForm(ITrackeableFile origamFile,
                AutoIncrementedIntIndex<Guid> guidIndex,
                AutoIncrementedIntIndex<string> parentFolderIndex,
                IDictionary<string, int> idCategoryDictionary,
                AutoIncrementedIntIndex<TypeInfo> typeIndex)
            {
                origamFile.ParentFolderIds.CheckIsValid(origamFile.Path);
                RelativePath = origamFile.Path.Relative;
                FileHash = origamFile.FileHash;
                ContainedObjInfos = origamFile.ContainedObjects.Values
                    .Select(objInfo => new ObjectInfoSerializedForm(
                        objInfo,
                        guidIndex,
                        idCategoryDictionary,
                        typeIndex))
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

            public ITrackeableFile GetOrigamFile(
                AutoIncrementedIntIndex<Guid> guidIndex,
                AutoIncrementedIntIndex<string> parentFolderIndex,
                IDictionary<int, string> elementIdDictionary,
                OrigamFileFactory origamFileFactory,
                AutoIncrementedIntIndex<TypeInfo> typeIndex)
            {
                ITrackeableFile trackableFile = origamFileFactory.New( 
                    relativePath: RelativePath,
                    fileHash: FileHash,
                    parentFolderIds: ParentFolderIdsNums
                        .ToDictionary(
                            entry => parentFolderIndex[entry.Key],
                            entry => guidIndex[entry.Value]));

                if (trackableFile is OrigamFile origamFile)
                {
                    trackableFile = AddObjectInfo(guidIndex, origamFile, elementIdDictionary, typeIndex);
                }

                return trackableFile;
            }

            private OrigamFile AddObjectInfo(
                AutoIncrementedIntIndex<Guid> guidIndex,
                OrigamFile origamFile,
                IDictionary<int, string> elementIdDictionary,
                AutoIncrementedIntIndex<TypeInfo> typeIndex)    
            {
                foreach (ObjectInfoSerializedForm objInfoSf in ContainedObjInfos)
                {
                    Guid guid = guidIndex[objInfoSf.IdNumber];
                    PersistedObjectInfo objInfo =
                        objInfoSf.GetObjectInfo(origamFile, 
                            guidIndex, elementIdDictionary, typeIndex);

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
            private readonly int categoryId;


            [ProtoMember(2)]
            public int IdNumber { get; }
            [ProtoMember(3)]
            private bool IsFolder { get; }
            [ProtoMember(4)]
            private int ParentIdNumber { get; }
            
            [ProtoMember(5)]          
            public int TypeId { get; }
            
            private ObjectInfoSerializedForm()
            {
            }
            
            public ObjectInfoSerializedForm(PersistedObjectInfo objInfo,
                AutoIncrementedIntIndex<Guid> guidIndex,
                IDictionary<string, int> idCategoryDictionary,
                AutoIncrementedIntIndex<TypeInfo> typeIndex)
            {
                categoryId = idCategoryDictionary[objInfo.Category];
                IdNumber = guidIndex.AddValueAndGetId(objInfo.Id);
                ParentIdNumber = guidIndex.AddValueAndGetId(objInfo.ParentId);
                IsFolder = objInfo.IsFolder;
                TypeId = typeIndex.AddValueAndGetId(new TypeInfo(objInfo.FullTypeName, objInfo.Version));
            }

            public PersistedObjectInfo GetObjectInfo(OrigamFile origamFile,
                AutoIncrementedIntIndex<Guid> guidIndex,
                IDictionary<int, string> elementIdDictionary,
                AutoIncrementedIntIndex<TypeInfo> typeIndex)
            {
                return 
                    new PersistedObjectInfo(
                        category: elementIdDictionary[categoryId], 
                        id:guidIndex[IdNumber],
                        parentId:guidIndex[ParentIdNumber],
                        isFolder:IsFolder,
                        origamFile:origamFile,
                        fullTypeName: typeIndex[TypeId].FullTypeName,
                        version: typeIndex[TypeId].Version);
            }
            public override string ToString()
            {
                return "ObjectInfoSerializedForm: " +
                       " elementNameId: " + categoryId +
                       ", IdNumber: " + IdNumber +
                       ", IsFolder: " + IsFolder +
                       ", ParentIdNumber: " + ParentIdNumber;
            }
        }
    }
    [ProtoContract]
    public class TypeInfo
    {
        [ProtoMember(1)]
        public string FullTypeName { get;  }

        public Version Version {
            get
            {
                if (version == null)
                {
                    version = Version.Parse(versionStr);
                }
                return version;
            }
        }

        private Version version;

        [ProtoMember(2)]
        private readonly string versionStr;

        public TypeInfo()
        {
        }

        public TypeInfo( string fullTypeName, Version version)
        {
            versionStr =  version.ToString();
            FullTypeName = fullTypeName;
        }

        protected bool Equals(TypeInfo other)
        {
            return versionStr == other.versionStr && FullTypeName == other.FullTypeName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TypeInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((versionStr != null ? versionStr.GetHashCode() : 0) * 397) ^ (FullTypeName != null ? FullTypeName.GetHashCode() : 0);
            }
        }
    }
}