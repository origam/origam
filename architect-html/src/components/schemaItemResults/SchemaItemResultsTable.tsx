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

import { RootStoreContext, T } from '@/main';
import { ISearchResult } from '@api/IArchitectApi';
import S from '@components/schemaItemResults/SchemaItemResultsTable.module.scss';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { observer } from 'mobx-react-lite';
import { ReactNode, useContext } from 'react';
import { VscWarning } from 'react-icons/vsc';

export interface ISchemaItemResultRow {
  key: string;
  result: ISearchResult;
  extraCell?: ReactNode;
}

const SchemaItemResultsTable = observer(
  ({
    rows,
    extraColumn,
    emptyText,
  }: {
    rows: ISchemaItemResultRow[];
    extraColumn?: { label: string; position: 'first' | 'last' };
    emptyText: string;
  }) => {
    const rootStore = useContext(RootStoreContext);
    const run = runInFlowWithHandler(rootStore.errorDialogController);
    const notApplicable = T('n/a', 'schema_item_results_not_applicable');
    const columnCount = 6 + (extraColumn ? 1 : 0);
    const sortedRows = [...rows].sort((left, right) =>
      left.result.foundIn.localeCompare(right.result.foundIn),
    );

    function highlightInModelTree(result: ISearchResult) {
      run({
        generator: function* () {
          yield* rootStore.modelTreeState.expandAndHighlightSchemaItem({
            parentNodeIds: result.parentNodeIds ?? [],
            schemaItemId: result.schemaId,
          });
        },
      });
    }

    const extraHeader = extraColumn && <th key="extra">{extraColumn.label}</th>;

    return (
      <div className={S.tableWrapper}>
        <table className={S.table} data-test-id="schema-item-results-table">
          <thead>
            <tr>
              <th
                className={S.statusColumn}
                aria-label={T('Status', 'schema_item_results_column_status')}
              />
              {extraColumn?.position === 'first' && extraHeader}
              <th>{T('Found in', 'schema_item_results_column_found_in')}</th>
              <th>{T('Root type', 'schema_item_results_column_root_type')}</th>
              <th>{T('Type', 'schema_item_results_column_type')}</th>
              <th>{T('Folder', 'schema_item_results_column_folder')}</th>
              <th>{T('Package', 'schema_item_results_column_package')}</th>
              {extraColumn?.position === 'last' && extraHeader}
            </tr>
          </thead>
          <tbody>
            {sortedRows.map(row => {
              const extraCell = extraColumn && <td key="extra">{row.extraCell}</td>;
              return (
                <tr
                  key={row.key}
                  className={row.result.isOrphaned ? S.orphanedRow : S.row}
                  onClick={() => highlightInModelTree(row.result)}
                >
                  <td className={S.statusCell}>
                    {row.result.isOrphaned && (
                      <span
                        className={S.orphanedMarker}
                        title={T(
                          'Orphaned reference - parent chain is broken',
                          'schema_item_results_orphaned_note',
                        )}
                      >
                        <VscWarning />
                      </span>
                    )}
                  </td>
                  {extraColumn?.position === 'first' && extraCell}
                  <td>{row.result.foundIn}</td>
                  <td>{row.result.isOrphaned ? notApplicable : row.result.rootType}</td>
                  <td>{row.result.type}</td>
                  <td>{row.result.isOrphaned ? notApplicable : row.result.folder}</td>
                  <td>{row.result.isOrphaned ? notApplicable : row.result.package}</td>
                  {extraColumn?.position === 'last' && extraCell}
                </tr>
              );
            })}
            {sortedRows.length === 0 && (
              <tr>
                <td className={S.empty} colSpan={columnCount}>
                  {emptyText}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    );
  },
);

export default SchemaItemResultsTable;
