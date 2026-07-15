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

public class TabId
{
    public Guid SchemaItemId { get; }
    private readonly string id;
    public TabType Type { get; }

    public static TabId Default(Guid schemaItemId)
    {
        return new TabId(schemaItemId, TabType.Default);
    }

    public static TabId Documentation(Guid schemaItemId)
    {
        return new TabId(schemaItemId, TabType.DocumentationEditor);
    }

    public TabId(string tabId)
    {
        if (string.IsNullOrEmpty(tabId))
        {
            throw new ArgumentException($"Could not parse {tabId} to tab Id");
        }

        string[] strings = tabId.Split("_");
        if (strings.Length != 2)
        {
            throw new ArgumentException($"Could not parse {tabId} to tab Id");
        }

        if (Enum.TryParse(strings[0], out TabType tabType))
        {
            Type = tabType;
        }
        else
        {
            throw new ArgumentException($"Could not parse {tabId} to tab Id");
        }

        if (Guid.TryParse(strings[1], out Guid parsedId))
        {
            SchemaItemId = parsedId;
        }
        else
        {
            throw new ArgumentException($"Could not parse {tabId} to tab Id");
        }

        id = tabId;
    }

    TabId(Guid schemaItemId, TabType tabType)
    {
        SchemaItemId = schemaItemId;
        Type = tabType;
        id = tabType + "_" + schemaItemId;
    }

    protected bool Equals(TabId other)
    {
        return id == other.id;
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj.GetType() != GetType())
        {
            return false;
        }
        return Equals((TabId)obj);
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

public enum TabType
{
    Default,
    DocumentationEditor,
}
