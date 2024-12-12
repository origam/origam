using Origam.Architect.Server.ReturnModels;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.ControlAdapter;

public class ControlAdapterFactory(EditorPropertyFactory propertyFactory,
    SchemaService schemaService, IPersistenceService persistenceService)
{
    public ControlAdapter Create(ControlSetItem controlSetItem)
    {
        string oldFullClassName = controlSetItem.ControlItem.ControlType;
        try
        {
            string className = oldFullClassName
                .Split(".")
                .LastOrDefault();
            string newFullClassName =
                "Origam.Architect.Server.Controls." + className;
            Type controlType = Type.GetType(newFullClassName);
            if (controlType == null)
            {
                throw new Exception("Cannot find type: " + newFullClassName);
            }
            
            return new ControlAdapter(controlSetItem, controlType,
                propertyFactory, schemaService, persistenceService);
        }
        catch (Exception ex)
        {
            throw new Exception("Cannot find a form class for " +
                                oldFullClassName, ex);
        }
    }
}