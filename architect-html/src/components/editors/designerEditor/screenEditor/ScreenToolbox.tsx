import React from "react";
import { observer } from "mobx-react-lite";
import { IToolBoxItem } from "src/API/IArchitectApi.ts";
import S from "src/components/editors/designerEditor/common/Toolbox.module.scss";
import {
  Toolbox
} from "src/components/editors/designerEditor/common/Toolbox.tsx";
import {
  ScreenEditorState
} from "src/components/editors/designerEditor/screenEditor/ScreenEditorState.tsx";
import {
  ComponentType
} from "src/components/editors/designerEditor/common/ComponentType.tsx";
import { action } from "mobx";

export const ScreenToolbox: React.FC<{
  designerState: ScreenEditorState
}> = observer((props) => {
  const surfaceState = props.designerState.surface;
  const toolboxState = props.designerState.screenToolbox;

  const onFieldDragStart = (section: IToolBoxItem) => {
    action(() => {
      surfaceState.draggedComponentData = {
        identifier: section.id,
        type: ComponentType.FormPanel
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
        <div className={S.toolboxFieldIcon}>
        </div>
        <div>
          {section.name}
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
          {toolboxState.sections.map(section => getToolboxComponent(section))}
        </div>
      },
      {
        label: "Widgets",
        node: <div className={S.draggAbles}>
          {toolboxState.widgets.map(widget => getToolboxComponent(widget))}
        </div>
      }
    ]}
  />;
});