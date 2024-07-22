import React, { useEffect, useState } from "react";
import { TreeNode } from "./LazyLoadedTree.tsx";
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

  return (
    <div>
      <h3>{`Editing: ${props.node.nodeText}`}</h3>
      <button onClick={props.onBackClick}>Back</button>
      <div>{properties.map(x=>
        <div>
          <div>{x.name}</div>
          <input value={x.value}></input>
        </div>
      )}</div>
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