import { observer } from "mobx-react";
import React from "react";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
import { T } from "utils/translation";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import S from "gui/Components/Dialogs/AboutDialog.module.scss";
import { IAboutInfo } from "model/entities/types/IAboutInfo";

@observer
export class AboutDialog extends React.Component<{
  aboutInfo: IAboutInfo;
  onOkClick: () => void;
}> {
  render() {
    return (
      <ModalWindow
        title={T("About", "about_application")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button tabIndex={0} onClick={() => this.props.onOkClick()}>
              {T("Ok", "button_ok")}
            </button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={CS.dialogContent}>
          <div>Server version: </div>
          <div className={S.version}>{this.props.aboutInfo.serverVersion}</div>
          <br/>
          <div>Client version: </div>
          <div className={S.version}>
            <div>{"Commit ID: "} 
              <a href={this.props.aboutInfo.clientCommitLink}>{this.props.aboutInfo.clientCommitId}</a>
              </div>
            {/* <div>Commit Link: {this.props.aboutInfo.clientCommitLink}</div> */}
            <div>Build Date: {this.props.aboutInfo.clientBuildDate}</div>
          </div>
          <br/>
          <div>Copyright 2020 Advantage Solutions, s. r. o.</div>
          <br/>
        </div>
      </ModalWindow>
    );
  }
}
