using Origam.DA.ObjectPersistence;

namespace Origam.DA.Service
{
    public interface IRuntimeModelConfig
    {
        void SetConfigurationValues(IFilePersistent instance);
        void UpdateConfig(IPersistent persistent);
    }
}