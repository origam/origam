import { useEffect, useContext } from "react";
import { useDispatch, useSelector } from 'react-redux';
import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import S from 'src/components/gridEditor/GridEditor.module.scss';
import { ArchitectApiContext } from "src/API/ArchitectApiContext.tsx";
import { initializeEditor, updateProperty } from './GrirEditorSlice.ts';
import { RootState } from 'src/stores/store.ts';

export interface EditorProperty {
  name: string;
  type: string;
  value: any;
  category: string;
  description: string;
  readOnly: boolean;
}

export interface PropertyChange {
  name: string;
  value: any;
}

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

  const handleInputChange = (propertyName: string, value: any) => {
    dispatch(updateProperty({editorId, propertyName, value}));
  };

  if (!editorState) return null;

  const groupedProperties = editorState.properties.reduce((groups: {
    [key: string]: EditorProperty[]
  }, property) => {
    (groups[property.category] = groups[property.category] || []).push(property);
    return groups;
  }, {});

  const sortedCategories = Object.keys(groupedProperties).sort();

  return (
    <div className={S.gridEditor}>
      <h3 className={S.title}>{`Editing: ${props.node.nodeText}`}</h3>
      <div>
        {sortedCategories.map(category => (
          <div className={S.category} key={category}>
            <h4>{category}</h4>
            {groupedProperties[category].map((property: EditorProperty) => (
              <div className={S.property} key={property.name}>
                <div className={S.propertyName}>{property.name}</div>
                <input
                  disabled={property.readOnly}
                  value={property.value ? property.value : undefined}
                  onChange={(e) => handleInputChange(property.name, e.target.value)}
                />
              </div>
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}