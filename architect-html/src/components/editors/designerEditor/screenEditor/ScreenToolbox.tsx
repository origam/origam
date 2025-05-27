/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o. 

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

import React from 'react';
import { observer } from 'mobx-react-lite';
import { IToolBoxItem } from 'src/API/IArchitectApi.ts';
import S from 'src/components/editors/designerEditor/common/Toolbox.module.scss';
import { Toolbox } from 'src/components/editors/designerEditor/common/Toolbox.tsx';
import { ScreenEditorState } from 'src/components/editors/designerEditor/screenEditor/ScreenEditorState.tsx';
import { ComponentType } from 'src/components/editors/designerEditor/common/ComponentType.tsx';
import { action } from 'mobx';

export const ScreenToolbox: React.FC<{
  designerState: ScreenEditorState;
}> = observer(props => {
  const surfaceState = props.designerState.surface;
  const toolboxState = props.designerState.screenToolbox;

  const onFieldDragStart = (section: IToolBoxItem) => {
    action(() => {
      surfaceState.draggedComponentData = {
        identifier: section.id,
        type: ComponentType.FormPanel,
      };
    })();
  };

  function getToolboxComponent(section: IToolBoxItem) {
    return (
      <div
        key={section.name}
        draggable
        onDragStart={() => onFieldDragStart(section)}
        className={S.toolboxField}
      >
        <div className={S.toolboxFieldIcon}></div>
        <div>{section.name}</div>
      </div>
    );
  }

  return (
    <Toolbox
      toolboxState={props.designerState.toolbox}
      tabViewItems={[
        {
          label: 'Screen Sections',
          node: (
            <div className={S.draggAbles}>
              {toolboxState.sections.map(section => getToolboxComponent(section))}
            </div>
          ),
        },
        {
          label: 'Widgets',
          node: (
            <div className={S.draggAbles}>
              {toolboxState.widgets.map(widget => getToolboxComponent(widget))}
            </div>
          ),
        },
      ]}
    />
  );
});
