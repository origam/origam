namespace Origam.Architect.Server.Controls;

public class ReferencePropertyAttribute(string name) : Attribute
{
    public string Name { get;} = name;
}