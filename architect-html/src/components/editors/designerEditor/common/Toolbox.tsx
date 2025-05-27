import React, { useContext } from "react";
import { observer } from "mobx-react-lite";
import { RootStoreContext, T } from "src/main.tsx";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import S from "src/components/editors/designerEditor/common/Toolbox.module.scss";
import { ITabViewItem, TabView } from "src/components/tabView/TabView.tsx";
import {
  ToolboxState
} from "src/components/editors/designerEditor/common/ToolboxState.tsx";

export const Toolbox: React.FC<{
  toolboxState: ToolboxState,
  tabViewItems: ITabViewItem[]
}> = observer(({toolboxState, tabViewItems}) => {
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);

  return (
    <div className={S.toolbox}>
      <div className={S.inputs}>
        <div className={S.inputContainer}>
          <div>
            {T("Data Source", "tool_box_data_source")}
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
            {T("Name", "tool_box_name")}
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
            {T("Package", "tool_box_package")}
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
        items={tabViewItems}
      />
    </div>
  );
});