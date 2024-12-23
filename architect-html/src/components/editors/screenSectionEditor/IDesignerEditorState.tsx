import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import {
  DesignSurfaceState
} from "src/components/editors/screenSectionEditor/DesignSurfaceState.tsx";
import {
  Component
} from "src/components/editors/screenSectionEditor/Component.tsx";

export interface IDesignerEditorState extends IEditorState {
  surface: DesignSurfaceState;

  deleteComponent(component: Component): any;

  onDesignerMouseUp(x: number, y: number): any;

  createDraggedComponent(x: number, y: number): any;
}