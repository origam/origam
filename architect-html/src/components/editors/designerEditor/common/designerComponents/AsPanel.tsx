import {
  sectionLayer
} from "src/components/editors/designerEditor/common/Layers.ts";
import { ReactElement } from "react";
import S
  from "src/components/editors/designerEditor/common/designerComponents/Components.module.scss";
import {
  Component
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";

export class AsPanel extends Component {
  get canHaveChildren(): boolean {
    return true;
  }

  get zIndex(): number {
    return sectionLayer;
  }

  getDesignerRepresentation(): ReactElement | null {
    return (
      <div className={S.panel}>
      </div>
    );
  }
}