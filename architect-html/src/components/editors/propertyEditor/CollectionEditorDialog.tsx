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

import { T } from '@/main.tsx';
import { ICollectionMember } from '@api/IArchitectApi';
import { ModalWindow } from '@dialogs/ModalWindow';
import { EditorProperty } from '@editors/gridEditor/EditorProperty.ts';
import S from '@editors/propertyEditor/CollectionEditorDialog.module.scss';
import { IPropertyManager } from '@editors/propertyEditor/IPropertyManager.tsx';
import PropertyEditor from '@editors/propertyEditor/PropertyEditor.tsx';
import cn from 'classnames';
import { observer } from 'mobx-react-lite';
import { useMemo, useState } from 'react';

const readOnlyPropertyManager: IPropertyManager = {
  *onPropertyUpdated() {},
};

export const CollectionEditorDialog = observer(
  (props: { propertyName: string; members: ICollectionMember[]; onClose: () => void }) => {
    const [selectedIndex, setSelectedIndex] = useState(0);
    const selectedMember = props.members[selectedIndex];
    const properties = useMemo(
      () => selectedMember?.properties.map(property => new EditorProperty(property)) ?? [],
      [selectedMember],
    );

    return (
      <ModalWindow
        title={T('{0} Collection Editor', 'collection_editor_title', props.propertyName)}
        buttonsCenter={
          <button data-test-id="collection-editor-close" onClick={props.onClose}>
            {T('Close', 'dialog_close')}
          </button>
        }
      >
        <div className={S.content} data-test-id="collection-editor">
          <div className={S.column}>
            <div className={S.label}>{T('Members:', 'collection_editor_members')}</div>
            <div className={S.members} role="listbox">
              {props.members.map((member, index) => (
                <button
                  type="button"
                  key={index}
                  role="option"
                  aria-selected={index === selectedIndex}
                  className={cn(S.member, { [S.selected]: index === selectedIndex })}
                  onClick={() => setSelectedIndex(index)}
                  data-test-id={`collection-member-${member.name}`}
                >
                  {member.name}
                </button>
              ))}
            </div>
          </div>
          <div className={cn(S.column, S.propertiesColumn)}>
            <div className={S.label}>{T('Properties:', 'collection_editor_properties')}</div>
            <div className={S.properties}>
              <PropertyEditor properties={properties} propertyManager={readOnlyPropertyManager} />
            </div>
          </div>
        </div>
      </ModalWindow>
    );
  },
);
