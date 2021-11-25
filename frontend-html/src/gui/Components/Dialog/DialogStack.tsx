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

import React, { useContext } from "react";
import ReactDOM from "react-dom";
import S from "./DialogStack.module.scss";
import { MobXProviderContext, observer } from "mobx-react";
import { getDialogStack } from "../../../model/selectors/DialogStack/getDialogStack";
import { IDialogInfo } from "model/entities/types/IDialogInfo";

export const ApplicationDialogStack: React.FC = observer(() => {
  const dialogStack = getDialogStack(
    useContext(MobXProviderContext).application
  );
  return <DialogStack stackedDialogs={dialogStack.stackedDialogs} close={dialogStack.closeDialog}/>;
});

@observer
export class DialogStack extends React.Component<{
  stackedDialogs: Array<IDialogInfo>;
  close: (componentKey: string) => void;
}> {

  onOverlayClick(dialogInfo: IDialogInfo) {
    if (dialogInfo.closeOnClickOutside) {
      this.props.close(dialogInfo.key);
    }
  }

  getStackedDialogs() {
    const result = [];
    for (let i = 0; i < this.props.stackedDialogs.length; i++) {
      if (i < this.props.stackedDialogs.length - 1) {
        result.push(
          React.cloneElement(this.props.stackedDialogs[i].component, {
            key: this.props.stackedDialogs[i].key
          })
        );
      } else {
        result.push(
          <div className={S.modalWindowOverlay} key={i}
               onClick={(event: any) => this.onOverlayClick(this.props.stackedDialogs[i])}/>,
          React.cloneElement(this.props.stackedDialogs[i].component, {
            key: this.props.stackedDialogs[i].key
          })
        );
      }
    }
    return result;
  }

  render() {
    return ReactDOM.createPortal(
      <>{this.getStackedDialogs()}</>,
      document.getElementById("modal-window-portal")!
    );
  }
}
