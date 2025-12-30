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
