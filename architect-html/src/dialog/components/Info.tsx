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

import { ModalWindow } from '@dialogs/ModalWindow';
import S from '@dialogs/ModalWindow.module.scss';
import { observer } from 'mobx-react-lite';
import React from 'react';

type InfoProps = {
  screenTitle: string;
  message: string;
  okLabel?: string;
  onOkClick?: (event: React.MouseEvent<HTMLButtonElement>) => void;
};

export const Info = observer(({ screenTitle, message, okLabel = 'Ok', onOkClick }: InfoProps) => {
  return (
    <ModalWindow
      title={screenTitle}
      buttonsCenter={
        <button tabIndex={0} onClick={onOkClick} type="button">
          {okLabel}
        </button>
      }
    >
      <div className={S.dialogContent}>{message}</div>
    </ModalWindow>
  );
});
