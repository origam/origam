using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Workbench.Services;

namespace Origam.Server.Utils
{
    public class StartupUtils
    {
        public static void SubscribeToFileSystemChanges()
        {
            if(ServiceManager.Services.GetService<IPersistenceService>() 
                is FilePersistenceService filePersistenceService)
            {
                void OnReloadNeeded(object e, ChangedFileEventArgs args)
                {
                    IPersistenceProvider persistenceProvider =
                        filePersistenceService.SchemaProvider;
                    if (persistenceProvider is FilePersistenceProvider filePersistProvider)
                    {
                        filePersistProvider.FlushCache();
                        filePersistProvider.ReloadFiles(tryUpdate: false);
                        filePersistProvider.PersistIndex();
                    }
                    OrigamUserContext.ResetAll();
                }
                filePersistenceService.ReloadNeeded += OnReloadNeeded;
            }
        }
    }
}
