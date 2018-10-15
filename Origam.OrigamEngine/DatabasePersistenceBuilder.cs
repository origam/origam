using System;
using Origam.Workbench.Services;

namespace Origam.OrigamEngine
{
    public class DatabasePersistenceBuilder : IPersistenceBuilder
    {
        public IDocumentationService GetDocumentationService()
        {
            return new DocumentationService();
        }

        public IPersistenceService GetPersistenceService()
        {
            return new PersistenceService();
        }
    }
}
