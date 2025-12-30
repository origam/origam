import { IApiEditorData, ISearchResult } from '@api/IArchitectApi';
import { EditorData } from '@components/modelTree/EditorData';
import { SearchResultsEditorState } from '@editors/searchResultsEditor/SearchResultsEditorState';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { observer } from 'mobx-react-lite';
import { type KeyboardEvent, useContext, useRef, useState } from 'react';
import { RootStoreContext, T } from '@/main.tsx';
import S from '@components/search/SearchInput.module.scss';

const debounceMs = 300;
const searchEditorId = 'SearchResultsEditor-Id';

const SearchInput = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const [query, setQuery] = useState('');
  const debounceRef = useRef<number>(undefined);
  const latestQueryRef = useRef('');

  function openSearchResults(queryText: string, results: ISearchResult[]) {
    const existingEditor = rootStore.editorTabViewState.editorsContainers.find(
      editor => editor.state instanceof SearchResultsEditorState,
    );
    if (existingEditor) {
      const editorState = existingEditor.state as SearchResultsEditorState;
      editorState.query = queryText;
      editorState.results = results;
      editorState.label = T('Search: {0}', 'editor_search_results_title', queryText);
      rootStore.editorTabViewState.setActiveEditor(editorState.editorId);
      return;
    }

    const tempEditorData: IApiEditorData = {
      editorId: searchEditorId,
      editorType: 'SearchResultsEditor',
      parentNodeId: undefined,
      isDirty: false,
      node: {
        id: '',
        origamId: '',
        nodeText: '',
        editorType: null,
      },
      data: {
        query: queryText,
        results,
      },
    };

    const editorData = new EditorData(tempEditorData, null);
    rootStore.editorTabViewState.openEditor(editorData);
  }

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
        openSearchResults(trimmedText, results);
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
