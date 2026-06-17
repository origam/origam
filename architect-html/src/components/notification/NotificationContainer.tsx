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

import S from '@components/notification/NotificationContainer.module.scss';
import { RootStoreContext } from '@/main';
import { IActionResultNotification } from '@components/notification/NotificationState';
import { observer } from 'mobx-react-lite';
import React, { useContext, useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { VscPass } from 'react-icons/vsc';

export const NotificationContainer: React.FC = observer(() => {
  const notificationState = useContext(RootStoreContext).notificationState;
  const portalRoot = document.getElementById('modal-window-portal');
  if (!portalRoot) return null;
  return createPortal(
    <div className={S.container} aria-live="polite">
      {notificationState.notifications.map(notification => (
        <NotificationCard key={notification.id} notification={notification} />
      ))}
    </div>,
    portalRoot,
  );
});

const NotificationCard: React.FC<{ notification: IActionResultNotification }> = observer(({ notification }) => {
  const notificationState = useContext(RootStoreContext).notificationState;
  const [hovered, setHovered] = useState(false);
  const showResultRef = useRef<HTMLButtonElement | null>(null);
  const previouslyFocusedRef = useRef<HTMLElement | null>(null);
  const actionTakenRef = useRef(false);

  const count = notification.results.length;
  const subtitle =
    count === 0
      ? 'No items reported by server'
      : `${count} item${count === 1 ? '' : 's'} added to the model`;
  const hasAction = !!notification.onShowResult && count > 0;

  useEffect(() => {
    if (hasAction && showResultRef.current) {
      previouslyFocusedRef.current = document.activeElement as HTMLElement | null;
      showResultRef.current.focus();
    }
    return () => {
      if (!actionTakenRef.current) {
        previouslyFocusedRef.current?.focus?.();
      }
    };
  }, [hasAction]);

  useEffect(() => {
    if (hovered) {
      notificationState.pause(notification.id);
    } else {
      notificationState.resume(notification.id);
    }
  }, [hovered, notification.id, notificationState]);

  const onKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Escape') {
      event.stopPropagation();
      notificationState.dismiss(notification.id);
    }
  };

  return (
    <div
      className={S.notification}
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
        <div className={S.title}>{notification.title}</div>
        <div className={S.subtitle}>{subtitle}</div>
      </div>
      <button
        className={S.closeBtn}
        onClick={() => notificationState.dismiss(notification.id)}
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
              notification.onShowResult?.();
              notificationState.dismiss(notification.id);
            }}
          >
            Show result
          </button>
        </div>
      )}
      <div className={S.progress}>
        <div
          className={`${S.progressBar} ${hovered ? S.paused : ''}`}
          style={{ animationDuration: `${notification.durationMs}ms` }}
        />
      </div>
    </div>
  );
});
