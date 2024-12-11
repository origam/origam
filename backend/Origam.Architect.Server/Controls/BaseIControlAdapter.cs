using System.ComponentModel;
using System.Reflection;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public abstract class BaseIControlAdapter : IControlAdapter
{
    private readonly ControlSetItem controlSetItem;

    public BaseIControlAdapter(ControlSetItem controlSetItem)
    {
        this.controlSetItem = controlSetItem;
    }

    [Category("(ORIGAM)")]
    [SchemaItemProperty]
    public string SchemaItemName {
        get => controlSetItem.Name;
        set => controlSetItem.Name = value;
    }
    
    [Category("(ORIGAM)")]
    [SchemaItemProperty]
    public string Roles {
        get => controlSetItem.Roles;
        set => controlSetItem.Roles = value;
    }
    [Category("(ORIGAM)")]
    [SchemaItemProperty]
    public string Features {
        get => controlSetItem.Features;
        set => controlSetItem.Features = value;
    }
    
    [ReadOnly(true)]
    [Category("(ORIGAM)")]
    [SchemaItemProperty]
    public string SchemaItemId => controlSetItem.Id.ToString();

    public PropertyInfo[] GetSchemaItemProperties()
    {
        return GetType().GetProperties()
            .Where(p => p.GetCustomAttribute<SchemaItemPropertyAttribute>() != null)
            .ToArray();
    }
    
    public PropertyInfo[] GetValueItemProperties()
    {
        return GetType().GetProperties()
            .Where(p => p.GetCustomAttribute<SchemaItemPropertyAttribute>() == null)
            .ToArray();
    }
}

public class SchemaItemPropertyAttribute : Attribute
{
    
}