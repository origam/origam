import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { IDataView } from "model/entities/types/IDataView";
import { ReactNode } from "react";
import { getDataViewById } from "model/selectors/DataView/getDataViewById";

export interface INavigationNode {
  readonly name: string;
  readonly children: INavigationNode[];
  readonly parent: INavigationNode | undefined;
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
    let bindings = getFormScreen(this.dataView)
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
    private panelMap: { [key: string]: ReactNode }) {
  }

  equals(other: INavigationNode) {
    return this.id === other.id;
  }
}