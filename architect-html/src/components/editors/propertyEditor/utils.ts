import { EditorProperty } from '@editors/gridEditor/EditorProperty';

export function getSortedProperties(properties: EditorProperty[]) {
  const groupedProperties = properties.reduce(
    (
      groups: {
        [key: string]: EditorProperty[];
      },
      property,
    ) => {
      (groups[property.category ?? 'Misc'] = groups[property.category ?? 'Misc'] || []).push(
        property,
      );
      return groups;
    },
    {},
  );

  const sortedCategories = Object.keys(groupedProperties).sort();
  for (const category of sortedCategories) {
    groupedProperties[category].sort(category === 'Layout' ? sortLayoutProperties : sortProperties);
  }
  return { groupedProperties, sortedCategories };
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
