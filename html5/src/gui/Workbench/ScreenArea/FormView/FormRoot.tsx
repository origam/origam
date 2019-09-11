import S from "./FormRoot.module.css";
import React from "react";
import { observer } from "mobx-react";
import { action } from "mobx";

@observer
export class FormRoot extends React.Component<{}> {

  elmFormRoot: HTMLDivElement | null = null;
  refFormRoot = (elm: HTMLDivElement | null) => (this.elmFormRoot = elm);

  render() {
    return (
      <div ref={this.refFormRoot} className={S.formRoot} onClick={undefined}>
        {this.props.children}
      </div>
    );
  }
}
