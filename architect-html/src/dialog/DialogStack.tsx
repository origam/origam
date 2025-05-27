/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

import React, { useCallback, useContext } from "react";
import { createPortal } from 'react-dom';
import S from "./DialogStack.module.scss";
import { observer } from "mobx-react-lite";
import { RootStoreContext } from "src/main.tsx";
import { IDialogInfo } from "src/dialog/types.ts";

export const ApplicationDialogStack: React.FC = observer(() => {
  const dialogStack = useContext(RootStoreContext).dialogStack;
  return <DialogStack stackedDialogs={dialogStack.stackedDialogs} close={dialogStack.closeDialog}/>;
});

interface DialogStackProps {
  stackedDialogs: Array<IDialogInfo>;
  close: (componentKey: string) => void;
}

export const DialogStack: React.FC<DialogStackProps> = observer(({ stackedDialogs, close }) => {
  const onOverlayClick = useCallback((dialogInfo: IDialogInfo) => {
    if (dialogInfo.closeOnClickOutside) {
      close(dialogInfo.key);
    }
  }, [close]);

  const getStackedDialogs = useCallback(() => {
    const result = [];

    for (let i = 0; i < stackedDialogs.length; i++) {
      if (i < stackedDialogs.length - 1) {
        // For all dialogs except the last one
        result.push(
          React.cloneElement(stackedDialogs[i].component, {
            key: stackedDialogs[i].key
          })
        );
      } else {
        // For the last dialog, add overlay and the dialog
        result.push(
          <div
            className={S.modalWindowOverlay}
            key={`overlay-${i}`}
            onClick={() => onOverlayClick(stackedDialogs[i])}
          />,
          React.cloneElement(stackedDialogs[i].component, {
            key: stackedDialogs[i].key
          })
        );
      }
    }

    return result;
  }, [stackedDialogs, onOverlayClick]);

  return createPortal(
    <>{getStackedDialogs()}</>,
    document.getElementById("modal-window-portal")!
  );
});
