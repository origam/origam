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

import React from "react";
import {
  ScreenSectionEditorState
} from "src/components/editors/designerEditor/screenSectionEditor/ScreenSectionEditorState.tsx";
import { observer } from "mobx-react-lite";
import { IEditorField } from "src/API/IArchitectApi.ts";
import { action } from "mobx";
import {
  ComponentType,
  getComponentTypeKey,
  toComponentType
} from "src/components/editors/designerEditor/common/ComponentType.tsx";
import S from "src/components/editors/designerEditor/common/Toolbox.module.scss";
import {
  Toolbox
} from "src/components/editors/designerEditor/common/Toolbox.tsx";
import { T } from "src/main.tsx";

export const SectionToolbox: React.FC<{
  designerState: ScreenSectionEditorState
}> = observer((props) => {
  const surfaceState = props.designerState.surface;
  const sectionToolbox = props.designerState.sectionToolbox;

  const onFieldDragStart = (field: IEditorField) => {
    action(() => {
      surfaceState.draggedComponentData = {
        identifier: field.name,
        type: toComponentType(field.type)
      };
    })();
  };

  const onControlDragStart = (type: ComponentType) => {
    action(() => {
      if (sectionToolbox.selectedFieldName || type === ComponentType.GroupBox) {
        surfaceState.draggedComponentData = {
          identifier: sectionToolbox.selectedFieldName,
          type: type
        };
      }
    })();
  };

  function getToolboxComponent(field: IEditorField) {
    const isSelected = sectionToolbox.selectedFieldName === field.name;
    return (
      <div
        key={field.name}
        draggable
        onClick={() => sectionToolbox.selectedFieldName = field.name}
        onDragStart={() => onFieldDragStart(field)}
        className={S.toolboxField + " " + (isSelected ? S.selectedField : "")}
      >
        <div className={S.toolboxFieldIcon}>
        </div>
        <div>
          {field.name}
        </div>
      </div>
    );
  }

  function getControlComponent(type: ComponentType) {
    return (
      <div
        key={type}
        draggable
        onDragStart={() => onControlDragStart(type)}
        className={S.toolboxField}
      >
        <div className={S.toolboxFieldIcon}>
        </div>
        <div>
          {getComponentTypeKey(type)}
        </div>
      </div>
    );
  }

  return <Toolbox
    toolboxState={props.designerState.toolbox}
    tabViewItems={[
      {
        label: T("Fields", "section_tool_box_tab1"),
        node: <div className={S.draggAbles}>
          {sectionToolbox.fields.map(field => getToolboxComponent(field))}
        </div>
      },
      {
        label: T("Widgets", "section_tool_box_tab2"),
        node: <div className={S.draggAbles}>
          {getControlComponent(ComponentType.AsCheckBox)}
          {getControlComponent(ComponentType.AsCombo)}
          {getControlComponent(ComponentType.AsDateBox)}
          {getControlComponent(ComponentType.AsTextBox)}
          {getControlComponent(ComponentType.GroupBox)}
        </div>
      }
    ]}
  />;
});

