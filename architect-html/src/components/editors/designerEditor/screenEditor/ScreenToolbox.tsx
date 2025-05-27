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
import { T } from "src/main.tsx";

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
        label: T("Screen Sections", "screen_tool_box_tab1"),
        node: <div className={S.draggAbles}>
          {toolboxState.sections.map(section => getToolboxComponent(section))}
        </div>
      },
      {
        label: T("Widgets", "screen_tool_box_tab2"),
        node: <div className={S.draggAbles}>
          {toolboxState.widgets.map(widget => getToolboxComponent(widget))}
        </div>
      }
    ]}
  />;
});