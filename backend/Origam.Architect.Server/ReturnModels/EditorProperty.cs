#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

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
        bool readOnly
    )
    {
        Name = name;
        Type = type;
        Value = value;
        DropDownValues = dropDownValues;
        Category = category;
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
