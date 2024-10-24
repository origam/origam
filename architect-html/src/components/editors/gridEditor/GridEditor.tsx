import { useEffect } from "react";
import { useDispatch, useSelector } from 'react-redux';
import S from 'src/components/editors/gridEditor/GridEditor.module.scss';
import {
  EditorState, getEditorId,
   initializeEditor
} from 'src/components/editors/gridEditor/GrirEditorSlice.ts';
import { RootState } from 'src/stores/store.ts';
import { PropertyEditor } from "src/components/editors/propertyEditor/PropertyEditor.tsx";
import {
} from "src/components/editors/xsltEditor/XsltEditor.tsx";
import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTreeSlice.ts";

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
  useEffect(() => {
    if (!editorState) {
      dispatch(initializeEditor(node) as any);
    }
  }, [editorId, dispatch, editorState]);
  return editorId;
}
