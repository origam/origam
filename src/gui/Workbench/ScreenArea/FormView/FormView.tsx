import React from "react";
import S from "./FormView.module.css";

export class FormView extends React.Component<{}> {
  render() {
    return (
      <div className={S.formView}>
        {/*<Toolbar ... />*/}
        {this.props.children}
      </div>
    );
  }
}
