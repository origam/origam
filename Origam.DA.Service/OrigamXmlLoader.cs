using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CSharpFunctionalExtensions;
using Origam.DA.ObjectPersistence.Providers;
using Origam.Extensions;
using MoreLinq;
using Origam.ErrorHandling;

namespace Origam.DA.Service
{
    public class OrigamXmlLoader
    {
        private readonly ObjectFileDataFactory objectFileDataFactory;
        private readonly DirectoryInfo topDirectory;
        private readonly XmlFileDataFactory xmlFileDataFactory;

        public OrigamXmlLoader(ObjectFileDataFactory objectFileDataFactory, 
            DirectoryInfo topDirectory, XmlFileDataFactory xmlFileDataFactory)
        {
            this.objectFileDataFactory = objectFileDataFactory;
            this.topDirectory = topDirectory;
            this.xmlFileDataFactory = xmlFileDataFactory;
        }

        public Maybe<XmlLoadError> LoadInto(ItemTracker itemTracker,  bool tryUpdate)
        {
            Result<List<XmlFileData>, XmlLoadError> result =
             FindMissingFiles(itemTracker, tryUpdate);

            if (result.IsSuccess)
            {
                AddOrigamFiles(itemTracker, result.Value);
                RemoveOrigamFilesThatNoLongerExist(itemTracker);
                return Maybe<XmlLoadError>.None;
            } 
            else
            {
                return result.Error;
            }
        }

        private void AddOrigamFiles(ItemTracker itemTracker,
            List<XmlFileData> filesToLoad)
        {
            GetNamespaceFinder(filesToLoad, itemTracker)
                .FileDataWithNamespacesAssigned
                .AsParallel()
                .Select(objFileData => objFileData.Read())
                .ForEach( x=>
                {
                    itemTracker.AddOrReplace(x);
                    itemTracker.AddOrReplaceHash(x);
                });
        }

        private void RemoveOrigamFilesThatNoLongerExist(ItemTracker itemTracker)
        {
            IEnumerable<FileInfo> allFilesInSubDirectories
                = topDirectory.GetAllFilesInSubDirectories();
            itemTracker.KeepOnly(allFilesInSubDirectories);
        }

        private Result<List<XmlFileData>, XmlLoadError> FindMissingFiles(
            ItemTracker itemTracker, bool tryUpdate)
        {
            List<Result<XmlFileData, XmlLoadError>> results = topDirectory
                .GetAllFilesInSubDirectories()
                .AsParallel()
                .Where(OrigamFile.IsOrigamFile)
                .Where(file =>
                    !itemTracker.ContainsFile(file))
                .Select(fileInfo =>
                    xmlFileDataFactory.Create(fileInfo, tryUpdate))
                .ToList();

            List<Result<XmlFileData, XmlLoadError>> errors = results
                .Where(result => result.IsFailure)
                .ToList();

            IEnumerable<XmlFileData> data = results
                .Select(res => res.Value);
                
            return errors.Count == 0
                ? Result.Ok<List<XmlFileData>, XmlLoadError>(data.ToList())
                : Result.Fail<List<XmlFileData>, XmlLoadError>(errors[0].Error);
        }

        private INamespaceFinder GetNamespaceFinder(List<XmlFileData> filesToLoad,
            ItemTracker itemTracker)
        {
            if (filesToLoad.Count == 0)
            {
                return new NullNamespaceFinder();
            } 

            // PreLoadedNamespaceFinder needs to realod all files so we have to
            // clear tracker and run FindMissingFiles method again to get all
            // origam files. This will not be necessary once loading of
            // individual files is supported.  
            
            itemTracker.Clear();
            List<XmlFileData> allOrigamFiles = 
                FindMissingFiles(itemTracker: itemTracker, tryUpdate: false)
                .Value;
            return new PreLoadedNamespaceFinder(
                allOrigamFiles,
                objectFileDataFactory);
        }
    }

    public class XmlFileDataFactory
    {
        private readonly List<MetaVersionFixer> versionFixers;

        public XmlFileDataFactory(List<MetaVersionFixer> versionFixers)
        {
            this.versionFixers = versionFixers;
        }

        public Result<XmlFileData, XmlLoadError> Create(FileInfo fileInfo,
            bool tryUpdate = false)
        {
            Result<OrigamXmlDocument> documentResult = LoadXmlDoc(fileInfo);
            if (documentResult.IsFailure)
            {
                return Result.Fail<XmlFileData, XmlLoadError>(
                    new XmlLoadError(documentResult.Error));
            }

            Result<int, XmlLoadError> result = versionFixers
                .Select(fixer =>fixer.UpdateVersion(documentResult.Value, tryUpdate))
                .Cast<Result<int, XmlLoadError>?>()
                .FirstOrDefault(res => res.Value.IsFailure)
                ?? Result.Ok<int, XmlLoadError>(0);
            return result.OnSuccess(res =>
                new XmlFileData(documentResult.Value, fileInfo));
        }

        private Result<OrigamXmlDocument> LoadXmlDoc(FileInfo fileInfo)
        {
            OrigamXmlDocument xmlDocument = new OrigamXmlDocument();
            try
            {
                xmlDocument.Load(fileInfo.FullName);
            } catch (XmlException ex)
            {
                return Result.Fail<OrigamXmlDocument>(
                    $"Could not read file: {fileInfo.FullName}{Environment.NewLine}{ex.Message}");
            }
            return Result.Ok(xmlDocument);
        }
    }

    public class OrigamXmlDocument : XmlDocument
    {
        public string GetNameSpaceByName(string xmlNameSpaceName)
        {
            if (IsEmpty) return null;
            return ChildNodes[1]?.Attributes?[xmlNameSpaceName]?.InnerText;
        }

        public bool IsEmpty => ChildNodes.Count < 2;
    }

    public class XmlFileData
    {
        public XmlDocument XmlDocument { get; }
        public XmlNamespaceManager NamespaceManager{ get; }
        public FileInfo FileInfo { get;}

        internal XmlFileData(XmlDocument xmlDocument, FileInfo fileInfo)
        {
            XmlDocument = xmlDocument;
            FileInfo = fileInfo;
            NamespaceManager = new XmlNamespaceManager(XmlDocument.NameTable);       
            NamespaceManager.AddNamespace("x",OrigamFile.ModelPersistenceUri);
            NamespaceManager.AddNamespace("p",OrigamFile.PackageUri);
            NamespaceManager.AddNamespace("g",OrigamFile.GroupUri);
        }
    }

    public class MetaVersionFixer
    {
        private readonly string xmlNameSpaceName;
        private readonly bool failIfNamespaceNotFound;
        private readonly Version currentVersion;

        public MetaVersionFixer(string xmlNameSpaceName, Version currentVersion,
            bool failIfNamespaceNotFound)
        {
            this.xmlNameSpaceName = xmlNameSpaceName;
            this.failIfNamespaceNotFound = failIfNamespaceNotFound;
            this.currentVersion = currentVersion;
        }

        public  Result<int,XmlLoadError> UpdateVersion(OrigamXmlDocument xmlDoc, bool tryUpdate)
        {
            if (xmlDoc.IsEmpty) Result.Ok<int, XmlLoadError>(0);
            string nameSpace = xmlDoc.GetNameSpaceByName(xmlNameSpaceName);
            if (nameSpace != null)
            {
                return UpdateVersion(xmlDoc, nameSpace, tryUpdate);
            }
            return failIfNamespaceNotFound 
                ? Result.Fail<int,XmlLoadError>( new XmlLoadError(ErrType.XmlGeneralError, xmlNameSpaceName+" namespace not found in: "+xmlDoc.BaseURI)) 
                : Result.Ok<int, XmlLoadError>(0);
        }

        private Result<int,XmlLoadError> UpdateVersion(OrigamXmlDocument xmlDoc,string nameSpace, bool tryUpdate)
        {
            Version version = ElementNameFactory.Create(nameSpace).Version;
            if ( version > currentVersion)
            {
                return Result.Fail<int,XmlLoadError>( new XmlLoadError(ErrType.XmlGeneralError, $"Cannot work with file: {xmlDoc.BaseURI} because it's version of namespace \"{nameSpace}\" is newer than the current version: {currentVersion}"));
            }
            if( version < currentVersion &&
               !version.DiffersOnlyInBuildFrom(currentVersion))
            {
                if(tryUpdate)
                {
                    throw new NotImplementedException();
                } else
                {
                    return Result.Fail<int,XmlLoadError>( 
                        new XmlLoadError( ErrType.XmlVersionIsOlderThanCurrent,
                        $"{xmlDoc.BaseURI} has old version of: {nameSpace}, current version: {currentVersion}"));
                }
            }
            return Result.Ok<int, XmlLoadError>(0);
        } 
    }

    public class XmlLoadError
    {
        public ErrType Type { get;}
        public readonly string Message;

        public XmlLoadError(ErrType type, string message)
        {
            Message = message;
            Type = type;
        }

        public XmlLoadError(string message)
        {
            Message = message;
            Type = ErrType.XmlGeneralError;
        }
    }

    public enum ErrType
    {
        XmlVersionIsOlderThanCurrent,
        XmlGeneralError
    }

    public class ObjectFileData
    {
        public ParentFolders ParentFolderIds { get; }
        private readonly XmlFileData xmlFileData;
        public Folder Folder { get;}
        public FileInfo FileInfo => xmlFileData.FileInfo;
        public virtual Folder FolderToDetermineParentGroup => Folder;
        private readonly OrigamFileFactory origamFileFactory;
        
        public ObjectFileData(IList<ElementName> parentFolders, XmlFileData xmlFileData,
            OrigamFileFactory origamFileFactory)
        {
            this.origamFileFactory = origamFileFactory;
            this.xmlFileData = xmlFileData
                   ?? throw new ArgumentNullException(nameof(xmlFileData));
            ParentFolderIds = new ParentFolders(parentFolders);
            Folder = new Folder(xmlFileData.FileInfo.DirectoryName);
        }
        public OrigamFile Read()
        {
            OrigamFile origamFile = origamFileFactory.New(
                fileInfo: xmlFileData.FileInfo,
                parentFolderIds: ParentFolderIds,
                isAFullyWrittenFile: true);

            Guid parentId = Guid.Empty;
            RecursiveNodeRead(xmlFileData.XmlDocument,parentId, origamFile);
            return origamFile;
        }

        private void RecursiveNodeRead(XmlNode currentNode, Guid parentNodeId,
            OrigamFile origamFile )
        {
            foreach (object nodeObj in currentNode.ChildNodes)
            {
                var childNode = (XmlNode) nodeObj;
                XmlAttribute idAttribute = childNode.Attributes?["x:id"];
                Guid currentNodeId; 
                if (idAttribute != null)
                {
                    Guid parentId = ParseParentId(parentNodeId, childNode);
                    bool isFolder = ParseIsFolder(childNode);
                    currentNodeId = new Guid(idAttribute.Value);
                    var objectInfo = new PersistedObjectInfo(
                        elementName: ElementNameFactory.Create(childNode),
                        id:currentNodeId ,
                        parentId: parentId,
                        isFolder: isFolder,
                        origamFile: origamFile);

                    origamFile.ContainedObjects.Add(objectInfo.Id, objectInfo);
                } 
                else
                {
                    currentNodeId = Guid.Empty;
                }
                RecursiveNodeRead(childNode, currentNodeId, origamFile);
            }
        }

        private static bool ParseIsFolder(XmlNode childNode)
        {
            bool.TryParse(
                childNode?.Attributes?["x:isFolder"]?.Value,
                out bool isFolder);
            return isFolder;
        }

        private static Guid ParseParentId(Guid parentNodeId, XmlNode childNode)
        {
            XmlAttribute nodeAttribute = childNode?.Attributes?["x:parentId"];
            Guid parentId = nodeAttribute != null
                ? new Guid(nodeAttribute.Value)
                : parentNodeId;
            return parentId;
        }
    }

    public class GroupFileData : ObjectFileData
    {
        public Guid GroupId { get; }
        public override Folder FolderToDetermineParentGroup => Folder.Parent;
        public GroupFileData(IList<ElementName> parentFolders,XmlFileData xmlFileData,
            OrigamFileFactory origamFileFactory) :
            base(parentFolders, xmlFileData, origamFileFactory)
        {
            string groupIdStr = 
                xmlFileData
                ?.XmlDocument
                ?.SelectSingleNode("//g:group",xmlFileData.NamespaceManager)
                ?.Attributes?["x:id"]
                ?.Value 
                ?? throw new Exception($"Could not read group id from: {FileInfo.FullName}");
            
            GroupId = new Guid(groupIdStr);
        }
    }

    public class ReferenceFileData
    {
        public ParentFolders ParentFolderIds { get; } = new ParentFolders();
        public Folder Folder { get; }

        public ReferenceFileData(XmlFileData xmlFileData)
        {
            XmlNodeList xmlNodeList = xmlFileData
                      .XmlDocument
                      ?.SelectNodes("//x:groupReference",
                          xmlFileData.NamespaceManager)
                      ?? throw new Exception($"Could not find groupReference in: {xmlFileData.FileInfo.FullName}");
            foreach (object node in xmlNodeList)
            {
                string name = (node as XmlNode)?.Attributes?["x:type"].Value 
                    ?? throw new Exception($"Could not read type form file: {xmlFileData.FileInfo.FullName} node: {node}");
                string idStr = (node as XmlNode).Attributes?["x:refId"].Value
                    ?? throw new Exception($"Could not read id form file: {xmlFileData.FileInfo.FullName} node: {node}");

                var folderUri = ElementNameFactory.Create(name);
                ParentFolderIds[folderUri] = new Guid(idStr);
            }
            Folder = new Folder(xmlFileData.FileInfo.DirectoryName);
        }
    }

    public class PackageFileData: ObjectFileData
    {
        public Guid PackageId { get; }

        public PackageFileData(IList<ElementName> parentFolders,XmlFileData xmlFileData, 
            OrigamFileFactory origamFileFactory) :
            base(parentFolders,xmlFileData, origamFileFactory)
        {
            string idStr = xmlFileData
                ?.XmlDocument
                ?.SelectSingleNode("//p:package", xmlFileData.NamespaceManager)
                ?.Attributes?["x:id"]
                ?.Value
                ?? throw new Exception($"Could not read package id form file: {xmlFileData.FileInfo.FullName}");
            PackageId = new Guid(idStr);
        }
    }

    public class ObjectFileDataFactory
    {
        private readonly OrigamFileFactory origamFileFactory;
        private readonly IList<ElementName> parentFolders;

        public ObjectFileDataFactory(OrigamFileFactory origamFileFactory, IList<ElementName> parentFolders)
        {
            this.origamFileFactory = origamFileFactory;
            this.parentFolders = parentFolders;
        }

        public PackageFileData NewPackageFileData(XmlFileData xmlData)
        {
           return new PackageFileData(parentFolders, xmlData, origamFileFactory); 
        }

        public GroupFileData NewGroupFileData(XmlFileData xmlData)
        {
            return new GroupFileData(parentFolders, xmlData, origamFileFactory);   
        }
        public ObjectFileData NewObjectFileData(XmlFileData xmlData)
        {
           return new ObjectFileData(parentFolders, xmlData, origamFileFactory); 
        }
    }  

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
                    case OrigamFile.RefFileName:
                        var referenceFileData = new ReferenceFileData(xmlData);
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
           referenceFileDict.TryGetValue(data.Folder, out var refFileData);
           return refFileData;
        }
    }
    
    /// <summary>
    ///  This class wraps DirectoryInfo because we need it as a key in a
    ///  dictionary and DirectoryInfo doesn't override hashcode ands Equals.
    /// </summary>
    public class Folder
    {
        private readonly DirectoryInfo dirInfo;

        public Folder(string path)
        {
            dirInfo = new DirectoryInfo(path); 
        }

        public Folder Parent => new Folder(dirInfo.Parent.FullName);

        public bool IsParentOf(Folder other) => 
            dirInfo.IsOnPathOf(other.dirInfo);

        private bool Equals(Folder other) => 
            string.Equals(dirInfo.FullName, other.dirInfo.FullName);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Folder) obj);
        }

        public override int GetHashCode() => 
            (dirInfo.FullName != null ? dirInfo.FullName.GetHashCode() : 0);
    }
}
