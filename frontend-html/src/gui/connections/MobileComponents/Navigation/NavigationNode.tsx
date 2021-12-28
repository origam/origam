import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { IDataView } from "model/entities/types/IDataView";
import { ReactNode } from "react";

export class NavigationNode {

  get name() {
    return this.dataView.name;
  }

  get children() {
    return getFormScreen(this.dataView)
      .getBindingsByParentId(this.dataView.modelInstanceId)
      .map(binding => new NavigationNode(binding.childDataView, this.panelMap));
  }

  get parent(): NavigationNode | undefined {
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
    const chain: NavigationNode[] = [this];
    while (parent) {
      chain.push(parent);
      parent = parent.parent;
    }
    return chain.reverse();
  }

  get showDetailLinks() {
    return this.dataView.isFormViewActive();
  }

  get element() {
    return this.panelMap[this.dataView.modelInstanceId];
  }

  constructor(
    private dataView: IDataView,
    private panelMap: { [key: string]: ReactNode }) {
  }

  equals(other: NavigationNode) {
    return this.dataView.id === other.dataView.id;
  }
}