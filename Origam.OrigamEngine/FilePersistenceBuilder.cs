using System;
using Origam.Workbench.Services;
using System.Collections.Generic;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Providers;
using Origam.DA.Service;
using Origam.Schema;

namespace Origam.OrigamEngine
{
    public class FilePersistenceBuilder : IPersistenceBuilder
    {
        private static FilePersistenceService persistenceService;
        
        public IDocumentationService GetDocumentationService() =>
            new FileStorageDocumentationService(
                (IFilePersistenceProvider) persistenceService.SchemaProvider,
                persistenceService.FileEventQueue);

        public IPersistenceService GetPersistenceService() => 
            GetPersistenceService(watchFileChanges: true);

        public IPersistenceService GetPersistenceService(bool watchFileChanges)
        {
            persistenceService = CreateNewPersistenceService(watchFileChanges);
            return persistenceService;
        }

        public FilePersistenceService CreateNewPersistenceService(bool watchFileChanges)
        {
            List<ElementName> defaultFolders = new List<ElementName>
            {
                ElementNameFactory.Create(typeof(SchemaExtension)),
                ElementNameFactory.Create(typeof(SchemaItemGroup))
            };

            return new FilePersistenceService(
                defaultFolders: defaultFolders,
                watchFileChanges: watchFileChanges);
        }

        public FilePersistenceService CreateNoBinFilePersistenceService()
        {
            List<ElementName> defaultFolders = new List<ElementName>
            {
                ElementNameFactory.Create(typeof(SchemaExtension)),
                ElementNameFactory.Create(typeof(SchemaItemGroup))
            };

            return new FilePersistenceService(
                defaultFolders: defaultFolders,
                watchFileChanges: false,
                useBinFile: false);
        }

        public static void Clear()
        {
            persistenceService = null;
        }
    }
}
