using System;
using Origam.Workbench.Services;

namespace Origam.ProjectAutomation
{
    public class FileModelInitBuilder:AbstractBuilder
    {
        public override string Name => "Initialize model";
        public override void Execute(Project project)
        {
            OrigamEngine.OrigamEngine.InitializeRuntimeServices();

            ServiceManager
                .Services
                .GetService<IPersistenceService>()
                .LoadSchemaList();
            
            LoadBaseSchema(project);
        }
        private void LoadBaseSchema(Project project)
        {
            SchemaService schema =
                ServiceManager.Services.GetService<SchemaService>();
            try
            {
                ServiceManager.Services
                    .GetService<IPersistenceService>()
                    .LoadSchemaList();
                schema.LoadSchema(new Guid(project.BasePackageId), false, false);
            } catch
            {
                Rollback();
                try
                {
                    // In case something went wrong AFTER the model was loaded
                    // (e.g. Architect failed handling some events) we unload the model.
                    // Since we do not know if it failed really AFTER, we just catch
                    // possible exceptions.
                    schema.UnloadSchema();
                } catch
                {
                }
                throw;
            }
        }

        public override void Rollback()
        { 
            OrigamEngine.OrigamEngine.UnloadConnectedServices();
        }
    }
}