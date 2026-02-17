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
import { T } from "utils/translation";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import S from "gui/Components/Dialogs/AboutDialog.module.scss";
import { IAboutInfo } from "model/entities/types/IAboutInfo";
import { AboutView } from "gui/Components/Dialogs/AboutView";
import { ModalDialog } from "gui/Components/Dialog/ModalDialog";

@observer
export class AboutDialog extends React.Component<{
  aboutInfo: IAboutInfo;
  onOkClick: () => void;
}> {
  render() {
    return (
      <ModalDialog
        title={T("About", "about_application")}
        titleButtons={null}
        onEscape={() => this.props.onOkClick()}
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
        <div className={CS.dialogContent + " " + S.contentArea}>
         <AboutView aboutInfo={this.props.aboutInfo}/>
        </div>
      </ModalDialog>
    );
  }
}
