import {
  IArchitectApi, IScreenEditorData, IScreenEditorModel, IToolBoxItem
} from "src/API/IArchitectApi.ts";
import {
  ToolboxState
} from "src/components/editors/designerEditor/common/ToolboxState.tsx";

export class ScreenToolboxState {
  toolboxState: ToolboxState;
  sections: IToolBoxItem[];
  widgets: IToolBoxItem[];

  constructor(
    screenEditorData: IScreenEditorData,
    id: string,
    private architectApi: IArchitectApi
  ) {
    this.sections = screenEditorData.sections;
    this.widgets = screenEditorData.widgets;
    this.toolboxState = new ToolboxState(
      screenEditorData.dataSources,
      screenEditorData.name,
      screenEditorData.schemaExtensionId,
      screenEditorData.selectedDataSourceId,
      id,
      this.updateTopProperties.bind(this))
  }

  private updateTopProperties() {
    return function* (this: ScreenToolboxState): Generator<Promise<IScreenEditorModel>, void, IScreenEditorModel> {
      const updateResult = yield this.architectApi.updateScreenEditor({
        schemaItemId: this.toolboxState.id,
        name: this.toolboxState.name,
        selectedDataSourceId: this.toolboxState.selectedDataSourceId,
        modelChanges: []
      });
      const newData = updateResult.data;
      this.toolboxState.name = newData.name;
      this.toolboxState.selectedDataSourceId = newData.selectedDataSourceId;
    }.bind(this);
  }
}