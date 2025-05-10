import S
  from "src/components/editors/propertyEditor/PropertyEditor.module.scss";
import { observer } from "mobx-react-lite";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import { RootStoreContext } from "src/main.tsx";
import { useContext } from "react";
import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import {
  IPropertyManager
} from "src/components/editors/propertyEditor/IPropertyManager.tsx";
import {
  NumericPropertyInput
} from "src/components/editors/propertyEditor/NumericPropertyInput.tsx";

export const PropertyEditor: React.FC<{
  properties: EditorProperty[];
  propertyManager: IPropertyManager;
}> = observer((props) => {
  const rootStore = useContext(RootStoreContext);

  if (!props.properties) {
    return null;
  }

  function onValueChange(property: EditorProperty, value: any) {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: function* () {
        const parsedValue = property.type === "enum"
          ? parseInt(value)
          : value;
        yield* props.propertyManager.onPropertyUpdated(property, parsedValue);
      },
    });
  }

  const {
    groupedProperties,
    sortedCategories
  } = getSortedProperties(props.properties);

  function renderPropertyEditor(property: EditorProperty) {
    if (property.type === "enum" || property.type === "looukup") {
      return (
        <select
          value={property.value ?? ""}
          onChange={(e) => onValueChange(property, e.target.value)}>
          {property.dropDownValues.map(x =>
            <option
              key={property.value + x.name}
              value={x.value}>{x.name}
            </option>)
          }
        </select>
      )
    }
    if (property.type === "boolean") {
      return (
        <div className={S.checkboxContainer}>
          <input
            type="checkbox"
            checked={property.value}
            onChange={(e) => onValueChange(property, e.target.checked)}
            disabled={property.readOnly}
            className={S.checkbox}
          />
        </div>
      )
    }
    if (property.type === "integer" || property.type === "float") {
      return (
        <NumericPropertyInput
          property={property}
          type={property.type}
          onChange={(value) => onValueChange(property, value)}
        />
      );
    }
    return (
      <input
        type="text"
        disabled={property.readOnly}
        value={property.value != null ? property.value : undefined}
        onChange={(e) => onValueChange(property, e.target.value)}
      />
    );
  }

  return (
    <div className={S.root}>
      {sortedCategories.map(category => (
        <div className={S.category} key={category}>
          <h4>{category ?? "Misc"}</h4>
          {groupedProperties[category].map((property: EditorProperty) => (
            <div className={S.property} key={property.name}>
              <div
                title={property.error}
                className={S.propertyName + (property.error ? " " + S.errorProperty : "")}
              >
                {property.name}
              </div>
              {renderPropertyEditor(property)}
            </div>
          ))}
        </div>
      ))}
    </div>
  );
});


function getSortedProperties(properties: EditorProperty[]) {
  const groupedProperties = properties.reduce((groups: {
    [key: string]: EditorProperty[]
  }, property) => {
    (groups[property.category ?? "Misc"] = groups[property.category ?? "Misc"] || []).push(property);
    return groups;
  }, {});

  const sortedCategories = Object.keys(groupedProperties).sort();
  for (const category of sortedCategories) {
    groupedProperties[category].sort(
      category === "Layout"
        ? sortLayoutProperties
        : sortProperties);
  }
  return {groupedProperties, sortedCategories};
}

function sortProperties(a: EditorProperty, b: EditorProperty) {
  return a.name.localeCompare(b.name);
}

function sortLayoutProperties(a: EditorProperty, b: EditorProperty) {
  const order: { [key: string]: number } = {
    Left: 1,
    Top: 2,
    Width: 3,
    Height: 4,
  };

  const priorityA = order[a.name] ?? Number.MAX_SAFE_INTEGER;
  const priorityB = order[b.name] ?? Number.MAX_SAFE_INTEGER;

  return priorityA - priorityB;
}