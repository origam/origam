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
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Origam.Extensions;
using ProtoBuf;

namespace Origam.DA.Service;
[ProtoContract]
public class TrackerSerializationData
{
    public List<ITrackeableFile> GetOrigamFiles(OrigamFileFactory origamFileFactory) 
        => TransformBack(origamFileFactory);
    
    public Dictionary<string, int> ItemTrackerStats => itemTrackerStats;
    
    [ProtoMember(2)]
    private readonly AutoIncrementedIntIndex<string> categoryIndex = 
        new AutoIncrementedIntIndex<string>();
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
    private IDictionary<string, int> CategoryIdDictionary =>
        categoryIndex.ValueToId;
    
    private IDictionary<int, string> IdCategoryDictionary =>
        categoryIndex.IdToValue;
    private TrackerSerializationData()
    {
    }
    public TrackerSerializationData(IEnumerable<ITrackeableFile> origamFiles, 
        Dictionary<string, int> itemTrackerStats)
    {
        origamFiles
            .SelectMany(x=>x.ContainedObjects.Values)
            .ForEach(x=> categoryIndex.AddValueAndGetId(x.Category));
        ToSerializationForms(origamFiles, categoryIndex.ValueToId);
        this.itemTrackerStats = itemTrackerStats;
    }
    private void ToSerializationForms(
        IEnumerable<ITrackeableFile> origamFiles, IDictionary<string,int> categoryIdDictionary)
    {
        serializationList = origamFiles
            .Select(orFile =>
                new OrigamFileSerializedForm(orFile, this))
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
                serForm.GetOrigamFile(this, origamFileFactory))
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
            TrackerSerializationData trackerData)
        {
            origamFile.ParentFolderIds.CheckIsValid(origamFile.Path);
            RelativePath = origamFile.Path.Relative;
            FileHash = origamFile.FileHash;
            ContainedObjInfos = origamFile.ContainedObjects.Values
                .Select(objInfo => new ObjectInfoSerializedForm(
                    objInfo, trackerData))
                .ToList();
            if (ContainedObjInfos == null)
            {
                throw new Exception( $"origamFile: {origamFile.Path.Absolute} contains no objects");
            }
            ParentFolderIdsNums = origamFile.ParentFolderIds
                .ToDictionary(
                    entry => trackerData.parentFolderIndex.AddValueAndGetId(entry.Key),
                    entry => trackerData.guidIndex.AddValueAndGetId(entry.Value));
        }
        public ITrackeableFile GetOrigamFile(TrackerSerializationData trackerData,
            OrigamFileFactory origamFileFactory)
        {
            ITrackeableFile trackableFile = origamFileFactory.New( 
                relativePath: RelativePath,
                fileHash: FileHash,
                parentFolderIds: ParentFolderIdsNums
                    .ToDictionary(
                        entry => trackerData.parentFolderIndex[entry.Key],
                        entry => trackerData.guidIndex[entry.Value]));
            if (trackableFile is OrigamFile origamFile)
            {
                trackableFile = AddObjectInfo(origamFile, trackerData);
            }
            return trackableFile;
        }
        private OrigamFile AddObjectInfo(
            OrigamFile origamFile, TrackerSerializationData trackerData)    
        {
            foreach (ObjectInfoSerializedForm objInfoSf in ContainedObjInfos)
            {
                Guid guid = trackerData.guidIndex[objInfoSf.IdNumber];
                PersistedObjectInfo objInfo =
                    objInfoSf.GetObjectInfo(origamFile, trackerData);
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
            TrackerSerializationData trackerData)
        {
            categoryId = trackerData.CategoryIdDictionary[objInfo.Category];
            IdNumber = trackerData.guidIndex.AddValueAndGetId(objInfo.Id);
            ParentIdNumber = trackerData.guidIndex.AddValueAndGetId(objInfo.ParentId);
            IsFolder = objInfo.IsFolder;
            TypeId = trackerData.typeIndex.AddValueAndGetId(new TypeInfo(objInfo.FullTypeName, objInfo.Version));
        }
        public PersistedObjectInfo GetObjectInfo(OrigamFile origamFile,
            TrackerSerializationData trackerData)
        {
            return 
                new PersistedObjectInfo(
                    category: trackerData.IdCategoryDictionary[categoryId], 
                    id: trackerData.guidIndex[IdNumber],
                    parentId: trackerData.guidIndex[ParentIdNumber],
                    isFolder:IsFolder,
                    origamFile:origamFile,
                    fullTypeName: trackerData.typeIndex[TypeId].FullTypeName,
                    version: trackerData.typeIndex[TypeId].Version);
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
