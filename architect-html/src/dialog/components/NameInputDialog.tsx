/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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
import S from '@dialogs/components/NameInputDialog.module.scss';
import { observer } from 'mobx-react-lite';
import React, { useId, useState } from 'react';

interface NameInputDialogProps {
  screenTitle: string;
  label: string;
  okLabel: string;
  cancelLabel: string;
  initialValue?: string;
  placeholder?: string;
  // Returns an already-translated error message, or null when the value is valid.
  validate?: (value: string) => string | null;
  onOkClick: (value: string) => void;
  onCancelClick: () => void;
}

export const NameInputDialog: React.FC<NameInputDialogProps> = observer(
  ({
    screenTitle,
    label,
    okLabel,
    cancelLabel,
    initialValue = '',
    placeholder,
    validate,
    onOkClick,
    onCancelClick,
  }) => {
    const inputId = useId();
    const [value, setValue] = useState(initialValue);
    const trimmed = value.trim();
    const error = validate ? validate(value) : null;
    const canSubmit = trimmed.length > 0 && !error;

    const submit = () => {
      if (canSubmit) {
        onOkClick(trimmed);
      }
    };

    const onKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
      if (event.key === 'Enter') {
        event.preventDefault();
        submit();
      } else if (event.key === 'Escape') {
        event.preventDefault();
        onCancelClick();
      }
    };

    return (
      <ModalWindow
        title={screenTitle}
        buttonsCenter={
          <>
            <button
              className={`${S.okButton} ${canSubmit ? 'isPrimary' : ''}`}
              disabled={!canSubmit}
              onClick={submit}
            >
              {okLabel}
            </button>
            <button onClick={onCancelClick}>{cancelLabel}</button>
          </>
        }
      >
        <div className={S.content}>
          <div className={S.field}>
            <label className={S.label} htmlFor={inputId}>
              {label}
            </label>
            <input
              autoFocus
              id={inputId}
              className={S.input}
              value={value}
              placeholder={placeholder}
              onChange={event => setValue(event.target.value)}
              onKeyDown={onKeyDown}
            />
            {error && trimmed.length > 0 && <div className={S.error}>{error}</div>}
          </div>
        </div>
      </ModalWindow>
    );
  },
);
