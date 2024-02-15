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
import { MobXProviderContext } from "mobx-react";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { ModalWindow } from "gui/Components/Dialogs/ModalWindow";
import { isPhoneLayoutActive } from "model/selectors/isPhoneLayoutActive";

export const ModalDialog: React.FC<{
  title: React.ReactNode;
  titleButtons: React.ReactNode;
  titleIsWorking?: boolean;
  buttonsLeft: React.ReactNode;
  buttonsRight: React.ReactNode;
  buttonsCenter: React.ReactNode;
  width?: number;
  height?: number;
  topPosiotionProc?: number;
  onKeyDown?: (event: any) => void;
  onWindowMove?: (top: number, left: number)=>void;
  mustRunFullScreenInMobile?: boolean;
}> = (props) => {
  const application = useContext(MobXProviderContext).application

  return (
    <ModalWindow
      title={props.title}
      titleButtons={props.titleButtons}
      titleIsWorking={props.titleIsWorking}
      buttonsLeft={props.buttonsLeft}
      buttonsRight={props.buttonsRight}
      buttonsCenter={props.buttonsCenter}
      width={props.width}
      height={props.height}
      fullScreen={isPhoneLayoutActive(application) || isMobileLayoutActive (application) && props.mustRunFullScreenInMobile}
      topPosiotionProc={props.topPosiotionProc}
      onKeyDown={props.onKeyDown}
      onWindowMove={props.onWindowMove}
    >
      {props.children}
    </ModalWindow>
  );
}
