﻿namespace Origam.Architect.Server.Controllers;

public class ChangesModel
{
    public Guid SchemaItemId { get; set; }
    public List<PropertyChange> Changes { get; set; }
}

public class PropertyChange
{
    public string Name { get; set; }
    public string Value { get; set; }
}