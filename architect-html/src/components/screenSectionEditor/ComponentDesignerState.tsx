import { observable } from "mobx";

export interface IComponent {
  id: string;
  type: ComponentType;
  left: number;
  top: number;
  width: number;
  height: number;
  text: string;
  parentId: string | null;
  relativeLeft?: number;
  relativeTop?: number;
}

export type ComponentType = 'Label' | 'GroupBox';

export class ComponentDesignerState {
  @observable accessor components: IComponent[] = [];
}

export class Component implements IComponent{
  id: string;
  type: ComponentType;
  @observable accessor  left: number;
  @observable accessor  top: number;
  @observable accessor  width: number;
  @observable accessor  height: number;
  @observable accessor  text: string;
  parentId: string | null;
  @observable accessor  relativeLeft: number | undefined;
  @observable accessor  relativeTop: number | undefined;

  constructor(args:{id: string, type: ComponentType, left: number, top: number, width: number, height: number, text: string}) {
    this.id = args.id;
    this.type = args.type;
    this.left = args.left;
    this.top = args.top;
    this.width = args.width;
    this.height = args.height;
    this.text = args.text;
  }
}