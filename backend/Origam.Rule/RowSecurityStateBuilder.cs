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

using Origam.DA;
using Origam.Schema.EntityModel;
using Origam.Service.Core;
using System;
using System.Data;
using Origam.Extensions;

namespace Origam.Rule
{
    public class RowSecurityStateBuilder
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.
                GetCurrentMethod().DeclaringType);
        public RowSecurityState Result { get; private set; }
        private DataRow row;
        private RuleEngine ruleEngine;
        private Guid entityId;
        private bool isNew;
        private XmlContainer originalData;
        private XmlContainer actualData;
        private bool isBuildable;
        private RuleEvaluationCache ruleEvaluationCache;


        public static RowSecurityState BuildFull(RuleEngine ruleEngine,
            DataRow row, object profileId, Guid formId)
        {
            var builder = new RowSecurityStateBuilder(row, ruleEngine);
            if (!builder.isBuildable)
            {
                return null;
            }
            return builder.AddMainEntityRowStateAndFormatting()
                .AddMainEntityFieldStates()
                .AddRelations(profileId)
                .AddDisabledActions(formId)
                .Result;
        }

        public static RowSecurityState BuildWithoutRelationsAndActions(
            RuleEngine ruleEngine, DataRow row)
        {
            var builder = new RowSecurityStateBuilder(row, ruleEngine);
            if (!builder.isBuildable)
            {
                return null;
            }
            return builder.AddMainEntityRowStateAndFormatting()
                .AddMainEntityFieldStates()
                .Result;
        }

        public static RowSecurityState
            BuildJustMainEntityRowLevelEvenWithoutFields(RuleEngine ruleEngine,
            DataRow row)
        {
            var builder = new RowSecurityStateBuilder(row, ruleEngine);
            if (!builder.isBuildable)
            {
                return null;
            }
            return builder.AddMainEntityRowStateAndFormatting()
                .Result;
        }

        private RowSecurityStateBuilder(DataRow row, RuleEngine ruleEngine)
        {
            if (!DatasetTools.HasRowValidParent(row)
                || row.Table.ExtendedProperties.Contains("EntityId"))
            {
                isBuildable = false;
            }

            this.ruleEngine = ruleEngine;
            this.row = row;

            // get extra info from row
            entityId = (Guid)row.Table.ExtendedProperties["EntityId"];
            isNew = row.RowState == DataRowState.Added
                || row.RowState == DataRowState.Detached;
            originalData = DatasetTools.GetRowXml(row,
                    DataRowVersion.Original);
            actualData = DatasetTools.GetRowXml(row,
                    row.HasVersion(DataRowVersion.Proposed)
                    ? DataRowVersion.Proposed : DataRowVersion.Default);
            ruleEvaluationCache = new RuleEvaluationCache();
            isBuildable = true;
        }

        private RowSecurityStateBuilder AddMainEntityRowStateAndFormatting()
        {
            if (!isBuildable)
            {
                return this;
            }
            EntityFormatting formatting = ruleEngine.Formatting(actualData,
            entityId, Guid.Empty, null);

            Result = new RowSecurityState
            {
                Id = DatasetTools.PrimaryKey(row)[0],
                BackgroundColor = formatting.BackColor.ToArgb(),
                ForegroundColor = formatting.ForeColor.ToArgb(),
                AllowDelete = ruleEngine.EvaluateRowLevelSecurityState(
                    originalData, actualData, null, CredentialType.Delete,
                    entityId, Guid.Empty, isNew, ruleEvaluationCache),
                AllowCreate = ruleEngine.EvaluateRowLevelSecurityState(
                    originalData, actualData, null, CredentialType.Create,
                    entityId, Guid.Empty, isNew, ruleEvaluationCache)
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
                if (col.ExtendedProperties.Contains("Id"))
                {
                    Guid fieldId = (Guid)col.ExtendedProperties["Id"];

                    bool allowUpdate = ruleEngine.
                        EvaluateRowLevelSecurityState(originalData,
                            actualData, col.ColumnName,
                            CredentialType.Update,
                            entityId, fieldId, isNew, ruleEvaluationCache);
                    bool allowRead = ruleEngine.
                        EvaluateRowLevelSecurityState(originalData,
                            actualData, col.ColumnName,
                            CredentialType.Read,
                            entityId, fieldId, isNew, ruleEvaluationCache);
                    EntityFormatting fieldFormatting = ruleEngine.
                        Formatting(actualData, entityId, fieldId, null);
                    string dynamicLabel = ruleEngine.DynamicLabel(
                        actualData, entityId, fieldId, null);
                    Result.Columns.Add(new FieldSecurityState(
                        col.ColumnName,
                        allowUpdate, allowRead, dynamicLabel,
                        fieldFormatting.BackColor.ToArgb(),
                        fieldFormatting.ForeColor.ToArgb()));
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
                Guid childEntityId = (Guid)rel.ChildTable.
                    ExtendedProperties["EntityId"];
                bool isDummyRow = false;
                DataRow childRow = null;
                DataRow[] childRows = row.GetChildRows(rel);                
                try
                {
                    if (childRows.Length > 0)
                    {
                        childRow = childRows[0];
                    }
                    else
                    {
                        isDummyRow = true;
                        childRow = DatasetTools.CreateRow(row,
                            rel.ChildTable, rel, profileId);

                        // go through each column and lookup any looked-up
                        // column values
                        foreach (DataColumn childCol in
                            childRow.Table.Columns)
                        {
#if !ORIGAM_SERVER
                if (childRow.RowState != DataRowState.Unchanged
                    && childRow.RowState != DataRowState.Detached)
                {
#endif
                            ruleEngine.ProcessRulesLookupFields(childRow,
                                childCol.ColumnName);
#if !ORIGAM_SERVER
                }
#endif
                        }
                    }
                    XmlContainer originalChildData = DatasetTools.GetRowXml(
                        childRow, DataRowVersion.Original);
                    XmlContainer actualChildData = DatasetTools.GetRowXml(
                        childRow, childRow.HasVersion(DataRowVersion.Proposed)
                        ? DataRowVersion.Proposed : DataRowVersion.Default);
                    bool allowRelationCreate = ruleEngine.
                        EvaluateRowLevelSecurityState(originalChildData,
                        actualChildData, null,
                        CredentialType.Create,
                        childEntityId, Guid.Empty,
                        row.RowState == DataRowState.Added 
                            || row.RowState == DataRowState.Detached,
                        ruleEvaluationCache
                    );
                    Result.Relations.Add(new RelationSecurityState(
                        rel.ChildTable.TableName, allowRelationCreate));
                }
                catch (Exception ex)
                {
                    if (log.IsErrorEnabled)
                    {
                        log.LogOrigamError(string.Format(
                            "Failed evaluating security rule for "
                            + "child relation {0} for entity {1}",
                            rel?.RelationName, entityId), ex);
                    }
                    throw;
                }
                finally
                {
                    if (isDummyRow && childRow != null) childRow.Delete();
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
                originalData, actualData, entityId, formId);            
            return this;
        }
    }
}
