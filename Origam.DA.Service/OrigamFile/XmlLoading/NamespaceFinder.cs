using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace Origam.DA.Service
{
    internal interface INamespaceFinder
    {
        IEnumerable<ObjectFileData> FileDataWithNamespacesAssigned { get; }
    }

    internal class NullNamespaceFinder: INamespaceFinder
    {
        public IEnumerable<ObjectFileData> FileDataWithNamespacesAssigned =>
            new List<ObjectFileData>();
    }
    internal class PreLoadedNamespaceFinder: INamespaceFinder
    {
        private readonly List<ObjectFileData> objectFileData =
            new List<ObjectFileData>();

        private readonly List<PackageFileData> packageFiles = 
            new List<PackageFileData>();
        private readonly Dictionary<Folder,GroupFileData> groupFileDict=
            new Dictionary<Folder, GroupFileData>();
        private readonly Dictionary<Folder,ReferenceFileData> referenceFileDict=
            new Dictionary<Folder, ReferenceFileData>();
        private readonly ObjectFileDataFactory objectFileDataFactory;
        private readonly List<XmlFileData> filesToLoad;

        public IEnumerable<ObjectFileData> FileDataWithNamespacesAssigned {
            get
            {
                FillDataFileLists(filesToLoad);
                AssignAllocationAttributes();
                
                foreach (ObjectFileData fileData in objectFileData)
                {
                    yield return fileData;
                }
                foreach (GroupFileData groupFileData in groupFileDict.Values)
                {
                    yield return groupFileData;
                }
                foreach (PackageFileData packageFileData in packageFiles)
                {
                    yield return packageFileData;
                }
                foreach (ReferenceFileData referenceFileData in referenceFileDict.Values)
                {
                    yield return referenceFileData;
                }
            }
        }

        public PreLoadedNamespaceFinder(List<XmlFileData> filesToLoad, ObjectFileDataFactory objectFileDataFactory)
        {
            this.objectFileDataFactory = objectFileDataFactory;
            this.filesToLoad = filesToLoad;
        }

        private void FillDataFileLists(List<XmlFileData> allFiles)
        {
            foreach (XmlFileData xmlData in allFiles)
            {
                switch (xmlData.FileInfo.Name)
                {
                    case OrigamFile.PackageFileName:
                        packageFiles.Add(objectFileDataFactory.NewPackageFileData(xmlData));
                        break;
                    case OrigamFile.GroupFileName:
                        var groupFileData = objectFileDataFactory.NewGroupFileData(xmlData);
                        groupFileDict.Add(groupFileData.Folder,groupFileData);
                        break;
                    case OrigamFile.ReferenceFileName:
                        var referenceFileData = objectFileDataFactory.NewReferenceFileData(xmlData);
                        referenceFileDict.Add(
                            referenceFileData.Folder,
                            referenceFileData);
                        break;
                    default:
                        objectFileData.Add(objectFileDataFactory.NewObjectFileData(xmlData));
                        break;
                }
            }
        }

        private void AssignAllocationAttributes()
        {
            objectFileData
                .AsParallel()
                .ForEach(AssignLocationAttributes);
            groupFileDict.Values.ForEach(AssignLocationAttributes);
        }

        private void AssignLocationAttributes(ObjectFileData data)
        {
            ReferenceFileData refFile = FindReferenceFile(data);
            if (refFile != null)
            {
                refFile.ParentFolderIds.CopyTo(data.ParentFolderIds);
                data.ParentFolderIds.CheckIsValid();
                return;
            }

            Guid? packageId = FindPackageId(data);
            if (!packageId.HasValue)
            {
                throw new Exception(
                    $"Could not find containing package for: {data.FileInfo.FullName}");
            }
            data.ParentFolderIds.PackageId = packageId.Value;

            GroupFileData groupFile = FindGroupFile(data);
            if (groupFile != null)
            {
                data.ParentFolderIds.GroupId = groupFile.GroupId;
            }
            data.ParentFolderIds.CheckIsValid();
        }

        protected virtual GroupFileData FindGroupFile(ObjectFileData data)
        {
            groupFileDict.TryGetValue(
                data.FolderToDetermineParentGroup,
                out var groupFileData);
            return groupFileData != data ? groupFileData : null;
        }

        protected virtual Guid? FindPackageId(ObjectFileData data)
        {          
            return packageFiles
                .FirstOrDefault(package => package.Folder.IsParentOf(data.Folder))    
                ?.PackageId;
        }

        protected virtual ReferenceFileData FindReferenceFile(ObjectFileData data)
        {
            referenceFileDict.TryGetValue(
                data.FolderToDetermineReferenceGroup, out var refFileData);
            return refFileData;
        }
    }
}