import {
  IComponentData, parseComponentType
} from "src/components/editors/screenSectionEditor/ComponentType.tsx";
import { observable } from "mobx";
import {
  IComponent
} from "src/components/editors/screenSectionEditor/ComponentDesignerState.tsx";
import { ApiControl } from "src/API/IArchitectApi.ts";

export class Component implements IComponent {
  id: string;
  data: IComponentData;
  @observable accessor left: number;
  @observable accessor top: number;
  @observable accessor width: number;
  @observable accessor height: number;
  labelWidth: number;
  @observable accessor parentId: string | null = null;
  @observable accessor relativeLeft: number | undefined;
  @observable accessor relativeTop: number | undefined;

  constructor(args: {
    id: string,
    data: IComponentData,
    left: number,
    top: number,
    width: number,
    height: number,
    labelWidth: number,
  }) {
    this.id = args.id;
    this.data = args.data;
    this.left = args.left;
    this.top = args.top;
    this.width = args.width;
    this.height = args.height;
    this.labelWidth = args.labelWidth;
  }
}

export function toComponent(
  control: ApiControl,
  parent: IComponent |  null,
  allComponents: Component[]
): Component[] {

  const componentArgs = {
    id: control.id,
    data: {
      type: parseComponentType(control.type),
      name: control.name,
    },
    left: 0,
    top: 0,
    width: 0,
    height: 0,
    parentId: parent,
    labelWidth: 0,
  }

  for (const item of control.valueItems) {
    switch (item.name){
      case "Left":
        componentArgs.left = parseFloat(item.value);
        break;
      case "Top":
        componentArgs.top = parseFloat(item.value);
        break;
      case "Width":
        componentArgs.width = parseFloat(item.value);
        break;
      case "Height":
        componentArgs.height = parseFloat(item.value);
        break;
      case "CaptionLength":
        componentArgs.labelWidth = parseFloat(item.value);
        break;
    }
  }
  const newComponent = new Component(componentArgs);
  for (const childControl of control.children) {
    toComponent(childControl, newComponent, allComponents);
  }
  allComponents.push(newComponent);
  return allComponents;
}