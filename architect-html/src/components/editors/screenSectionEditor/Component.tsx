import {
  IComponentData,
  parseComponentType
} from "src/components/editors/screenSectionEditor/ComponentType.tsx";
import { observable } from "mobx";
import {
  IComponent,
  LabelPosition, parseLabelPosition
} from "src/components/editors/screenSectionEditor/ComponentDesignerState.tsx";
import { ApiControl } from "src/API/IArchitectApi.ts";
import {
  EditorProperty
} from "src/components/editors/gridEditor/GridEditorState.ts";

export class Component implements IComponent {
  id: string;
  data: IComponentData;
  @observable accessor properties: EditorProperty[];

  @observable private accessor _left: number;
  get left(): number { return this._left; }
  set left(value: number) { this._left = value; }

  @observable private accessor _top: number;
  get top(): number { return this._top; }
  set top(value: number) { this._top = value; }

  @observable private accessor _width: number;
  get width(): number { return this._width; }
  set width(value: number) { this._width = value; }

  @observable private accessor _height: number;
  get height(): number { return this._height; }
  set height(value: number) { this._height = value; }

  labelWidth: number;

  private _labelPosition: LabelPosition;
  get labelPosition(): LabelPosition { return this._labelPosition; }

  @observable accessor parentId: string | null = null;
  @observable accessor relativeLeft: number | undefined;
  @observable accessor relativeTop: number | undefined;

  constructor(args: {
    id: string,
    data: IComponentData,
    properties: EditorProperty[]
  }) {
    this.id = args.id;
    this.data = args.data;
    this.properties = args.properties;
    this._left = this.get("Left");
    this._top = this.get("Top");
    this._width = this.get("Width");
    this._height =  this.get("Height");
    this.labelWidth =  this.get("CaptionWidth");
    this._labelPosition =  parseLabelPosition(this.get("CaptionPosition"));
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
    return this.properties.find(p => p.name === text)?.value;
  }
}

export function toComponent(
  control: ApiControl,
  parent: IComponent |  null,
  allComponents: Component[]
): Component[] {

  const newComponent = new Component({
    id: control.id,
    data: {
      type: parseComponentType(control.type),
      name: control.name,
    },
    properties: control.properties.map(prop => new EditorProperty(prop)),
  });
  for (const childControl of control.children) {
    toComponent(childControl, newComponent, allComponents);
  }
  allComponents.push(newComponent);
  return allComponents;
}