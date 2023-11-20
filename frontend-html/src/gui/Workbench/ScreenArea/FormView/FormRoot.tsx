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

import S from "./FormRoot.module.scss";
import React from "react";
import { observer } from "mobx-react";
import { action, observable } from "mobx";
import cx from "classnames";
import { IDataView } from "model/entities/types/IDataView";

@observer
export class FormRoot extends React.Component<{
  className?: string;
  dataView: IDataView;
  style?: any
}> {

  @observable
  disableOverflow = false;

  componentDidMount() {
    window.addEventListener("click", this.handleWindowClick);
    this.adjustOverflow();
  }

  componentDidUpdate() {
    this.adjustOverflow();
  }

  // overflow has to be set to "unset" in the FormRoot div to avoid flickering in case there is a react-virtualized List
  // component directly under it. This is the case of readOnly, multiline TextEditor with a lot of text in it.
  private adjustOverflow() {
    const divFourLevelsDown = getFirstDivChild(this.elmFormRoot, 4);
    if (divFourLevelsDown && divFourLevelsDown.className.includes("ReactVirtualized__List")) {
      this.disableOverflow = true;
    }
  }

  componentWillUnmount() {
    window.removeEventListener("click", this.handleWindowClick);
  }

  @action.bound handleWindowClick(event: any) {
    if (this.elmFormRoot && this.elmFormRoot.contains(event.target) && event.target.tagName !== "DIV") {
      this.props.dataView!.formFocusManager.setLastFocused(event.target);
    }
  }

  elmFormRoot: HTMLDivElement | null = null;
  refFormRoot = (elm: HTMLDivElement | null) => (this.elmFormRoot = elm);

  render() {
    return (
      <div
        ref={this.refFormRoot}
        className={cx(this.props.className, S.formRoot) + " " +  (this.disableOverflow ? S.noOverflow : "")}
        style={this.props.style}
      >
        {this.props.children}
      </div>
    );
  }
}

function getFirstDivChild(element: HTMLDivElement | null, depth: number, currentDepth: number=0)
  : HTMLDivElement | null{

  if(!element || element.childNodes.length !== 1){
    return null;
  }
  const child = element.childNodes[0] ;
  if(child.nodeName !== "div"){
    return null;
  }
  if(currentDepth >= depth){
    return child as HTMLDivElement;
  }
  else{
    return getFirstDivChild(child as HTMLDivElement, depth, ++currentDepth);
  }
}
