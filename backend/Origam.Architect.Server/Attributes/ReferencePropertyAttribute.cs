namespace Origam.Architect.Server.Attributes;

public class ReferencePropertyAttribute(string name) : Attribute
{
    public string Name { get;} = name;
}