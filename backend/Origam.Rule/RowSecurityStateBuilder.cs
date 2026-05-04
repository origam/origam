#region license
/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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

using System;
using System.Data;
using Origam.DA;
using Origam.Extensions;
using Origam.Schema.EntityModel;
using Origam.Service.Core;

namespace Origam.Rule;

public class RowSecurityStateBuilder
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    public RowSecurityState Result { get; private set; }
    private DataRow row;
    private RuleEngine ruleEngine;
    private Guid entityId;
    private bool isNew;
    private XmlContainer originalData;
    private XmlContainer actualData;
    private bool isBuildable;
    private RuleEvaluationCache ruleEvaluationCache;

    public static RowSecurityState BuildFull(
        RuleEngine ruleEngine,
        DataRow row,
        object profileId,
        Guid formId
    )
    {
        var builder = new RowSecurityStateBuilder(row: row, ruleEngine: ruleEngine);
        if (!builder.isBuildable)
        {
            return null;
        }
        return builder
            .AddMainEntityRowStateAndFormatting()
            .AddMainEntityFieldStates()
            .AddRelations(profileId: profileId)
            .AddDisabledActions(formId: formId)
            .Result;
    }

    public static RowSecurityState BuildWithoutRelationsAndActions(
        RuleEngine ruleEngine,
        DataRow row
    )
    {
        var builder = new RowSecurityStateBuilder(row: row, ruleEngine: ruleEngine);
        if (!builder.isBuildable)
        {
            return null;
        }
        return builder.AddMainEntityRowStateAndFormatting().AddMainEntityFieldStates().Result;
    }

    public static RowSecurityState BuildJustMainEntityRowLevelEvenWithoutFields(
        RuleEngine ruleEngine,
        DataRow row
    )
    {
        var builder = new RowSecurityStateBuilder(row: row, ruleEngine: ruleEngine);
        if (!builder.isBuildable)
        {
            return null;
        }
        return builder.AddMainEntityRowStateAndFormatting().Result;
    }

    private RowSecurityStateBuilder(DataRow row, RuleEngine ruleEngine)
    {
        if (
            !DatasetTools.HasRowValidParent(row: row)
            || row.Table.ExtendedProperties.Contains(key: "EntityId")
        )
        {
            isBuildable = false;
        }
        this.ruleEngine = ruleEngine;
        this.row = row;
        // get extra info from row
        entityId = (Guid)row.Table.ExtendedProperties[key: "EntityId"];
        isNew = row.RowState == DataRowState.Added || row.RowState == DataRowState.Detached;
        originalData = DatasetTools.GetRowXml(row: row, version: DataRowVersion.Original);
        actualData = DatasetTools.GetRowXml(
            row: row,
            version: row.HasVersion(version: DataRowVersion.Proposed)
                ? DataRowVersion.Proposed
                : DataRowVersion.Default
        );
        ruleEvaluationCache = new RuleEvaluationCache();
        isBuildable = true;
    }

    private RowSecurityStateBuilder AddMainEntityRowStateAndFormatting()
    {
        if (!isBuildable)
        {
            return this;
        }
        EntityFormatting formatting = ruleEngine.Formatting(
            data: actualData,
            entityId: entityId,
            fieldId: Guid.Empty,
            contextPosition: null
        );
        Result = new RowSecurityState
        {
            Id = DatasetTools.PrimaryKey(row: row)[0],
            BackgroundColor = formatting.BackColor.ToArgb(),
            ForegroundColor = formatting.ForeColor.ToArgb(),
            AllowDelete = ruleEngine.EvaluateRowLevelSecurityState(
                originalData: originalData,
                actualData: actualData,
                field: null,
                type: CredentialType.Delete,
                entityId: entityId,
                fieldId: Guid.Empty,
                isNewRow: isNew,
                ruleEvaluationCache: ruleEvaluationCache
            ),
            AllowCreate = ruleEngine.EvaluateRowLevelSecurityState(
                originalData: originalData,
                actualData: actualData,
                field: null,
                type: CredentialType.Create,
                entityId: entityId,
                fieldId: Guid.Empty,
                isNewRow: isNew,
                ruleEvaluationCache: ruleEvaluationCache
            ),
        };
        return this;
    }

    private RowSecurityStateBuilder AddMainEntityFieldStates()
    {
        if (!isBuildable)
        {
            return this;
        }
        foreach (DataColumn col in row.Table.Columns)
        {
            if (col.ExtendedProperties.Contains(key: "Id"))
            {
                Guid fieldId = (Guid)col.ExtendedProperties[key: "Id"];
                bool allowUpdate = ruleEngine.EvaluateRowLevelSecurityState(
                    originalData: originalData,
                    actualData: actualData,
                    field: col.ColumnName,
                    type: CredentialType.Update,
                    entityId: entityId,
                    fieldId: fieldId,
                    isNewRow: isNew,
                    ruleEvaluationCache: ruleEvaluationCache
                );
                bool allowRead = ruleEngine.EvaluateRowLevelSecurityState(
                    originalData: originalData,
                    actualData: actualData,
                    field: col.ColumnName,
                    type: CredentialType.Read,
                    entityId: entityId,
                    fieldId: fieldId,
                    isNewRow: isNew,
                    ruleEvaluationCache: ruleEvaluationCache
                );
                EntityFormatting fieldFormatting = ruleEngine.Formatting(
                    data: actualData,
                    entityId: entityId,
                    fieldId: fieldId,
                    contextPosition: null
                );
                string dynamicLabel = ruleEngine.DynamicLabel(
                    data: actualData,
                    entityId: entityId,
                    fieldId: fieldId,
                    contextPosition: null
                );
                Result.Columns.Add(
                    item: new FieldSecurityState(
                        name: col.ColumnName,
                        allowUpdate: allowUpdate,
                        allowRead: allowRead,
                        dynamicLabel: dynamicLabel,
                        backgroundColor: fieldFormatting.BackColor.ToArgb(),
                        foregroundColor: fieldFormatting.ForeColor.ToArgb()
                    )
                );
            }
        }
        return this;
    }

    private RowSecurityStateBuilder AddRelations(object profileId)
    {
        if (!isBuildable)
        {
            return this;
        }
        foreach (DataRelation rel in row.Table.ChildRelations)
        {
            Guid childEntityId = (Guid)rel.ChildTable.ExtendedProperties[key: "EntityId"];
            bool isDummyRow = false;
            DataRow childRow = null;
            DataRow[] childRows = row.GetChildRows(relation: rel);
            try
            {
                if (childRows.Length > 0)
                {
                    childRow = childRows[0];
                }
                else
                {
                    isDummyRow = true;
                    childRow = DatasetTools.CreateRow(
                        parentRow: row,
                        newRowTable: rel.ChildTable,
                        relation: rel,
                        profileId: profileId
                    );
                    // go through each column and lookup any looked-up
                    // column values
                    foreach (DataColumn childCol in childRow.Table.Columns)
                    {
#if !ORIGAM_SERVER
                        if (
                            childRow.RowState != DataRowState.Unchanged
                            && childRow.RowState != DataRowState.Detached
                        )
                        {
#endif
                            ruleEngine.ProcessRulesLookupFields(
                                row: childRow,
                                columnName: childCol.ColumnName
                            );
#if !ORIGAM_SERVER
                        }
#endif
                    }
                }
                XmlContainer originalChildData = DatasetTools.GetRowXml(
                    row: childRow,
                    version: DataRowVersion.Original
                );
                XmlContainer actualChildData = DatasetTools.GetRowXml(
                    row: childRow,
                    version: childRow.HasVersion(version: DataRowVersion.Proposed)
                        ? DataRowVersion.Proposed
                        : DataRowVersion.Default
                );
                bool allowRelationCreate = ruleEngine.EvaluateRowLevelSecurityState(
                    originalData: originalChildData,
                    actualData: actualChildData,
                    field: null,
                    type: CredentialType.Create,
                    entityId: childEntityId,
                    fieldId: Guid.Empty,
                    isNewRow: row.RowState == DataRowState.Added
                        || row.RowState == DataRowState.Detached,
                    ruleEvaluationCache: ruleEvaluationCache
                );
                Result.Relations.Add(
                    item: new RelationSecurityState(
                        name: rel.ChildTable.TableName,
                        allowCreate: allowRelationCreate
                    )
                );
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.LogOrigamError(
                        message: string.Format(
                            format: "Failed evaluating security rule for "
                                + "child relation {0} for entity {1}",
                            arg0: rel?.RelationName,
                            arg1: entityId
                        ),
                        ex: ex
                    );
                }
                throw;
            }
            finally
            {
                if (isDummyRow && childRow != null)
                {
                    childRow.Delete();
                }
            }
        }
        return this;
    }

    private RowSecurityStateBuilder AddDisabledActions(Guid formId)
    {
        if (!isBuildable)
        {
            return this;
        }
        Result.DisabledActions = ruleEngine.GetDisabledActions(
            originalData: originalData,
            actualData: actualData,
            entityId: entityId,
            formId: formId
        );
        return this;
    }
}
