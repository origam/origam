import React from "react";
import S from "./FormView.module.scss";
import { Provider, observer, inject } from "mobx-react";
import { IFormPanelView } from "model/entities/FormPanelView/types/IFormPanelView";
import cx from "classnames";

@inject(({ dataView }) => {
  return {
    formPanelView: dataView.formPanelView,
  };
})
@observer
export class FormView extends React.Component<{
  formPanelView?: IFormPanelView;
}> {
  render() {
    return (
      <Provider formPanelView={this.props.formPanelView}>
        <div className={cx(S.formView, "isFormView")}>
          <form className={S.formViewForm} onSubmit={(event: any) => event.preventDefault()}>
            {this.props.children}
          </form>
        </div>
      </Provider>
    );
  }
}
