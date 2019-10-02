import S from "./FormRoot.module.css";
import React from "react";
import { observer } from "mobx-react";
import { action } from "mobx";

@observer
export class FormRoot extends React.Component<{style?:any}> {
  componentDidMount() {
    window.addEventListener("click", this.handleWindowClick);
  }

  componentWillUnmount() {
    window.removeEventListener("click", this.handleWindowClick);
  }

  @action.bound handleWindowClick(event: any) {
    if (this.elmFormRoot && !this.elmFormRoot.contains(event.target)) {
    }
  }

  elmFormRoot: HTMLDivElement | null = null;
  refFormRoot = (elm: HTMLDivElement | null) => (this.elmFormRoot = elm);

  render() {
    return (
      <div ref={this.refFormRoot} className={S.formRoot} onClick={undefined} style={this.props.style}>
        {this.props.children}
      </div>
    );
  }
}
