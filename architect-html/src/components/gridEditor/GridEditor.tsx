import { useEffect, useState } from "react";
import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import axios from "axios";


export function GridEditor(props: {
  node: TreeNode
  onBackClick: () => void
}) {
  const [properties, setProperties] = useState<EditorProperty[]>([])
  useEffect(() => {
    async function getData (){
      const newProperties = (await axios.get("/Editor/EditableProperties", {
        params: {schemaItemId: props.node.id}
      })).data;
      setProperties(newProperties);
    }
    getData();
  }, []);

   const groupedProperties = properties.reduce((groups: {[key: string]: any}, property) => {
    (groups[property.category] = groups[property.category] || []).push(property);
    return groups;
  }, {});

  const sortedCategories = Object.keys(groupedProperties).sort();

  return (
    <div>
      <h3>{`Editing: ${props.node.nodeText}`}</h3>
      <button onClick={props.onBackClick}>Back</button>
      <div>{sortedCategories.map(category =>
        <div>
          <h4>{category}</h4>
          {groupedProperties[category].map((x: EditorProperty) => (
            <div>
              <div>{x.name}</div>
              <input value={x.value}></input>
            </div>
          ))}
        </div>
      )}
      </div>
    </div>
  );
}

interface EditorProperty {
  name: string;
  type: string;
  value: any;
  category: string;
  description: string;
}