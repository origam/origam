import {
  screenLayer
} from "src/components/editors/designerEditor/common/Layers.ts";
import { ReactElement } from "react";
import S
  from "src/components/editors/designerEditor/common/designerComponents/Components.module.scss";
import {
  Component
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";
import {
  IComponentData
} from "src/components/editors/designerEditor/common/ComponentType.tsx";
import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";

const ChildGap = 10;

export class SplitPanel extends Component {

  get canHaveChildren(): boolean {
    return true;
  }

  get zIndex(): number {
    return this.countParents() + screenLayer;
  }

  private readonly getChildren: (component: Component) => Component[];

  constructor(args: {
    id: string,
    parent: Component | null,
    data: IComponentData,
    properties: EditorProperty[],
    getChildren: (component: Component) => Component[]
  })
  {
    super(args);
    this.getChildren = args.getChildren;
  }

  update() {
    const children = this.getChildren(this);
    if (children.length == 0) {
      return;
    }
    if (children.length > 2) {
      throw new Error("Split panel cannot have more than 2 children");
    }
    const orientation = this.get("Orientation");
    switch (orientation) {
      case Orientation.Horizontal: {
        const {upperChild, lowerChild} = this.getUpperAndLowerChild(children);
        upperChild.relativeTop = ChildGap;
        upperChild.relativeLeft = ChildGap;
        upperChild.width = Math.round(this.width - ChildGap * 2);
        upperChild.height = Math.round(this.height / 2 - ChildGap * 1.5);
        if (lowerChild) {
          lowerChild.relativeTop = Math.round(this.height /2 + ChildGap * 0.5);
          lowerChild.relativeLeft = ChildGap;
          lowerChild.width = Math.round(this.width - ChildGap * 2);
          lowerChild.height = Math.round(this.height / 2 - ChildGap * 1.5);
        }
        break;
      }
      case Orientation.Vertical:
        return;
      default:
        throw new Error(`Unknown split panel orientation "${orientation}"`);
    }
  }

  getUpperAndLowerChild(children: Component[]) {
    if (children.length == 1) {
      return {
        upperChild: children[0],
        lowerChild: undefined
      }
    }
    if (children[0].relativeTop > children[1].relativeLeft) {
      return {
        upperChild: children[0],
        lowerChild: children[1]
      }
    }
    return {
      upperChild: children[1],
      lowerChild: children[0]
    }
  }

  getDesignerRepresentation(): ReactElement | null {
    return (
      <div className={S.groupBoxContent}>
      </div>
    );
  }
}

enum Orientation {Horizontal, Vertical}