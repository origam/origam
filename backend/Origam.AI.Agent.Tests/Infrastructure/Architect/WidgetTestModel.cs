#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

namespace Origam.AI.Agent.Tests.Infrastructure.Architect;

public sealed class WidgetTestModel(string lookupId)
{
    public const string LookupName = "LookupBusinessPartner";
    public const string LookupTypeName = "Data Service Lookup";
    public const string WizardField = "Text1";
    public const string TextField = "Text2";
    public const string GroupedTextField = "Note";
    public const string DateField = "Date1";
    public const string BooleanField = "Flag";
    public const string ColorField = "Color";
    public const string FileNameField = "PictureFileName";
    public const string BlobField = "Picture";
    public const string RadioField = "RadioValue";
    public const string WrapperTextField = "WrapperText";
    public const string WrapperDateField = "WrapperDate";
    public const string LookupField = "refBusinessPartnerId";
    public const string TagArrayField = "TagIds";
    public const string ChecklistArrayField = "CheckIds";
    public const string RelationName = "Tags";

    private const string DatabaseEntityTypeName = "Database Entity";
    public const string ScreenSectionTypeName = "Screen Section";
    private const string DatabaseFieldTypeName = "Database Field";
    private const string VirtualFieldTypeName = "Virtual Field";

    private static readonly string[] DatabaseFields =
    [
        WizardField,
        TextField,
        GroupedTextField,
        DateField,
        BooleanField,
        ColorField,
        FileNameField,
        BlobField,
        RadioField,
        WrapperTextField,
        WrapperDateField,
        LookupField,
    ];

    private static readonly string[] ArrayFields = [TagArrayField, ChecklistArrayField];

    public string Suffix { get; } = Guid.NewGuid().ToString("N")[..8];
    public string LookupId { get; } = lookupId;
    public string EntityId { get; private set; } = string.Empty;
    public string ChildEntityId { get; private set; } = string.Empty;
    public string SectionId { get; private set; } = string.Empty;
    public string EntityName => "AiWidget" + Suffix;
    public string ChildEntityName => "AiWidgetTag" + Suffix;
    public string ChildKeyField => "ref" + EntityName + "Id";
    public string SectionName => "AiWidgetSection" + Suffix;
    public string YesConstantName => "AiWidgetYes" + Suffix;
    public string NoConstantName => "AiWidgetNo" + Suffix;

    public string EntityPrompt =>
        $"Create a database entity named {EntityName} with these database fields, all "
        + $"nullable: {WizardField} (String, length 200), {TextField} (String, length 100), "
        + $"{GroupedTextField} (String, length 100), {DateField} (Date), {WrapperDateField} "
        + $"(Date), {BooleanField} (Boolean), {ColorField} (Integer), {FileNameField} (String, "
        + $"length 400), {BlobField} (Blob), {RadioField} (String, length 20), "
        + $"{WrapperTextField} (String, length 200) and {LookupField} (UniqueIdentifier) whose "
        + $"DefaultLookup is the lookup {LookupName} with id {LookupId}. Do not ask me to "
        + "confirm anything, just create everything.";

    public string ArrayFieldsPrompt =>
        $"Now create a second database entity named {ChildEntityName} with two nullable "
        + $"UniqueIdentifier database fields: {ChildKeyField} and {LookupField}, the latter "
        + $"with DefaultLookup = the lookup with id {LookupId}. Then add to {EntityName} a "
        + $"Relationship named {RelationName} whose RelatedEntity is {ChildEntityName}, with "
        + $"a Key that joins the base entity field Id of {EntityName} with the related entity "
        + $"field {ChildKeyField}. Finally add to {EntityName} two virtual fields named "
        + $"{TagArrayField} and {ChecklistArrayField} with DataType Array, ArrayRelation = the "
        + $"{RelationName} relationship, ArrayValueField = the {LookupField} field of "
        + $"{ChildEntityName} and DefaultLookup = the lookup with id {LookupId}. Do not ask "
        + "me to confirm anything, just create everything.";

    public string SectionPrompt =>
        $"Now create a screen section named {SectionName} for the entity {EntityName} that "
        + $"shows only the field {WizardField}; use {SectionName} as its caption too. Do not "
        + "ask me to confirm anything, just create it.";

    public string SectionConfirmPrompt =>
        $"Yes, create the screen section {SectionName} exactly as you listed it, with the "
        + $"field {WizardField} only.";

    public async Task<string?> CompleteAsync(ArchitectModelBuilder builder)
    {
        var entityId = await builder.FindItemIdAsync(EntityName, DatabaseEntityTypeName);
        if (entityId is null)
        {
            return $"The database entity {EntityName} is not in the model.";
        }
        var childEntityId = await builder.FindItemIdAsync(ChildEntityName, DatabaseEntityTypeName);
        if (childEntityId is null)
        {
            return $"The database entity {ChildEntityName} is not in the model.";
        }

        var fields = await builder.ReadFieldsAsync(entityId);
        var missing = DatabaseFields
            .Where(name => !HasField(fields, name, DatabaseFieldTypeName))
            .Concat(ArrayFields.Where(name => !HasField(fields, name, VirtualFieldTypeName)))
            .ToList();
        if (missing.Count > 0)
        {
            return $"{EntityName} lacks the fields {string.Join(separator: ", ", missing)}; "
                + "it has: "
                + string.Join(
                    separator: ", ",
                    fields.Values.Select(field => $"{field.Name} ({field.ItemTypeName})")
                );
        }

        var sectionId = await builder.FindItemIdAsync(SectionName, ScreenSectionTypeName);
        if (sectionId is null)
        {
            return $"The screen section {SectionName} is not in the model.";
        }
        var sectionEntityId = await builder.ReadSectionEntityIdAsync(sectionId);
        if (!string.Equals(sectionEntityId, entityId, StringComparison.OrdinalIgnoreCase))
        {
            return $"The screen section {SectionName} is bound to the entity {sectionEntityId} "
                + $"instead of {EntityName} ({entityId}).";
        }

        EntityId = entityId;
        ChildEntityId = childEntityId;
        SectionId = sectionId;
        return null;
    }

    private static bool HasField(
        IReadOnlyDictionary<string, EntityField> fields,
        string name,
        string itemTypeName
    )
    {
        return fields.TryGetValue(name, out var field) && field.ItemTypeName == itemTypeName;
    }
}
