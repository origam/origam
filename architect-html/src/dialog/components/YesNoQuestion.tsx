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

import { observer } from "mobx-react-lite";
import React from "react";
import S from "src/dialog/ModalWindow.module.scss";
import { ModalWindow } from "src/dialog/ModalWindow.tsx";

interface YesNoQuestionProps {
  screenTitle: string;
  yesLabel: string;
  noLabel: string;
  message: string;
  onYesClick?: (event: React.MouseEvent<HTMLButtonElement>) => void;
  onNoClick?: (event: React.MouseEvent<HTMLButtonElement>) => void;
}

export const YesNoQuestion: React.FC<YesNoQuestionProps> = observer(({
  screenTitle,
  yesLabel,
  noLabel,
  message,
  onYesClick,
  onNoClick
}) => {
  return (
    <ModalWindow
      title={screenTitle}
      buttonsCenter={
        <>
          <button
            id="yesButton"
            tabIndex={0}
            autoFocus
            onClick={onYesClick}
          >
            {yesLabel}
          </button>
          <button
            id="noButton"
            tabIndex={0}
            onClick={onNoClick}
          >
            {noLabel}
          </button>
        </>
      }
    >
      <div className={S.dialogContent}>{message}</div>
    </ModalWindow>
  );
});