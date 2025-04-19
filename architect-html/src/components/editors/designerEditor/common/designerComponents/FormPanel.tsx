import { ReactElement } from "react";
import {
  IComponentData
} from "src/components/editors/designerEditor/common/ComponentType.tsx";
import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import {
  Component
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";

export class FormPanel extends Component {

  reactElement: ReactElement;

  constructor(args: {
    id: string,
    parent: Component | null,
    data: IComponentData,
    properties: EditorProperty[],
    reactElement: ReactElement,
  }) {
    super(args);
    this.reactElement = args.reactElement;
  }

  getDesignerRepresentation(): ReactElement | null {
    return this.reactElement;
  }
}