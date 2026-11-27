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
import cn from 'classnames';
import { observer } from 'mobx-react-lite';
import { ReactNode, useCallback, useContext, useEffect, useRef, useState } from 'react';
import { VscArrowSwap, VscTriangleDown, VscTriangleUp, VscWarning } from 'react-icons/vsc';

export interface ISchemaItemResultRow {
  key: string;
  result: ISearchResult;
  extraCell?: ReactNode;
  extraText?: string;
}

type TSortDirection = 'ascending' | 'descending';

interface ISortKey {
  columnKey: string;
  direction: TSortDirection;
}

const EXTRA_COLUMN_KEY = 'extra';
const MIN_COLUMN_WIDTH = 48;
const collator = new Intl.Collator(undefined, { numeric: true });

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
    const [sortKeys, setSortKeys] = useState<ISortKey[]>([
      { columnKey: 'foundIn', direction: 'ascending' },
    ]);
    const [columnWidths, setColumnWidths] = useState<number[] | null>(null);
    const resizeStartRef = useRef<{
      columnIndex: number;
      pointerX: number;
      widths: number[];
      bodyCursor: string;
      bodyUserSelect: string;
    } | null>(null);
    const notApplicable = T('n/a', 'schema_item_results_not_applicable');

    const dataColumns = [
      {
        key: 'foundIn',
        label: T('Found in', 'schema_item_results_column_found_in'),
        text: (row: ISchemaItemResultRow) => row.result.foundIn,
      },
      {
        key: 'rootType',
        label: T('Root type', 'schema_item_results_column_root_type'),
        text: (row: ISchemaItemResultRow) =>
          row.result.isOrphaned ? notApplicable : row.result.rootType,
      },
      {
        key: 'type',
        label: T('Type', 'schema_item_results_column_type'),
        text: (row: ISchemaItemResultRow) => row.result.type,
      },
      {
        key: 'folder',
        label: T('Folder', 'schema_item_results_column_folder'),
        text: (row: ISchemaItemResultRow) =>
          row.result.isOrphaned ? notApplicable : row.result.folder,
      },
      {
        key: 'package',
        label: T('Package', 'schema_item_results_column_package'),
        text: (row: ISchemaItemResultRow) =>
          row.result.isOrphaned ? notApplicable : row.result.package,
      },
    ];
    const extraDataColumn = extraColumn && {
      key: EXTRA_COLUMN_KEY,
      label: extraColumn.label,
      text: (row: ISchemaItemResultRow) => row.extraText ?? '',
    };
    const columns = [
      ...(extraColumn?.position === 'first' && extraDataColumn ? [extraDataColumn] : []),
      ...dataColumns,
      ...(extraColumn?.position === 'last' && extraDataColumn ? [extraDataColumn] : []),
    ];

    const sortColumns = sortKeys.flatMap(sortKey => {
      const column = columns.find(candidate => candidate.key === sortKey.columnKey);
      return column ? [{ column, direction: sortKey.direction }] : [];
    });
    const sortedRows = [...rows].sort((left, right) => {
      for (const { column, direction } of sortColumns) {
        const order = collator.compare(column.text(left), column.text(right));
        if (order !== 0) {
          return direction === 'ascending' ? order : -order;
        }
      }
      return collator.compare(left.result.foundIn, right.result.foundIn);
    });

    function changeSort(columnKey: string, isAddingToSort: boolean) {
      setSortKeys(currentKeys => {
        const currentKey = currentKeys.find(sortKey => sortKey.columnKey === columnKey);
        if (!isAddingToSort) {
          return [
            {
              columnKey,
              direction: currentKey?.direction === 'ascending' ? 'descending' : 'ascending',
            },
          ];
        }
        if (!currentKey) {
          return [...currentKeys, { columnKey, direction: 'ascending' }];
        }
        if (currentKey.direction === 'ascending') {
          return currentKeys.map((sortKey): ISortKey =>
            sortKey === currentKey ? { columnKey, direction: 'descending' } : sortKey,
          );
        }
        return currentKeys.filter(sortKey => sortKey !== currentKey);
      });
    }

    function startResizing(event: React.PointerEvent<HTMLSpanElement>) {
      const headerCell = event.currentTarget.closest('th');
      const headerRow = event.currentTarget.closest('tr');
      if (!headerCell || !headerRow) {
        return;
      }
      event.preventDefault();
      event.currentTarget.setPointerCapture(event.pointerId);
      const widths =
        columnWidths ??
        Array.from(headerRow.cells, cell => cell.getBoundingClientRect().width).slice(0, -1);
      resizeStartRef.current = {
        columnIndex: headerCell.cellIndex,
        pointerX: event.clientX,
        widths,
        bodyCursor: resizeStartRef.current?.bodyCursor ?? document.body.style.cursor,
        bodyUserSelect: resizeStartRef.current?.bodyUserSelect ?? document.body.style.userSelect,
      };
      setColumnWidths(widths);
      document.body.style.cursor = 'col-resize';
      document.body.style.userSelect = 'none';
    }

    function resize(event: React.PointerEvent<HTMLSpanElement>) {
      const resizeStart = resizeStartRef.current;
      if (!resizeStart) {
        return;
      }
      const width = Math.max(
        MIN_COLUMN_WIDTH,
        resizeStart.widths[resizeStart.columnIndex] + event.clientX - resizeStart.pointerX,
      );
      setColumnWidths(
        resizeStart.widths.map((currentWidth, index) =>
          index === resizeStart.columnIndex ? width : currentWidth,
        ),
      );
    }

    const stopResizing = useCallback(() => {
      const resizeStart = resizeStartRef.current;
      if (!resizeStart) {
        return;
      }
      resizeStartRef.current = null;
      document.body.style.cursor = resizeStart.bodyCursor;
      document.body.style.userSelect = resizeStart.bodyUserSelect;
    }, []);

    useEffect(() => stopResizing, [stopResizing]);

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

    const totalWidth = columnWidths?.reduce((sum, width) => sum + width, 0);

    return (
      <div className={S.tableWrapper}>
        <table
          className={cn(S.table, columnWidths && S.resized)}
          style={columnWidths ? { width: `max(100%, ${totalWidth}px)` } : undefined}
          data-test-id="schema-item-results-table"
        >
          {columnWidths && (
            <colgroup>
              {columnWidths.map((width, index) => (
                <col key={index} style={{ width }} />
              ))}
              <col />
            </colgroup>
          )}
          <thead>
            <tr>
              <th
                className={S.statusColumn}
                aria-label={T('Status', 'schema_item_results_column_status')}
              />
              {columns.map(column => {
                const sortKey = sortKeys.find(candidate => candidate.columnKey === column.key);
                const sortPosition = sortKey ? sortKeys.indexOf(sortKey) : -1;
                return (
                  <th
                    key={column.key}
                    className={sortKey ? S.sortedColumn : undefined}
                    aria-sort={sortKey && sortPosition === 0 ? sortKey.direction : undefined}
                  >
                    <button
                      type="button"
                      className={S.sortButton}
                      title={T(
                        'Sort by {0}. Ctrl+click to sort by several columns.',
                        'schema_item_results_sort_by',
                        column.label,
                      )}
                      onClick={event => changeSort(column.key, event.ctrlKey || event.metaKey)}
                    >
                      <span className={S.sortLabel}>{column.label}</span>
                      {sortKey ? (
                        <span className={S.sortIndicator} aria-hidden="true">
                          {sortKeys.length > 1 && sortPosition + 1}
                          {sortKey.direction === 'ascending' ? (
                            <VscTriangleUp />
                          ) : (
                            <VscTriangleDown />
                          )}
                        </span>
                      ) : (
                        <VscArrowSwap className={S.unsortedIcon} />
                      )}
                    </button>
                    <span
                      className={S.resizeHandle}
                      role="separator"
                      aria-orientation="vertical"
                      onPointerDown={startResizing}
                      onPointerMove={resize}
                      onLostPointerCapture={stopResizing}
                    />
                  </th>
                );
              })}
              <th className={S.fillerColumn} aria-hidden="true" />
            </tr>
          </thead>
          <tbody>
            {sortedRows.map(row => (
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
                {columns.map(column => (
                  <td key={column.key}>
                    {column.key === EXTRA_COLUMN_KEY ? row.extraCell : column.text(row)}
                  </td>
                ))}
                <td className={S.fillerColumn} aria-hidden="true" />
              </tr>
            ))}
            {sortedRows.length === 0 && (
              <tr>
                <td className={S.empty} colSpan={columns.length + 2}>
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
