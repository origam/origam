import React from "react";
import {
  ScreenSectionEditorState
} from "src/components/editors/screenSectionEditor/ScreenSectionEditorState.tsx";
import { observer } from "mobx-react-lite";
import { IEditorField } from "src/API/IArchitectApi.ts";
import { action } from "mobx";
import {
  ComponentType,
  getComponentTypeKey,
  toComponentType
} from "src/components/editors/screenSectionEditor/ComponentType.tsx";
import S from "src/components/editors/screenSectionEditor/Toolbox.module.scss";
import {
  Toolbox
} from "src/components/editors/screenSectionEditor/Toolbox.tsx";

export const SectionToolbox: React.FC<{
  designerState: ScreenSectionEditorState
}> = observer((props) => {
  const surfaceState = props.designerState.surface;
  const toolboxState = props.designerState.toolbox;

  const onFieldDragStart = (field: IEditorField) => {
    action(() => {
      surfaceState.draggedComponentData = {
        fieldName: field.name,
        type: toComponentType(field.type)
      };
    })();
  };

  const onControlDragStart = (type: ComponentType) => {
    action(() => {
      if (toolboxState.selectedFieldName || type === ComponentType.GroupBox) {
        surfaceState.draggedComponentData = {
          fieldName: toolboxState.selectedFieldName,
          type: type
        };
      }
    })();
  };

  function getToolboxComponent(field: IEditorField) {
    const isSelected = toolboxState.selectedFieldName === field.name;
    return (
      <div
        key={field.name}
        draggable
        onClick={() => toolboxState.selectedFieldName = field.name}
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
        label: "Fields",
        node: <div className={S.draggAbles}>
          {toolboxState.fields.map(field => getToolboxComponent(field))}
        </div>
      },
      {
        label: "Widgets",
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