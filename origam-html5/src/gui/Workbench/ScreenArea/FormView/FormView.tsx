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

import React from "react";
import S from "./FormView.module.scss";
import {inject, observer, Provider} from "mobx-react";
import {IFormPanelView} from "model/entities/FormPanelView/types/IFormPanelView";
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
