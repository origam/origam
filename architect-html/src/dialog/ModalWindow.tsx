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

import S from '@dialogs/ModalWindow.module.scss';
import { requestFocus } from '@utils/focus.ts';
import { observer } from 'mobx-react-lite';
import React, { useCallback, useEffect, useRef } from 'react';

interface ModalWindowProps {
  title: React.ReactNode;
  titleButtons?: React.ReactNode;
  buttonsLeft?: React.ReactNode;
  buttonsRight?: React.ReactNode;
  buttonsCenter?: React.ReactNode;
  width?: number;
  height?: number;
  children?: React.ReactNode;
}

export const ModalWindow = observer((props: ModalWindowProps) => {
  const footerRef = useRef<HTMLDivElement | null>(null);
  const focusHookRef = useRef(false);

  const footerFocusHookEnsureOn = useCallback(() => {
    const footerElement = footerRef.current;
    if (footerElement && !focusHookRef.current) {
      footerElement.addEventListener(
        'keydown',
        (evt: KeyboardEvent) => {
          if (evt.key === 'Tab') {
            evt.preventDefault();
            const target = evt.target as HTMLElement;

            if (evt.shiftKey) {
              if (target.previousSibling) {
                requestFocus(target.previousSibling);
              } else {
                requestFocus(footerElement.lastChild);
              }
            } else {
              if (target.nextSibling) {
                requestFocus(target.nextSibling);
              } else {
                requestFocus(footerElement.firstChild);
              }
            }
          }
        },
        true,
      );
      focusHookRef.current = true;
    }
  }, []);

  useEffect(() => {
    footerFocusHookEnsureOn();
  }, [footerFocusHookEnsureOn]);

  const renderFooter = () => {
    if (props.buttonsLeft || props.buttonsCenter || props.buttonsRight) {
      return (
        <div ref={footerRef} className={S.footer}>
          {props.buttonsLeft}
          {props.buttonsCenter ? props.buttonsCenter : <div className={S.pusher} />}
          {props.buttonsRight}
        </div>
      );
    }
    return null;
  };

  return (
    <div
      className={S.modalWindow}
      style={{
        minWidth: props.width,
        minHeight: props.height,
      }}
      tabIndex={0}
    >
      {props.title && (
        <div className={S.title}>
          <div className={S.label}>
            <div className={S.labelText}>{props.title}</div>
          </div>
          <div className={S.buttons}>{props.titleButtons}</div>
        </div>
      )}
      <div className={S.body}>{props.children}</div>
      {renderFooter()}
    </div>
  );
});
