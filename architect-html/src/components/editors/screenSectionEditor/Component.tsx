import {
  IComponentData,
  parseComponentType
} from "src/components/editors/screenSectionEditor/ComponentType.tsx";
import { observable } from "mobx";
import {
  LabelPosition, parseLabelPosition
} from "src/components/editors/screenSectionEditor/ComponentDesignerState.tsx";
import { ApiControl } from "src/API/IArchitectApi.ts";

import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";

export class Component {
  id: string;
  parent: Component |  null;
  data: IComponentData;
  @observable accessor properties: EditorProperty[];

  get left(): number { return this.getProperty("Left")!.value; }
  set left(value: number) { this.getProperty("Left")!.value = value; }

  get top(): number { return this.getProperty("Top")!.value; }
  set top(value: number) { this.getProperty("Top")!.value = value; }

  get width(): number { return this.getProperty("Width")!.value; }
  set width(value: number) { this.getProperty("Width")!.value = value; }

  get height(): number { return this.getProperty("Height")!.value; }
  set height(value: number) { this.getProperty("Height")!.value = value; }

  get labelWidth(): number { return this.getProperty("CaptionLength")!.value; }
  set labelWidth(value: number) { this.getProperty("CaptionLength")!.value = value; }

  private _labelPosition: LabelPosition;
  get labelPosition(): LabelPosition { return this._labelPosition; }
  set labelPosition(value: number) { this._labelPosition = value; }

  @observable accessor parentId: string | null = null;
  @observable accessor relativeLeft: number | undefined;
  @observable accessor relativeTop: number | undefined;

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

  getLabelStyle() {
    switch (this.labelPosition) {
      case LabelPosition.Left:
        return {
          left: `${this.left - this.labelWidth}px`,
            top: `${this.top}px`,
            width: `${this.labelWidth}px`,
            height: `${this.height}px`,
        }
        case LabelPosition.Right:
          return {
            left: `${this.left + this.width}px`,
            top: `${this.top}px`,
            width: `${this.labelWidth}px`,
            height: `${this.height}px`,
          }
      case LabelPosition.Bottom:
        return {
          left: `${this.left}px`,
          top: `${this.top + this.height}px`,
          width: `${this.width}px`,
          height: `${this.labelWidth}px`,
        }
      case LabelPosition.Top:
        return {
          left: `${this.left}px`,
          top: `${this.top + -20}px`,
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