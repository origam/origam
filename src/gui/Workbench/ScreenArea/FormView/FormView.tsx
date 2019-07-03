import React from "react";
import S from "./FormView.module.css";

export class FormView extends React.Component<{}> {
  render() {
    return (
      <div className={S.formView}>
        {/*<Toolbar controller={this.formViewPresenter.toolbar} />*/}
        {this.props.children}
      </div>
    );
  }
}
