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

import { ISearchResult } from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { observer } from 'mobx-react-lite';
import { type KeyboardEvent, useContext, useRef, useState } from 'react';
import { RootStoreContext, T } from '@/main.tsx';
import S from '@components/search/SearchInput.module.scss';

const debounceMs = 300;
const SearchInput = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const [query, setQuery] = useState('');
  const debounceRef = useRef<number>(undefined);
  const latestQueryRef = useRef('');
  const packagesState = rootStore.packagesState;

  function executeSearch(searchText: string) {
    const trimmedText = searchText.trim();
    latestQueryRef.current = trimmedText;
    if (!trimmedText) {
      return;
    }

    run({
      generator: function* () {
        const results = (yield rootStore.architectApi.searchText(trimmedText)) as ISearchResult[];
        if (latestQueryRef.current !== trimmedText) {
          return;
        }
        rootStore.editorTabViewState.openSearchResults(
          trimmedText,
          results,
          T('Search: {0}', 'editor_search_results_title', trimmedText),
        );
      },
    });
  }

  function onQueryChange(value: string) {
    setQuery(value);
    window.clearTimeout(debounceRef.current);
    debounceRef.current = window.setTimeout(() => {
      executeSearch(value);
    }, debounceMs);
  }

  function onKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key !== 'Enter') {
      return;
    }
    window.clearTimeout(debounceRef.current);
    executeSearch(query);
  }

  if (!packagesState.activePackageId) {
    return null;
  }

  return (
    <div className={S.inputContainer}>
      <input
        className={S.input}
        placeholder={T('Search', 'search_placeholder')}
        value={query}
        onChange={event => onQueryChange(event.target.value)}
        onKeyDown={onKeyDown}
      />
    </div>
  );
});

export default SearchInput;
