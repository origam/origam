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
            List<ElementName> defaultFolders = new List<ElementName>
            {
                ElementNameFactory.Create(typeof(SchemaExtension)),
                ElementNameFactory.Create(typeof(SchemaItemGroup))
            };

            persistenceService = new FilePersistenceService(
                defaultFolders: defaultFolders,
                watchFileChanges: watchFileChanges);
            
            return persistenceService;
        }
    }
}
