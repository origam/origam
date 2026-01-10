/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

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
