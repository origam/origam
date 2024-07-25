import { useContext, useEffect } from "react";
import { useDispatch, useSelector } from 'react-redux';
import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import S from 'src/components/editors/gridEditor/GridEditor.module.scss';
import { ArchitectApiContext } from "src/API/ArchitectApiContext.tsx";
import {
  EditorState,
  initializeEditor
} from 'src/components/editors/gridEditor/GrirEditorSlice.ts';
import { RootState } from 'src/stores/store.ts';
import { PropertyEditor } from "src/components/editors/propertyEditor/PropertyEditor.tsx";
import {
} from "src/components/editors/xsltEditor/XsltEditor.tsx";

export function GridEditor(props: {
  node: TreeNode
}) {
  const editorId = props.node.nodeText + "_" + props.node.id;
  const editorState = useSelector((state: RootState) => state.editorStates.editors[editorId]);
  useEditorInitialization(editorState, props.node);

  return (
    <div className={S.gridEditor}>
      <h3 className={S.title}>{`Editing: ${props.node.nodeText}`}</h3>
      <PropertyEditor
        editorId={editorId}
        properties={editorState?.properties}/>
    </div>
  );
}

export function useEditorInitialization(editorState: EditorState, node: TreeNode){
  const dispatch = useDispatch();
  const editorId = getEditorId(node);
  const architectApi = useContext(ArchitectApiContext)!;
  useEffect(() => {
    async function getData() {
      try {
        const newProperties = await architectApi.getProperties(node.id);
        dispatch(initializeEditor({
          editorId,
          schemaItemId: node.id,
          properties: newProperties
        }));
      } catch (error) {
        console.error("Error fetching properties:", error);
      }
    }

    if (!editorState) {
      getData();
    }
  }, [editorId, architectApi, dispatch, editorState]);
  return editorId;
}

export function getEditorId(node: TreeNode): string {
  return node.nodeText + "_" + node.id;
}