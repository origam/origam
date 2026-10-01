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

import { T } from '@/main';
import S from '@components/search/SearchResultsView.module.scss';
import { SearchResultsTabState } from '@components/search/SearchResultsTabState';
import SchemaItemResultsTable from '@components/schemaItemResults/SchemaItemResultsTable';
import { observer } from 'mobx-react-lite';

const SearchResultsView = observer(({ editorState }: { editorState: SearchResultsTabState }) => {
  const rows = editorState.results.map(result => ({
    key: result.schemaId,
    result,
    extraCell: result.isOrphaned
      ? T('n/a', 'schema_item_results_not_applicable')
      : result.packageReference
        ? T('Yes', 'dialog_yes')
        : T('No', 'dialog_no'),
  }));

  return (
    <div className={S.root}>
      <div className={S.header}>
        <span className={S.title}>
          {T('Search results for "{0}"', 'editor_search_results_header', editorState.query)}
        </span>
        <span className={S.count}>{editorState.results.length}</span>
      </div>
      <SchemaItemResultsTable
        rows={rows}
        extraColumn={{
          label: T('Package reference', 'editor_search_results_column_package_reference'),
          position: 'last',
        }}
        emptyText={T('No results found.', 'editor_search_results_empty')}
      />
    </div>
  );
});

export default SearchResultsView;
