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

namespace Origam.Architect.Server.Services;

public class EditorId
{
    public Guid SchemaItemId { get; }
    private readonly string id;
    public EditorType Type { get; }

    public static EditorId Default(Guid schemaItemId)
    {
        return new EditorId(schemaItemId: schemaItemId, editorType: EditorType.Default);
    }

    public static EditorId Documentation(Guid schemaItemId)
    {
        return new EditorId(schemaItemId: schemaItemId, editorType: EditorType.DocumentationEditor);
    }

    public EditorId(string editorId)
    {
        if (string.IsNullOrEmpty(value: editorId))
        {
            throw new ArgumentException(message: $"Could not parse {editorId} to editor Id");
        }

        string[] strings = editorId.Split(separator: "_");
        if (strings.Length != 2)
        {
            throw new ArgumentException(message: $"Could not parse {editorId} to editor Id");
        }

        if (Enum.TryParse(value: strings[0], result: out EditorType editorType))
        {
            Type = editorType;
        }
        else
        {
            throw new ArgumentException(message: $"Could not parse {editorId} to editor Id");
        }

        if (Guid.TryParse(input: strings[1], result: out Guid parsedId))
        {
            SchemaItemId = parsedId;
        }
        else
        {
            throw new ArgumentException(message: $"Could not parse {editorId} to editor Id");
        }

        id = editorId;
    }

    EditorId(Guid schemaItemId, EditorType editorType)
    {
        SchemaItemId = schemaItemId;
        Type = editorType;
        id = editorType + "_" + schemaItemId;
    }

    protected bool Equals(EditorId other)
    {
        return id == other.id;
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }
        if (ReferenceEquals(objA: this, objB: obj))
        {
            return true;
        }
        if (obj.GetType() != GetType())
        {
            return false;
        }
        return Equals(other: (EditorId)obj);
    }

    public override int GetHashCode()
    {
        return (id != null ? id.GetHashCode() : 0);
    }

    public override string ToString()
    {
        return id;
    }
}

public enum EditorType
{
    Default,
    DocumentationEditor,
}
