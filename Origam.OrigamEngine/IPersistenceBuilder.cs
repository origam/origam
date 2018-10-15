using Origam.Workbench.Services;

namespace Origam.OrigamEngine
{
    public interface IPersistenceBuilder
    {
        IPersistenceService GetPersistenceService();
        IDocumentationService GetDocumentationService();
    }
}
