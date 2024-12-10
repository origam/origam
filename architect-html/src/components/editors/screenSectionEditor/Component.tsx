import {
  ComponentType,
  IComponentData,
  parseComponentType
} from "src/components/editors/screenSectionEditor/ComponentType.tsx";
import { observable } from "mobx";
import {
  LabelPosition,
  parseLabelPosition
} from "src/components/editors/screenSectionEditor/ComponentDesignerState.tsx";
import { ApiControl } from "src/API/IArchitectApi.ts";

import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import {
  controlLayer,
  panelLayer
} from "src/components/editors/screenSectionEditor/Layers.ts";

export class Component {
  id: string;
  @observable.ref accessor parent: Component |  null;
  data: IComponentData;
  @observable accessor properties: EditorProperty[];

  get relativeLeft(): number { return this.getProperty("Left")!.value; }
  set relativeLeft(value: number) { this.getProperty("Left")!.value = value; }

  get relativeTop(): number { return this.getProperty("Top")!.value; }
  set relativeTop(value: number) { this.getProperty("Top")!.value = value; }

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
    this.relativeTop = value -  (this.parent?.absoluteTop ?? 0);
  }

  get absoluteBottom(): number {
    return this.absoluteTop + this.height;
  }

  get width(): number { return this.getProperty("Width")!.value; }
  set width(value: number) { this.getProperty("Width")!.value = value; }

  get height(): number { return this.getProperty("Height")!.value; }
  set height(value: number) { this.getProperty("Height")!.value = value; }

  get labelWidth(): number { return this.getProperty("CaptionLength")!.value; }
  set labelWidth(value: number) { this.getProperty("CaptionLength")!.value = value; }

  private _labelPosition: LabelPosition;
  get labelPosition(): LabelPosition { return this._labelPosition; }
  set labelPosition(value: number) { this._labelPosition = value; }

  get zIndex(): number {
    if(this.data.type === ComponentType.AsPanel){
      return panelLayer;
    }
    if(this.data.type === ComponentType.GroupBox){
      return this.countParents() + panelLayer;
    }
    return controlLayer;
  }

  constructor(args: {
    id: string,
    parent: Component |  null,
    data: IComponentData,
    properties: EditorProperty[]
  }) {
    this.id = args.id;
    this.data = args.data;
    this.properties = args.properties;
    this._labelPosition =  parseLabelPosition(this.get("CaptionPosition"));
    this.parent = args.parent;
  }

  isPointInside(x: number, y: number) {
    return (
      x >= this.absoluteLeft &&
      x <= this.absoluteLeft + this.width &&
      y >= this.absoluteTop &&
      y <= this.absoluteTop + this.height
    );
  }

  countParents(): number{
    if(!this.parent){
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

export function toComponent(
  control: ApiControl,
  parent: Component |  null,
): Component {
  return new Component({
    id: control.id,
    parent: parent,
    data: {
      type: parseComponentType(control.type),
      fieldName: control.name,
    },
    properties: control.properties.map(prop => new EditorProperty(prop)),
  });
}

export function toComponentRecursive(
  control: ApiControl,
  parent: Component |  null,
  allComponents: Component[]
): Component[] {
  const newComponent = toComponent(control, parent);
  for (const childControl of control.children) {
    toComponentRecursive(childControl, newComponent, allComponents);
  }
  allComponents.push(newComponent);
  return allComponents;
}