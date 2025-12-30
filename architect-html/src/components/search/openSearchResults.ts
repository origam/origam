import { IApiEditorData, ISearchResult } from '@api/IArchitectApi';
import { EditorData } from '@components/modelTree/EditorData';
import { SearchResultsEditorState } from '@editors/searchResultsEditor/SearchResultsEditorState';
import { RootStore } from '@stores/RootStore';

const searchEditorId = 'SearchResultsEditor-Id';

export function openSearchResults(
  rootStore: RootStore,
  queryText: string,
  results: ISearchResult[],
  label: string,
) {
  const existingEditor = rootStore.editorTabViewState.editorsContainers.find(
    editor => editor.state instanceof SearchResultsEditorState,
  );
  if (existingEditor) {
    const editorState = existingEditor.state as SearchResultsEditorState;
    editorState.query = queryText;
    editorState.results = results;
    editorState.label = label;
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
