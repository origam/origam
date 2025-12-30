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

import { RootStoreContext, T } from '@/main';
import { EditorData } from '@components/modelTree/EditorData';
import { ISearchResult } from '@api/IArchitectApi';
import { SearchResultsEditorState } from '@editors/searchResultsEditor/SearchResultsEditorState';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import S from '@editors/searchResultsEditor/SearchResultsEditor.module.scss';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';

const SearchResultsEditor = observer(
  ({ editorState }: { editorState: SearchResultsEditorState }) => {
    const rootStore = useContext(RootStoreContext);
    const run = runInFlowWithHandler(rootStore.errorDialogController);

    function openSchemaItem(result: ISearchResult) {
      run({
        generator: function* () {
          yield* rootStore.modelTreeState.expandAndHighlightSchemaItem({
            parentNodeIds: result.parentNodeIds ?? [],
            schemaItemId: result.schemaId,
          });
          const apiEditorData = yield rootStore.architectApi.openEditor(result.schemaId);
          const editorData = new EditorData(apiEditorData, null);
          rootStore.editorTabViewState.openEditor(editorData);
        },
      });
    }

    return (
      <div className={S.root}>
        <div className={S.header}>
          <span className={S.title}>
            {T('Search results for "{0}"', 'editor_search_results_header', editorState.query)}
          </span>
          <span className={S.count}>{editorState.results.length}</span>
        </div>
        <div className={S.tableWrapper}>
          <table className={S.table}>
            <thead>
              <tr>
                <th>{T('Found in', 'editor_search_results_column_found_in')}</th>
                <th>{T('Root type', 'editor_search_results_column_root_type')}</th>
                <th>{T('Type', 'editor_search_results_column_type')}</th>
                <th>{T('Folder', 'editor_search_results_column_folder')}</th>
                <th>{T('Package', 'editor_search_results_column_package')}</th>
                <th>{T('Package reference', 'editor_search_results_column_package_reference')}</th>
                <th>{T('Open', 'editor_search_results_column_open')}</th>
              </tr>
            </thead>
            <tbody>
              {[...editorState.results]
                .sort((left, right) => left.foundIn.localeCompare(right.foundIn))
                .map(result => (
                  <tr key={result.schemaId} className={S.row}>
                    <td>{result.foundIn}</td>
                    <td>{result.rootType}</td>
                    <td>{result.type}</td>
                    <td>{result.folder}</td>
                    <td>{result.package}</td>
                    <td>
                      {result.packageReference ? T('Yes', 'dialog_yes') : T('No', 'dialog_no')}
                    </td>
                    <td>
                      <button className={S.openButton} onClick={() => openSchemaItem(result)}>
                        {T('Open', 'editor_search_results_open')}
                      </button>
                    </td>
                  </tr>
                ))}
              {editorState.results.length === 0 && (
                <tr>
                  <td className={S.empty} colSpan={7}>
                    {T('No results found.', 'editor_search_results_empty')}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    );
  },
);

export default SearchResultsEditor;
