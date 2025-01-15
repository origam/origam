import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import {
  DesignSurfaceState
} from "src/components/editors/designerEditor/common/DesignSurfaceState.tsx";
import {
  Component
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";

export interface IDesignerEditorState extends IEditorState {
  surface: DesignSurfaceState;

  delete(component: Component): any;

  onDesignerMouseUp(x: number, y: number): any;

  create(x: number, y: number): any;
}