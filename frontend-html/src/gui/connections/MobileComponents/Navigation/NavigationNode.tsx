import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { IDataView } from "model/entities/types/IDataView";
import { ReactNode } from "react";

export interface INavigationNode {
  readonly name: string;
  readonly children: INavigationNode[];
  parent: INavigationNode | undefined;
  readonly parentChain: INavigationNode[];
  readonly showDetailLinks: boolean;
  readonly element: ReactNode;
  readonly id: string;

  equals(other: INavigationNode): boolean;
}

export class NavigationNode implements INavigationNode {

  get name() {
    return this.dataView.name
      ? this.dataView.name
      : this.dataView.id;
  }

  get children() {
    return getFormScreen(this.dataView)
      .getBindingsByParentId(this.dataView.modelInstanceId)
      .map(binding => new NavigationNode(binding.childDataView, this.panelMap));
  }

  get parent(): INavigationNode | undefined {
    if(this.parentNode){
      return this.parentNode;
    }
    const bindings = getFormScreen(this.dataView)
      .getBindingsByChildId(this.dataView.modelInstanceId);
    if (bindings.length === 0) {
      return undefined;
    }
    if (bindings.length > 1) {
      throw new Error(`More than one master of detail ${this.name} was found`)
    }
    return new NavigationNode(bindings[0].parentDataView, this.panelMap);
  }

  get parentChain() {
    let parent = this.parent;
    const chain: INavigationNode[] = [this];
    while (parent) {
      chain.push(parent);
      parent = parent.parent;
    }
    return chain.reverse();
  }

  get showDetailLinks() {
    return this.dataView.isFormViewActive();
  }

  get element(): ReactNode {
    return this.panelMap[this.dataView.modelInstanceId];
  }

  get id() {
    return this.dataView.id
  }

  constructor(
    private dataView: IDataView,
    private panelMap: { [key: string]: ReactNode },
    private parentNode?: INavigationNode
  ) {
  }

  equals(other: INavigationNode) {
    return this.id === other.id;
  }
}

export class TabNavigationNode implements INavigationNode {
  readonly element: React.ReactNode;
  readonly id: string;
  readonly name: string;
  parent: INavigationNode | undefined;
  get parentChain(): INavigationNode[]{
    return [this.parent!, this];
  }
  readonly showDetailLinks: boolean = true;
  readonly panelMap: { [key: string]: ReactNode };

  get children(): INavigationNode[] {
    if(!this.dataView){
      return [];
    }
    return getFormScreen(this.dataView)
      .getBindingsByParentId(this.dataView.modelInstanceId)
      .map(binding => new NavigationNode(binding.childDataView, this.panelMap, this));
  }

  dataView: IDataView | undefined;

  constructor(args:{name: string, id: string, dataView: IDataView | undefined, ctx: any, element: ReactNode, panelMap: { [key: string]: ReactNode }}) {
    this.dataView = args.dataView
    this.id = args.id;
    this.name = args.name;
    this.element = args.element;
    this.panelMap = args.panelMap;
  }

  equals(other: INavigationNode): boolean {
    return this.id === other.id;
  }
}