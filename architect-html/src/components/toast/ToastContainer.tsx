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

import S from '@components/toast/ToastContainer.module.scss';
import { RootStoreContext } from '@/main';
import { IActionResultToast } from '@components/toast/ToastState';
import { observer } from 'mobx-react-lite';
import React, { useContext, useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { VscPass } from 'react-icons/vsc';

export const ToastContainer: React.FC = observer(() => {
  const toastState = useContext(RootStoreContext).toastState;
  const portalRoot = document.getElementById('modal-window-portal');
  if (!portalRoot) return null;
  return createPortal(
    <div className={S.container} aria-live="polite">
      {toastState.toasts.map(t => (
        <ToastCard key={t.id} toast={t} />
      ))}
    </div>,
    portalRoot,
  );
});

const ToastCard: React.FC<{ toast: IActionResultToast }> = observer(({ toast }) => {
  const toastState = useContext(RootStoreContext).toastState;
  const [hovered, setHovered] = useState(false);
  const showResultRef = useRef<HTMLButtonElement | null>(null);
  // Remember the element that owned focus before this toast stole it, so we can
  // restore focus when the toast goes away without the user taking the action.
  const previouslyFocusedRef = useRef<HTMLElement | null>(null);
  const actionTakenRef = useRef(false);

  const count = toast.results.length;
  const subtitle =
    count === 0
      ? 'No items reported by server'
      : `${count} item${count === 1 ? '' : 's'} added to the model`;
  const hasAction = !!toast.onShowResult && count > 0;

  useEffect(() => {
    if (hasAction && showResultRef.current) {
      previouslyFocusedRef.current = document.activeElement as HTMLElement | null;
      showResultRef.current.focus();
    }
    return () => {
      // Restore focus on dismiss unless the user clicked Show result (in which
      // case the destination view will own focus instead).
      if (!actionTakenRef.current) {
        previouslyFocusedRef.current?.focus?.();
      }
    };
    // Toast identity is stable for its lifetime; we only want mount/unmount.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Only hover pauses the countdown — focusing the toast must NOT freeze the
  // timer, so the user can still see the progress bar shrinking and act on it.
  useEffect(() => {
    if (hovered) {
      toastState.pause(toast.id);
    } else {
      toastState.resume(toast.id);
    }
  }, [hovered, toast.id, toastState]);

  const onKeyDown = (e: React.KeyboardEvent<HTMLDivElement>) => {
    if (e.key === 'Escape') {
      e.stopPropagation();
      toastState.dismiss(toast.id);
    }
  };

  return (
    <div
      className={S.toast}
      role="status"
      tabIndex={-1}
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      onKeyDown={onKeyDown}
    >
      <div className={S.icon}>
        <VscPass />
      </div>
      <div className={S.body}>
        <div className={S.title}>{toast.title}</div>
        <div className={S.subtitle}>{subtitle}</div>
      </div>
      <button
        className={S.closeBtn}
        onClick={() => toastState.dismiss(toast.id)}
        aria-label="Dismiss"
      >
        ✕
      </button>
      {hasAction && (
        <div className={S.actions}>
          <button
            ref={showResultRef}
            className={S.actionBtn}
            onClick={() => {
              actionTakenRef.current = true;
              toast.onShowResult?.();
              toastState.dismiss(toast.id);
            }}
          >
            Show result
          </button>
        </div>
      )}
      <div className={S.progress}>
        <div
          className={`${S.progressBar} ${hovered ? S.paused : ''}`}
          style={{ animationDuration: `${toast.durationMs}ms` }}
        />
      </div>
    </div>
  );
});
