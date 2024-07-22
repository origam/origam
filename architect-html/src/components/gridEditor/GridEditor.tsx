import { useEffect } from "react";
import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import axios from "axios";
import { useDispatch, useSelector } from "react-redux";
import { add, selectEditorById, updatePropertyValue } from "src/components/gridEditor/GridEditorSlice.ts";

export function GridEditor(props: {
  node: TreeNode
  onBackClick: () => void
}) {
  const editorId = props.node.nodeText + props.node.id;
  const editorState = useSelector((state: any) => selectEditorById(state, editorId))
    || {id:"", properties:[]};
  const dispatch = useDispatch();

  useEffect(() => {
    async function getData (){
      const newProperties = (await axios.get("/Editor/EditableProperties", {
        params: {schemaItemId: props.node.id}
      })).data;
      dispatch(add({id: editorId, properties: newProperties}));
    }
    getData();
  }, []);

  const handleInputChange = (propertyName: string, value: any) => {
    dispatch(updatePropertyValue({ id: editorId, propertyName, value }));
  };

  const groupedProperties = editorState.properties.reduce((groups: {[key: string]: any}, property) => {
    (groups[property.category] = groups[property.category] || []).push(property);
    return groups;
  }, {});

  const sortedCategories = Object.keys(groupedProperties).sort();

  return (
    <div>
      <h3>{`Editing: ${props.node.nodeText}`}</h3>
      <button onClick={props.onBackClick}>Back</button>
      <div>{sortedCategories.map(category =>
        <div key={category}>
          <h4>{category}</h4>
          {groupedProperties[category].map((x: EditorProperty) => (
            <div key={x.name}>
              <div>{x.name}</div>
              <input
                value={x.value}
                onChange={(e) => handleInputChange(x.name, e.target.value)}
              />
            </div>
          ))}
        </div>
      )}
      </div>
    </div>
  );
}

export interface EditorProperty {
  name: string;
  type: string;
  value: any;
  category: string;
  description: string;
}

export interface EditorState {
  id: string;
  properties: EditorProperty[];
}
