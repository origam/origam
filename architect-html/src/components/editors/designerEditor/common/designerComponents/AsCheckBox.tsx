import { ReactElement } from "react";
import S
  from "src/components/editors/designerEditor/common/designerComponents/Components.module.scss";
import {
  Component
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";

export class AsCheckBox extends Component {
  getDesignerRepresentation(): ReactElement | null {
    return (
      <div className={S.designSurfaceEditorContainer}>
        <div className={S.designSurfaceCheckbox}></div>
        <div>{this.properties.find(x => x.name === "Text")?.value}</div>
      </div>
    );
  }
}