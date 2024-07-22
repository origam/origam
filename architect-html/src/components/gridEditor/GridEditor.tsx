import React, { useState, useEffect } from "react";
import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import axios from "axios";

export interface EditorProperty {
  name: string;
  type: string;
  value: any;
  category: string;
  description: string;
  readOnly: boolean;
}

export function GridEditor(props: {
  node: TreeNode
  onBackClick: () => void
}) {
  const [properties, setProperties] = useState<EditorProperty[]>([]);

  useEffect(() => {
    async function getData() {
      try {
        const response = await axios.get("/Editor/EditableProperties", {
          params: { schemaItemId: props.node.id }
        });
        setProperties(response.data);
      } catch (error) {
        console.error("Error fetching properties:", error);
      }
    }
    getData();
  }, [props.node.id]);

  const handleInputChange = (propertyName: string, value: any) => {
    setProperties(prevProperties =>
      prevProperties.map(prop =>
        prop.name === propertyName ? { ...prop, value } : prop
      )
    );
  };

  const groupedProperties = properties.reduce((groups: {[key: string]: EditorProperty[]}, property) => {
    (groups[property.category] = groups[property.category] || []).push(property);
    return groups;
  }, {});

  const sortedCategories = Object.keys(groupedProperties).sort();

  return (
    <div>
      <h3>{`Editing: ${props.node.nodeText}`}</h3>
      <button onClick={props.onBackClick}>Back</button>
      <div>
        {sortedCategories.map(category => (
          <div key={category}>
            <h4>{category}</h4>
            {groupedProperties[category].map((property: EditorProperty) => (
              <div key={property.name}>
                <div>{property.name}</div>
                <input
                  disabled={property.readOnly}
                  value={property.value}
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