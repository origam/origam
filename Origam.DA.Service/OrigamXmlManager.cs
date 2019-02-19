using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using CSharpFunctionalExtensions;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.DA.Service
{
    internal class OrigamXmlManager
    {
        private readonly ExternalFileManager externalFileManger;
        private readonly OrigamPathFactory origamPathFactory;
        public XmlDocument OpenDocument { get; set; }
        private readonly object Lock = new object();
        
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);

        private LocalizedObjectCache loadedLocalizedObjects;

        public OrigamPath Path { private get; set; }

        public IDictionary<Guid, PersistedObjectInfo> ContainedObjects{ get;} 
            = new Dictionary<Guid, PersistedObjectInfo>();
        public ParentFolders ParentFolderIds { get; }
        public static XmlDocument NewDocument()
        {
            XmlDocument newDocument = new XmlDocument();       
            string xml = string.Format(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><x:file xmlns:x=\"{0}\" xmlns=\"{1}\" xmlns:p=\"{2}\"/>",
                OrigamFile.ModelPersistenceUri,
                OrigamFile.GroupUri,
                OrigamFile.PackageUri);
            newDocument.LoadXml(xml);
            return newDocument;
        }

        public OrigamXmlManager(OrigamPath path, ParentFolders parentFolderIds,
            ExternalFileManager externalFileManger, OrigamPathFactory origamPathFactory)
        {
            Path = path;
            ParentFolderIds = parentFolderIds;
            this.externalFileManger = externalFileManger;
            this.origamPathFactory = origamPathFactory;
        }

        public IFilePersistent LoadObject(Guid id, IPersistenceProvider provider,
            bool useCache)
        {
            IFilePersistent cachedObject = GetCachedObject(id, provider, useCache);
            if (useCache)
            {
                return cachedObject;
            }

            IFilePersistent loadedObj = null;
                loadedObj = new InstanceCloner(cachedObject, ParentFolderIds)
                    .RetrieveInstance(
                        id: id,
                        provider: provider,
                        parentId: cachedObject.FileParentId);
            
            if(loadedObj == null) throw new Exception();
            return loadedObj;
        }
        
        public void InvalidateCache()
        {
            lock (Lock)
            {
                loadedLocalizedObjects = null;
            }
        }

        public void RemoveFromCache(IPersistent instance)
        {
            lock (Lock)
            {
                loadedLocalizedObjects?.Remove(instance.Id);
            }
        }

        private IFilePersistent GetCachedObject(Guid id, IPersistenceProvider provider, bool useCache)
        {
            lock (Lock)
            {
                if (loadedLocalizedObjects == null)
                {
                    loadedLocalizedObjects = new LocalizedObjectCache();
                    loadedLocalizedObjects.AddRange(LoadAllObjectsFromDisk(provider, useCache));
                }
                if (!loadedLocalizedObjects.Contains(id))
                {
                    AddObjectsFromDisk(provider, useCache);
                }

                Maybe<IFilePersistent> mayBeObject = loadedLocalizedObjects.Get(id);
                if (mayBeObject.HasNoValue)
                {
                    throw new Exception("Could not find object with id:" + id +
                                        " in file: " + Path.Absolute);
                }
                return mayBeObject.Value;
            }
        }

        private void AddObjectsFromDisk(IPersistenceProvider provider, bool useCache)
        {
            lock (Lock)
            {
                var newObjects = new LocalizedObjectCache();
                newObjects.AddRange(LoadAllObjectsFromDisk(provider, useCache));

                newObjects.LocalizedPairs
                    .Where(keyVal => !loadedLocalizedObjects.Contains(keyVal.Key))
                    .ForEach(keyVal =>
                        loadedLocalizedObjects.Add(keyVal.Key, keyVal.Value));
            }
        }

        private IEnumerable<IFilePersistent> LoadAllObjectsFromDisk(IPersistenceProvider provider, bool useCache)
        {
            ParentIdTracker parentIdTracker = new ParentIdTracker();
            using (XmlReader xmlReader = GetDocumentReader())
            {
                var instanceCreator = 
                    new InstanceCreator(xmlReader, ParentFolderIds, externalFileManger);
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.EndElement) continue;
                    Guid? retrievedId = XmlUtils.ReadId(xmlReader);
                    if (!retrievedId.HasValue) continue;
                    parentIdTracker.SetParent(retrievedId.Value, xmlReader.Depth + 1);
                    IFilePersistent loadedObj = instanceCreator.RetrieveInstance( 
                        retrievedId.Value, provider, parentIdTracker.Get(xmlReader.Depth));
                    loadedObj.UseObjectCache=useCache;
                    yield return loadedObj;

                }
            }
        }

        private XmlReader GetDocumentReader()
        {
            if (OpenDocument == null)
            {
                FileInfo fi = new FileInfo(Path.Absolute);
                return new XmlTextReader(fi.OpenRead());
            }

            return new XmlNodeReader(OpenDocument);
        }

        private class ParentIdTracker
        {
            readonly Dictionary<int, Guid> depthToParentDict = new Dictionary<int, Guid>();

            public void SetParent(Guid parentId, int depth)
            {
                depthToParentDict[depth] = parentId;
            }

            public Guid Get(int depth)
            {
                if(!depthToParentDict.ContainsKey(depth)) return Guid.Empty;
                return depthToParentDict[depth];
            }
        }

        public void RemoveInstance(Guid id )
        {
            XmlNode nodeToDelete = OpenDocument
                .GetAllNodes()
                .Where(node => XmlUtils.ReadId(node).HasValue)
                .FirstOrDefault(node => XmlUtils.ReadId(node).Value == id);

            nodeToDelete?.Attributes
                .Cast<XmlAttribute>()
                .Select(attribute => attribute.Value)
                .Where(attrText => ExternalFilePath.IsExternalFileLink(attrText))
                .ForEach(attrText => externalFileManger.RemoveExternalFile(attrText));

            nodeToDelete?.ParentNode.RemoveChild(nodeToDelete);
            ContainedObjects.Remove(id);
        }

        public void WriteInstance(IFilePersistent instance,
            ElementName elementName)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Retrieving file {0}", Path);
            }

            new InstanceWriter(externalFileManger, OpenDocument)
                .Write(instance,elementName);
            AddToLoadedObjects(instance);
        }

        private void AddToLoadedObjects(IFilePersistent instance)
        {
            lock (Lock)
            {
                if (loadedLocalizedObjects == null)
                {
                    loadedLocalizedObjects = new LocalizedObjectCache();
                }

                loadedLocalizedObjects[instance.Id] = instance;
            }
        }

        public IEnumerable<ExternalFilePath> GetExternalFilePaths()
        {
            return OpenDocument.GetAllNodes()
                .Where(node => node?.Attributes != null)
                .SelectMany(node => node.Attributes.Cast<XmlAttribute>())
                .Select(xmlAttribute => xmlAttribute.Value)
                .Where(ExternalFilePath.IsExternalFileLink)
                .Select(attrValue => origamPathFactory.Create(Path, attrValue));
        }

        public void UpdateExternalLinks(OrigamFile origamFile)
        {
            if (origamFile.NewPath == null) return;
            origamFile.DeferredSaveDocument
                .GetAllNodes()
                .ForEach(node => ReplaceInLink(
                                    node: node, 
                                    find: origamFile.Path.FileName, 
                                    replace: origamFile.NewPath.FileName));
        }

        private void ReplaceInLink(XmlNode node, string find, string replace)
        {
             node.Attributes
                ?.Cast<XmlAttribute>()
                .Where(attr =>ExternalFilePath.IsExternalFileLink(attr.Value))
                .ForEach(attr => attr.Value = attr.Value.Replace(find, replace));
        }
  }
}