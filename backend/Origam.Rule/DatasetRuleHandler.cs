#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

/// <summary>
/// Summary description for DatasetRuleHandler.
/// </summary>
public class DatasetRuleHandler
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private bool _inRowChanging = false;
    private DataStructureRuleSet _ruleSet = null;
    private IDataDocument _currentRuleDocument = null;
    private RuleEngine _ruleEngine = null;

    public DatasetRuleHandler() { }

    public void RegisterDatasetEvents(
        IDataDocument xmlData,
        DataStructureRuleSet ruleSet,
        RuleEngine ruleEngine
    )
    {
        _ruleSet = ruleSet;
        _currentRuleDocument = xmlData;
        _ruleEngine = ruleEngine;
        foreach (DataTable table in xmlData.DataSet.Tables)
        {
            RegisterTableEvents(table: table);
        }
    }

    public void UnregisterDatasetEvents(IDataDocument xmlData)
    {
        _ruleSet = null;
        _currentRuleDocument = null;
        _ruleEngine = null;
        foreach (DataTable table in xmlData.DataSet.Tables)
        {
            UnregisterTableEvents(table: table);
        }
    }

    private void RegisterTableEvents(DataTable table)
    {
        table.RowChanged += new DataRowChangeEventHandler(table_RowChanged);
        table.ColumnChanged += new DataColumnChangeEventHandler(table_ColumnChanged);
    }

    private void UnregisterTableEvents(DataTable table)
    {
        table.RowChanged -= new DataRowChangeEventHandler(table_RowChanged);
        table.ColumnChanged -= new DataColumnChangeEventHandler(table_ColumnChanged);
    }

    public void OnRowChanged(
        DataRowChangeEventArgs e,
        IDataDocument data,
        DataStructureRuleSet ruleSet,
        RuleEngine ruleEngine
    )
    {
        if (_inRowChanging)
        {
            return;
        }

        if (e.Row.RowState == DataRowState.Detached)
        {
            return;
        }

        _inRowChanging = true;
        try
        {
            e.Row.BeginEdit();
            if (e.Action == DataRowAction.Change && e.Row.RowState != DataRowState.Added)
            {
                if (
                    e.Row.Table.Columns.Contains(name: "RecordUpdated")
                    && !e.Row[columnName: "RecordUpdated"].Equals(obj: DateTime.Now)
                )
                {
                    e.Row[columnName: "RecordUpdated"] = DateTime.Now;
                }
                if (e.Row.Table.Columns.Contains(name: "RecordUpdatedBy"))
                {
                    try
                    {
                        UserProfile profile = SecurityManager.CurrentUserProfile();

                        if (!e.Row[columnName: "RecordUpdatedBy"].Equals(obj: profile.Id))
                        {
                            e.Row[columnName: "RecordUpdatedBy"] = profile.Id;
                        }
                    }
                    catch { }
                }
            }
            DatasetTools.CheckRowErrorRecursive(
                row: DatasetTools.RootRow(childRow: e.Row),
                skipRow: null,
                includeChildErrorsInParent: false
            );
        }
        finally
        {
            e.Row.EndEdit();
            _inRowChanging = false;
        }
        if (
            e.Action == DataRowAction.Add
            || e.Action == DataRowAction.Change
            || e.Action == DataRowAction.Rollback
        )
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    message: "Starting rules after '" + e.Row?.Table.TableName + "' changed."
                );
            }
            try
            {
                ruleEngine.ProcessRules(rowChanged: e.Row, data: data, ruleSet: ruleSet);
            }
            catch (Exception ex)
            {
                var origamRuleException = new OrigamRuleException(
                    message: ResourceUtils.GetString(
                        key: "ErrorRuleFailureRecord",
                        args: [e.Row.Table.DisplayExpression, Environment.NewLine + ex.Message]
                    ),
                    innerException: ex,
                    row: e.Row
                );
                log.LogOrigamError(ex: origamRuleException); // DataTable will ignore the exception after we throw it so we at least log it here
                throw origamRuleException;
            }
        }
    }

    public void OnColumnChanged(
        DataColumnChangeEventArgs e,
        IDataDocument data,
        DataStructureRuleSet ruleSet,
        RuleEngine ruleEngine
    )
    {
        OrigamDataRow row = e.Row as OrigamDataRow;
#if ! ORIGAM_SERVER
        if (!row.IsColumnWithValidChange(e.Column))
        {
            return;
        }
#endif
        try
        {
            // check for self-join recursion
            foreach (DataRelation rel in e.Row.Table.ParentRelations)
            {
                // only self-join relations
                if (rel.ParentTable.Equals(obj: rel.ChildTable))
                {
                    bool isFkChanging = false;
                    // only if the changed column is the fk
                    foreach (DataColumn childCol in rel.ChildColumns)
                    {
                        if (e.Column.Equals(obj: childCol))
                        {
                            isFkChanging = true;
                        }
                    }
                    if (isFkChanging)
                    {
                        // go up
                        while (row.GetParentRows(relation: rel).Length != 0)
                        {
                            foreach (DataRow parentRow in row.GetParentRows(relation: rel))
                            {
                                if (parentRow.Equals(obj: e.Row))
                                {
                                    e.Row.SetParentRow(parentRow: null);
                                    throw new Exception(
                                        message: ResourceUtils.GetString(key: "ErrorRecursion")
                                    );
                                }
                                row = parentRow as OrigamDataRow;
                            }
                        }
                    }
                }
            }
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    message: "Column '"
                        + e.Row?.Table.TableName
                        + "."
                        + e.Column?.ColumnName
                        + "' changed to value: "
                        + e.ProposedValue
                );
            }
            ruleEngine.ProcessRules(
                rowChanged: e.Row,
                data: data,
                columnChanged: e.Column,
                ruleSet: ruleSet
            );
        }
        catch (OrigamRuleException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new OrigamRuleException(
                message: ResourceUtils.GetString(
                    key: "ErrorRuleFailureColumn",
                    args: [e.Column.Caption, e.Row.Table.DisplayExpression]
                ),
                innerException: ex,
                row: e.Row
            );
        }
    }

    public void OnRowDeleted(
        DataRow[] parentRows,
        DataRow deletedRow,
        IDataDocument data,
        DataStructureRuleSet ruleSet,
        RuleEngine ruleEngine
    )
    {
        try
        {
            if (parentRows.Length > 0)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        message: "Starting rules after '"
                            + deletedRow?.Table?.TableName
                            + "' deleted."
                    );
                }
                ruleEngine.ProcessRules(
                    rowChanged: deletedRow,
                    data: data,
                    ruleSet: ruleSet,
                    parentRows: parentRows
                );
                foreach (DataRow row in parentRows)
                {
                    try
                    {
                        // we pass all the columns from the deleted row so if some parent rules
                        // depended on some of these fields, they will be fired
                        //ruleEngine.ProcessRules(row, data, deletedRow.Table.Columns, ruleSet);
                        DatasetTools.CheckRowErrorRecursive(
                            row: DatasetTools.RootRow(childRow: row),
                            skipRow: null,
                            includeChildErrorsInParent: false
                        );
                    }
                    catch (Exception ex)
                    {
                        throw new OrigamRuleException(
                            message: ResourceUtils.GetString(
                                key: "ErrorRuleFailureDelete",
                                args:
                                [
                                    deletedRow.Table.DisplayExpression,
                                    Environment.NewLine + ex.Message,
                                ]
                            ),
                            innerException: ex,
                            row: row
                        );
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new OrigamRuleException(
                message: ResourceUtils.GetString(
                    key: "ErrorDeleteFailed",
                    args: [deletedRow.Table.TableName, Environment.NewLine + ex.Message]
                ),
                innerException: ex,
                row: deletedRow
            );
        }
    }

    public void OnRowCopied(
        DataRow row,
        IDataDocument data,
        DataStructureRuleSet ruleSet,
        RuleEngine ruleEngine
    )
    {
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Starting rules after '" + row?.Table?.TableName + "' was copied.");
        }
        try
        {
            //WORKAROUND: Every form should have FormGenerator.XmlData, but self join forms (like address book) don't support it
            // - XmlDataDocument does not support self join DataSets. So we just skip these and don't process rules on them
            // (they should not have any).
            if (data != null)
            {
                ruleEngine.ProcessRules(data: data, ruleSet: ruleSet, contextRow: row);
            }
        }
        catch (Exception ex)
        {
            throw new OrigamRuleException(
                message: ResourceUtils.GetString(
                    key: "ErrorRuleFailureCopy",
                    args: [row.Table.DisplayExpression, Environment.NewLine + ex.Message]
                ),
                innerException: ex,
                row: row
            );
        }
    }

    private void table_RowChanged(object sender, DataRowChangeEventArgs e)
    {
        if (e.Action != DataRowAction.Nothing && e.Action != DataRowAction.Commit)
        {
            OnRowChanged(
                e: e,
                data: _currentRuleDocument,
                ruleSet: _ruleSet,
                ruleEngine: _ruleEngine
            );
        }
    }

    private void table_ColumnChanged(object sender, DataColumnChangeEventArgs e)
    {
        OnColumnChanged(
            e: e,
            data: _currentRuleDocument,
            ruleSet: _ruleSet,
            ruleEngine: _ruleEngine
        );
    }
}
