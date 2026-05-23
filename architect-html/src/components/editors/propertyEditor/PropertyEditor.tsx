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

import { RootStoreContext } from '@/main';
import { EditorProperty } from '@editors/gridEditor/EditorProperty';
import { IPropertyManager } from '@editors/propertyEditor/IPropertyManager';
import S from '@editors/propertyEditor/PropertyEditor.module.scss';
import SinglePropertyEditor from '@editors/propertyEditor/SinglePropertyEditor.tsx';
import { getSortedProperties } from '@editors/propertyEditor/utils';
import cn from 'classnames';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { VscChevronDown, VscChevronRight } from 'react-icons/vsc';

const PropertyEditor = observer(
  (props: {
    properties: EditorProperty[];
    propertyManager: IPropertyManager;
    compact?: boolean;
  }) => {
    const { uiState } = useContext(RootStoreContext);

    if (!props.properties) {
      return null;
    }

    const { groupedProperties, sortedCategories } = getSortedProperties(props.properties);

    return (
      <div className={cn(S.root, { [S.compact]: props.compact })}>
        {sortedCategories.map(category => {
          const categoryKey = category ?? 'Misc';
          const collapsed = !props.compact && uiState.isPropertySectionCollapsed(categoryKey);
          const Chevron = collapsed ? VscChevronRight : VscChevronDown;
          return (
            <div className={S.category} key={category}>
              {!props.compact && (
                <button
                  type="button"
                  className={S.categoryHeader}
                  onClick={() => uiState.togglePropertySectionCollapsed(categoryKey)}
                  aria-expanded={!collapsed}
                >
                  <span>{categoryKey}</span>
                  <Chevron className={S.chevron} />
                </button>
              )}
              {!collapsed &&
                groupedProperties[category].map((property: EditorProperty) => (
                  <div className={S.property} key={property.name}>
                    <div
                      title={property.error}
                      className={cn(S.propertyName, { [S.errorProperty]: property.error })}
                    >
                      {property.name}
                    </div>
                    <SinglePropertyEditor
                      property={property}
                      propertyManager={props.propertyManager}
                      compact={props.compact}
                    />
                  </div>
                ))}
            </div>
          );
        })}
      </div>
    );
  },
);

export default PropertyEditor;
