import {
  ComponentType,
  IComponentData,
  parseComponentType
} from "src/components/editors/designerEditor/common/ComponentType.tsx";
import { observable } from "mobx";
import { IApiControl } from "src/API/IArchitectApi.ts";

import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import {
  controlLayer,
  screenLayer,
  sectionLayer
} from "src/components/editors/designerEditor/common/Layers.ts";
import {
  LabelPosition,
  parseLabelPosition
} from "src/components/editors/designerEditor/common/LabelPosition.tsx";
import S
  from "src/components/editors/designerEditor/common/DesignerSurface.module.scss";
import { ReactElement } from "react";
import {
  SectionItem
} from "src/components/editors/designerEditor/common/SectionItem.tsx";

export class Component {
  id: string;
  @observable.ref accessor parent: Component | null;
  data: IComponentData;
  @observable accessor properties: EditorProperty[];
  @observable.ref accessor designerRepresentation: ReactElement | null;

  get relativeLeft(): number {
    return this.getProperty("Left")!.value;
  }

  set relativeLeft(value: number) {
    this.getProperty("Left")!.value = value;
  }

  get relativeTop(): number {
    return this.getProperty("Top")!.value;
  }

  set relativeTop(value: number) {
    this.getProperty("Top")!.value = value;
  }

  get absoluteLeft(): number {
    return this.relativeLeft + (this.parent?.absoluteLeft ?? 0);
  }

  set absoluteLeft(value: number) {
    this.relativeLeft = value - (this.parent?.absoluteLeft ?? 0);
  }

  get absoluteRight(): number {
    return this.absoluteLeft + this.width;
  }

  get absoluteTop(): number {
    return this.relativeTop + (this.parent?.absoluteTop ?? 0);
  }

  set absoluteTop(value: number) {
    this.relativeTop = value - (this.parent?.absoluteTop ?? 0);
  }

  get absoluteBottom(): number {
    return this.absoluteTop + this.height;
  }

  get width(): number {
    return this.getProperty("Width")!.value;
  }

  set width(value: number) {
    this.getProperty("Width")!.value = value;
  }

  get height(): number {
    return this.getProperty("Height")!.value;
  }

  set height(value: number) {
    this.getProperty("Height")!.value = value;
  }

  get labelWidth(): number {
    return this.getProperty("CaptionLength")!.value;
  }

  set labelWidth(value: number) {
    this.getProperty("CaptionLength")!.value = value;
  }

  private _labelPosition: LabelPosition;
  get labelPosition(): LabelPosition {
    return this._labelPosition;
  }

  set labelPosition(value: number) {
    this._labelPosition = value;
  }

  get zIndex(): number {
    if (this.data.type === ComponentType.AsForm) {
      return screenLayer;
    }
    if (this.data.type === ComponentType.AsPanel) {
      return sectionLayer;
    }
    if (this.data.type === ComponentType.GroupBox) {
      return this.countParents() + sectionLayer;
    }
    return controlLayer;
  }

  constructor(args: {
    id: string,
    parent: Component | null,
    data: IComponentData,
    properties: EditorProperty[],
    designerRepresentation: ReactElement | null
  }) {
    this.id = args.id;
    this.data = args.data;
    this.properties = args.properties;
    this._labelPosition = parseLabelPosition(this.get("CaptionPosition"));
    this.parent = args.parent;
    this.designerRepresentation = args.designerRepresentation;
  }

  isPointInside(x: number, y: number) {
    return (
      x >= this.absoluteLeft &&
      x <= this.absoluteLeft + this.width &&
      y >= this.absoluteTop &&
      y <= this.absoluteTop + this.height
    );
  }

  countParents(): number {
    if (!this.parent) {
      return 0;
    }
    return this.parent.countParents() + 1;
  }

  getLabelStyle() {
    switch (this.labelPosition) {
      case LabelPosition.Left:
        return {
          left: `${this.absoluteLeft - this.labelWidth}px`,
          top: `${this.absoluteTop}px`,
          width: `${this.labelWidth}px`,
          height: `${this.height}px`,
        }
      case LabelPosition.Right:
        return {
          left: `${this.absoluteLeft + this.width}px`,
          top: `${this.absoluteTop}px`,
          width: `${this.labelWidth}px`,
          height: `${this.height}px`,
        }
      case LabelPosition.Bottom:
        return {
          left: `${this.absoluteLeft}px`,
          top: `${this.absoluteTop + this.height}px`,
          width: `${this.width}px`,
          height: `${this.labelWidth}px`,
        }
      case LabelPosition.Top:
        return {
          left: `${this.absoluteLeft}px`,
          top: `${this.absoluteTop + -20}px`,
          width: `${this.width}px`,
          height: `${this.labelWidth}px`,
        }
      case null:
      case undefined:
      case LabelPosition.None:
        return {display: 'none'}
    }
  }

  get(text: string): any {
    return this.getProperty(text)?.value;
  }

  getProperty(name: string): EditorProperty | undefined {
    return this.properties.find(p => p.name === name);
  }
}

export function controlToComponent(
  control: IApiControl,
  parent: Component | null,
): Component {
  const properties = control.properties.map(prop => new EditorProperty(prop));
  const componentType = parseComponentType(control.type);
  return new Component({
    id: control.id,
    parent: parent,
    data: {
      type: componentType,
      identifier: control.name,
    },
    properties: properties,
    designerRepresentation: getDesignerRepresentation(componentType, properties)
  });
}

export function sectionToComponent(
  sectionRootControl: IApiControl
):  ReactElement {
  const sectionComponents = toComponentRecursive(sectionRootControl, null, [])
  return <SectionItem components={sectionComponents}/>
}

export function toComponentRecursive(
  control: IApiControl,
  parent: Component | null,
  allComponents: Component[]
): Component[] {
  const newComponent = controlToComponent(control, parent);
  for (const childControl of control.children) {
    toComponentRecursive(childControl, newComponent, allComponents);
  }
  allComponents.push(newComponent);
  return allComponents;
}

function getDesignerRepresentation(type: ComponentType, properties: EditorProperty[]): ReactElement | null {
  switch (type) {
    case ComponentType.GroupBox:
      return (
        <div className={S.groupBoxContent}>
          <div
            className={S.groupBoxHeader}>{properties.find(x => x.name === "Text")?.value}
          </div>
        </div>
      );
    case ComponentType.AsForm:
    case ComponentType.AsPanel:
      return (
        <div className={S.panel}>
        </div>
      );
    case ComponentType.AsCheckBox:
      return (
        <div className={S.designSurfaceEditorContainer}>
          <div className={S.designSurfaceCheckbox}></div>
          <div>{properties.find(x => x.name === "Text")?.value}</div>
        </div>
      );
    case ComponentType.FormPanel:
      return null;
    default:
      return (
        <div className={S.designSurfaceEditorContainer}>
          <div className={S.designSurfaceInput}></div>
        </div>
      );
  }
}
