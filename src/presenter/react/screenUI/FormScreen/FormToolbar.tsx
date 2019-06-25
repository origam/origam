import React from "react";
import ReactDOM from "react-dom";
import { observer, inject } from "mobx-react";
import { IFormScreen } from "../../../../Screens/FormScreen/types";
import { action } from "mobx";
import * as FormScreenActions from "../../../../Screens/FormScreen/FormScreenActions";

@inject(({ formScreen }) => {
  return { formScreen };
})
@observer
export class FormToolbar extends React.Component<{ formScreen?: IFormScreen }> {
  @action.bound handleSaveClick() {
    this.props.formScreen!.dispatch(FormScreenActions.saveSession());
  }

  render() {
    return ReactDOM.createPortal(
      <>
        <div className="action-item-big" onClick={this.handleSaveClick}>
          <i className="far fa-save icon" />
          <br />
          Save
        </div>
        {/*<div className="action-item-big">
          <i className="fas fa-redo icon" />
          <br />
          Reload
      </div>*/}
      </>,
      document.getElementById("form-actions-container")!
    );
  }
}
