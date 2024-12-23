import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import {
  DesignSurfaceState
} from "src/components/editors/designerEditor/common/DesignSurfaceState.tsx";
import {
  Component
} from "src/components/editors/designerEditor/common/Component.tsx";

export interface IDesignerEditorState extends IEditorState {
  surface: DesignSurfaceState;

  deleteComponent(component: Component): any;

  onDesignerMouseUp(x: number, y: number): any;

  createDraggedComponent(x: number, y: number): any;
}