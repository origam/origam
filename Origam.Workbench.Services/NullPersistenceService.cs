using System;
using System.Collections;
using System.Data;
using Origam.DA.ObjectPersistence;
using Origam.Schema;

namespace Origam.Workbench.Services
{
    public class NullPersistenceService : IPersistenceService
    {
        public void InitializeService()
        {
        }

        public void UnloadService()
        {
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public IPersistenceProvider SchemaProvider => new NullPersistenceProvider();
        public IPersistenceProvider SchemaListProvider => new NullPersistenceProvider();

        public void LoadSchema(ArrayList extensions, bool append, bool loadDocumentation, bool loadDeploymentScripts,
            string transactionId)
        {
        }

        public SchemaExtension LoadSchema(Guid schemaExtension, bool loadDocumentation, bool loadDeploymentScripts,
            string transactionId)
        {
            throw new NotImplementedException();
        }

        public SchemaExtension LoadSchema(Guid schemaExtensionId, Guid extraExtensionId, bool loadDocumentation,
            bool loadDeploymentScripts, string transactionId)
        {
            throw new NotImplementedException();
        }

        public void LoadSchemaList()
        {
        }

        public void UpdateRepository()
        {
        }

        public bool IsRepositoryVersionCompatible()
        {
            throw new NotImplementedException();
        }

        public bool CanUpdateRepository()
        {
            throw new NotImplementedException();
        }

        public void ExportPackage(Guid extensionId, string fileName)
        {
        }

        public void MergePackage(Guid extensionId, DataSet data, string transcationId)
        {
        }

        public void MergeSchema(DataSet schema, Key activePackage)
        {
        }

        public void InitializeRepository()
        {
        }
    }
}