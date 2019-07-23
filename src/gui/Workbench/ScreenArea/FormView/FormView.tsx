import React from "react";
import S from "./FormView.module.css";

export class FormView extends React.Component<{}> {
  render() {
    return (
      <div className={S.formView}>
        <form onSubmit={(event: any) => event.preventDefault()}>
          {/*<Toolbar ... />*/}
          {this.props.children}
        </form>
      </div>
    );
  }
}
