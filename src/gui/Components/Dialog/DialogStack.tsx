import React, {useContext} from "react";
import ReactDOM from "react-dom";
import S from "./Dialog.module.scss";
import {MobXProviderContext, observer} from "mobx-react";
import {getDialogStack} from "../../../model/selectors/DialogStack/getDialogStack";
import { IDialogInfo } from "model/entities/types/IDialogInfo";

export const ApplicationDialogStack: React.FC = observer(() => {
  const dialogStack = getDialogStack(
    useContext(MobXProviderContext).application
  );
  return <DialogStack stackedDialogs={dialogStack.stackedDialogs} close={dialogStack.closeDialog} />;
});

@observer
export class DialogStack extends React.Component<{
  stackedDialogs: Array<IDialogInfo>;
  close: (componentKey: string)=> void;
}> {

  onOverlayClick(dialogInfo: IDialogInfo){
    if(dialogInfo.closeOnClickOutside){
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
            onClick={(event:any) => this.onOverlayClick(this.props.stackedDialogs[i])} />,
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
