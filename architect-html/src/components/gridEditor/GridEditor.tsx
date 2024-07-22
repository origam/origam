import { useState, useEffect, useContext } from "react";
import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import S from 'src/components/gridEditor/GridEditor.module.scss';
import { ArchitectApiContext } from "src/API/ArchitectApiContext.tsx";

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
  const architectApi = useContext(ArchitectApiContext)!;
  useEffect(() => {
    async function getData() {
      try {
        const newProperties = await architectApi.getProperties(props.node.id);
        setProperties(newProperties);
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
    <div className={S.gridEditor}>
      <h3 className={S.title}>{`Editing: ${props.node.nodeText}`}</h3>
      <button className={S.backButton} onClick={props.onBackClick}>Back</button>
      <div>
        {sortedCategories.map(category => (
          <div className={S.category} key={category}>
            <h4>{category}</h4>
            {groupedProperties[category].map((property: EditorProperty) => (
              <div className={S.property} key={property.name}>
                <div className={S.propertyName}>{property.name}</div>
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