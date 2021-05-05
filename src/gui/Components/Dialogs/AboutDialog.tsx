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
    const commitId = (window as any).ORIGAM_CLIENT_REVISION_HASH;
    const commitDate = (window as any).ORIGAM_CLIENT_REVISION_DATE;
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
        <div className={CS.dialogContent+" "+S.contentArea}>
          <div>Server version: </div>
          <div className={S.version}>{this.props.aboutInfo.serverVersion}</div>
          <br/>
          <div>Client version: </div>
          <div className={S.version}>
            <div>{"Commit ID: "} 
              <a href={"https://bitbucket.org/origamsource/origam-html5/commits/"+commitId}>{commitId}</a>
              </div>
            <div>Commit Date: {commitDate}</div>
          </div>
          <br/>
          <div>Copyright 2020 Advantage Solutions, s. r. o.</div>
          <br/>
        </div>
      </ModalWindow>
    );
  }
}
