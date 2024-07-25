import { useContext, useEffect } from "react";
import { useDispatch, useSelector } from 'react-redux';
import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import S from 'src/components/editors/gridEditor/GridEditor.module.scss';
import { ArchitectApiContext } from "src/API/ArchitectApiContext.tsx";
import {
  initializeEditor
} from 'src/components/editors/gridEditor/GrirEditorSlice.ts';
import { RootState } from 'src/stores/store.ts';
import { PropertyEditor } from "src/components/editors/propertyEditor/PropertyEditor.tsx";

export function GridEditor(props: {
  node: TreeNode
}) {
  const dispatch = useDispatch();
  const editorId = props.node.nodeText + "_" + props.node.id;
  const editorState = useSelector((state: RootState) => state.editorStates.editors[editorId]);
  const architectApi = useContext(ArchitectApiContext)!;

  useEffect(() => {
    async function getData() {
      try {
        const newProperties = await architectApi.getProperties(props.node.id);
        dispatch(initializeEditor({
          editorId,
          schemaItemId: props.node.id,
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

  return (
    <div className={S.gridEditor}>
      <h3 className={S.title}>{`Editing: ${props.node.nodeText}`}</h3>
      <PropertyEditor
        editorId={editorId}
        properties={editorState?.properties}/>
    </div>
  );
}