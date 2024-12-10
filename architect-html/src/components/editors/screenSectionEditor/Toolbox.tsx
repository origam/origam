import React, { useContext } from "react";
import {
  ComponentDesignerState
} from "src/components/editors/screenSectionEditor/ComponentDesignerState.tsx";
import { observer } from "mobx-react-lite";
import { RootStoreContext } from "src/main.tsx";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import { IEditorField } from "src/API/IArchitectApi.ts";
import { action } from "mobx";
import {
  toComponentType
} from "src/components/editors/screenSectionEditor/ComponentType.tsx";
import S
  from "src/components/editors/screenSectionEditor/Toolbox.module.scss";
import { TabView } from "src/components/tabView/TabView.tsx";

export const Toolbox: React.FC<{
  designerState: ComponentDesignerState
}> = observer((props) => {
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const surfaceState = props.designerState.surface;
  const toolboxState = props.designerState.toolbox;

  const onDragStart = (field: IEditorField) => {
    action(() => {
      surfaceState.draggedComponentData = {
        fieldName: field.name,
        type: toComponentType(field.type)
      };
    })();
  };

  function getToolboxComponent(field: IEditorField) {
    return (
      <div
        key={field.name}
        draggable
        onDragStart={() => onDragStart(field)}
        className={S.toolboxField}
      >
        <div className={S.toolboxFieldIcon}>

        </div>
        <div>
          {field.name}
        </div>
      </div>
    );
  }

  return (
    <div className={S.toolbox}>
      <div className={S.inputs}>
        <div className={S.inputContainer}>
          <div>
            Data Source
          </div>
          <select
            value={toolboxState.selectedDataSourceId ?? ""}
            onChange={(e) => run({generator: toolboxState.selectedDataSourceIdChanged(e.target.value)})}
          >
            {toolboxState.dataSources.map(x =>
              <option
                key={x.schemaItemId + x.name}
                value={x.schemaItemId}>{x.name}
              </option>)
            }
          </select>
        </div>
        <div className={S.inputContainer}>
          <div>
            Name
          </div>
          <input
            type="text"
            value={toolboxState.name}
            onChange={(e) => run({generator: toolboxState.nameChanged(e.target.value)})}
          />
        </div>
        <div className={S.inputContainer}>
          <div>
            Id
          </div>
          <input
            disabled={true}
            type="text"
            value={toolboxState.id}
          />
        </div>
        <div className={S.inputContainer}>
          <div>
            Package
          </div>
          <input
            disabled={true}
            type="text"
            value={toolboxState.schemaExtensionId}
          />
        </div>
      </div>
      <TabView
        width={260}
        state={toolboxState.tabViewState}
        items={[
          {
            label: "Fields",
            node: <div className={S.fields}>
              {toolboxState.fields.map(field => getToolboxComponent(field))}
              {/*<div*/}
              {/*  className={S.toolItem}*/}
              {/*  draggable*/}
              {/*  onDragStart={() => onDragStart('Label')}*/}
              {/*>*/}
              {/*  Label*/}
              {/*</div>*/}
              {/*<div*/}
              {/*  className={S.toolItem}*/}
              {/*  draggable*/}
              {/*  onDragStart={() => onDragStart('GroupBox')}*/}
              {/*>*/}
              {/*  GroupBox*/}
              {/*</div>*/}
            </div>
          },
          {
            label: "Widgets",
            node: <div/>
          }
        ]}
      />

    </div>
  );
});