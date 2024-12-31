using System.ComponentModel;

namespace Origam.Architect.Server.ReturnModels;

public class EditorProperty
{
    public EditorProperty(
        string name,
        Guid? controlPropertyId,
        string type,
        object value,
        DropDownValue[] dropDownValues,
        string category,
        string description,
        bool readOnly)
    {
    Name  = name;
    Type = type;
    Value = value;
    DropDownValues  = dropDownValues;
    Category= category;
    Description = description;
    ReadOnly = readOnly;
    ControlPropertyId = controlPropertyId;
    }

    public string Name { get; } 
    public string Type { get; }
    public object Value { get; set; }
    public DropDownValue[] DropDownValues { get; }
    public string Category { get; } 
    public string Description { get; }
    public bool ReadOnly { get; } 
    public Guid? ControlPropertyId { get; }
    public List<string> Errors { get; set; }
}

public class DropDownValue(string Name, object Value)
{
    public string Name { get; } = Name;
    public object Value { get; } = Value;
}

class Context : ITypeDescriptorContext
{
    public Context(object instance)
    {
        Instance = instance;
    }

    public object GetService(Type serviceType)
    {
        throw new NotImplementedException();
    }

    public void OnComponentChanged()
    {
        throw new NotImplementedException();
    }

    public bool OnComponentChanging()
    {
        throw new NotImplementedException();
    }

    public IContainer Container { get; }
    public object Instance { get; }
    public PropertyDescriptor PropertyDescriptor { get; }
}