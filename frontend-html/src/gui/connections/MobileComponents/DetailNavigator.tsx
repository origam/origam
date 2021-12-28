/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import React, { ReactNode } from "react";
import S from "./DetailNavigator.module.scss";
import { IDataView } from "model/entities/types/IDataView";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { observer } from "mobx-react";
import { Icon } from "@origam/components";
import { observable } from "mobx";


@observer
export class DetailNavigator extends React.Component<{
  rootNode: NavigationNode;
}> {

  @observable
  node: NavigationNode = this.props.rootNode;

  makeBreadcrumb(node: NavigationNode){
    return <a
      className={this.node.equals(node) ? "" : S.breadcrumb}
      key={node.name}
      onClick={() =>{
        if(!this.node.equals(node)){
          this.node = node
        }
      }}
    >
      {node.name}
    </a>
  }

  render() {
    return (
      <div className={S.root}>
        <div className={S.navigationBar}>
          {this.node.parentChain.length > 1 &&
            this.node.parentChain
            .flatMap((parent, i) => i === 0
              ? [this.makeBreadcrumb(parent)]
              : [<div className={S.pathSeparator}>/</div> ,
                this.makeBreadcrumb(parent)
              ]
            )
          }
        </div>
        {this.node.element}
        <div>
          { this.node.showDetailLinks &&
            this.node.children?.map(node =>
              <NavigationButton
                label={node.name}
                onClick={() => this.node = node}
              />)
          }
        </div>
      </div>
    );
  }
}

export const NavigationButton: React.FC<{
  label: string;
  onClick: () => void;
}> = observer((props) => {
  return (
    <div
      className={S.navigationButton}
      onClick={props.onClick}
    >
      <div>
        {props.label}
      </div>
      <Icon
        src={"./icons/noun-chevron-933251.svg"}
        className={S.navigationIcon}
      />
    </div>
  );
});


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

  get showDetailLinks(){
    return this.dataView.isFormViewActive();
  }

  get element() {
    return this.panelMap[this.dataView.modelInstanceId];
  }

  constructor(
    private dataView: IDataView,
    private panelMap: { [key: string]: ReactNode }) {
  }

  equals(other: NavigationNode){
    return this.dataView.id === other.dataView.id;
  }
}



