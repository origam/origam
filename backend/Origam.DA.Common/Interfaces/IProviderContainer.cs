namespace Origam.DA.ObjectPersistence;

/// <summary>
/// All persistable items implement this interface.
/// </summary>

public interface IProviderContainer
{
    IPersistenceProvider PersistenceProvider {get; set;}
}