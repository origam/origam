import {
  screenLayer
} from "src/components/editors/designerEditor/common/Layers.ts";
import { ReactElement } from "react";
import S
  from "src/components/editors/designerEditor/common/designerComponents/Components.module.scss";
import {
  Component
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";

export class AsForm extends Component {
  get canHaveChildren(): boolean {
    return true;
  }

  get zIndex(): number {
    return screenLayer;
  }

  getDesignerRepresentation(): ReactElement | null {
    return (
      <div className={S.panel}>
      </div>
    );
  }
}