namespace Origam.Architect.Server.Services;

public class EditorId
{
    public Guid SchemaItemId { get; }
    private readonly string id;
    public EditorType Type { get; }

    public static EditorId Default(Guid schemaItemId)
    {
        return new EditorId(schemaItemId, EditorType.Default);
    }

    public static EditorId Documentation(Guid schemaItemId)
    {
        return new EditorId(schemaItemId, EditorType.DocumentationEditor);
    }

    public EditorId(string editorId)
    {
        if (string.IsNullOrEmpty(editorId))
        {
            throw new ArgumentException($"Could not parse {editorId} to editor Id");
        }

        string[] strings = editorId.Split("_");
        if (strings.Length != 2)
        {
            throw new ArgumentException($"Could not parse {editorId} to editor Id");
        }

        if (Enum.TryParse(strings[0], out EditorType editorType))
        {
            Type = editorType;
        }
        else
        {
            throw new ArgumentException($"Could not parse {editorId} to editor Id");
        }

        if (Guid.TryParse(strings[1], out Guid parsedId))
        {
            SchemaItemId = parsedId;
        }
        else
        {
            throw new ArgumentException($"Could not parse {editorId} to editor Id");
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
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EditorId)obj);
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
    DocumentationEditor
}