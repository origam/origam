using Origam.Architect.Server.ArchitectLogic;
using Origam.Architect.Server.Controls;
using Origam.Architect.Server.Services;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.ControlAdapter;

public class ControlAdapterFactory(EditorPropertyFactory propertyFactory,
    SchemaService schemaService, IPersistenceService persistenceService,
    PropertyParser propertyParser)
{
    public ControlAdapter Create(ControlSetItem controlSetItem)
    {
        string oldFullClassName = controlSetItem.ControlItem.ControlType;
        try
        {
            string className = oldFullClassName
                .Split(".")
                .LastOrDefault();
            if (className == "PanelControlSet")
            {
                className = "AsPanel";
            }
            string newFullClassName =
                "Origam.Architect.Server.Controls." + className;
            Type controlType = Type.GetType(newFullClassName);
            if (controlType == null)
            {
                throw new Exception("Cannot find type: " + newFullClassName);
            }
            
            IControl control = Activator.CreateInstance(controlType) as IControl;
            return new ControlAdapter(controlSetItem, control,
                propertyFactory, schemaService, persistenceService, propertyParser);
        }
        catch (Exception ex)
        {
            throw new Exception("Cannot find a form class for " +
                                oldFullClassName, ex);
        }
    }
}