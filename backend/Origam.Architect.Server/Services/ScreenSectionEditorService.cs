
using Origam.Architect.Server.Models;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class ScreenSectionEditorService(
    SchemaService schemaService,
    IPersistenceService persistenceService)
{
    private readonly IPersistenceProvider persistenceProvider = persistenceService.SchemaProvider;
    
    public SectionEditorModel GetSectionEditorData(ISchemaItem editedItem)
    {
        if (editedItem is PanelControlSet screenSection)
        {
            var entityProvider = schemaService.GetProvider(typeof(EntityModelSchemaItemProvider)) as EntityModelSchemaItemProvider;
            var dataSources = entityProvider.ChildItems
                .Select(x => new DataSource { Name = x.Name, SchemaItemId = x.Id })
                .ToList();
            
            IDataEntity dataEntity = screenSection.DataEntity;
            
            List<EditorField> fields = dataEntity
                .ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn.CategoryConst)
                .OrderBy(field => field.Name)
                .Select(field => new EditorField
                {
                    Name = field.Name, 
                    Type = field.DataType
                })
                .ToList();
            
            return new SectionEditorModel
            {
                Name = editedItem.Name,
                SchemaExtensionId = editedItem.SchemaExtensionId,
                DataSources = dataSources,
                SelectedDataSourceId = screenSection.DataEntity.Id,
                Fields = fields
            };
        }

        return null;
    }
}