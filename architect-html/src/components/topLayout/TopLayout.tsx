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

import S from '@components/topLayout/TopLayout.module.scss';
import { ReactNode, useCallback, useEffect, useRef } from 'react';

type DragTarget = 'sidebar' | 'ai';

const MIN_EDITOR_WIDTH = 200;

const TopLayout = ({
  topToolBar,
  editorArea,
  sideBar,
  sideBarWidth,
  onSideBarWidthChange,
  minSideBarWidth,
  maxSideBarWidth,
  aiPanel,
  aiPanelVisible,
  aiPanelWidth,
  onAiPanelWidthChange,
  minAiPanelWidth,
  maxAiPanelWidth,
}: {
  topToolBar: ReactNode;
  editorArea: ReactNode;
  sideBar: ReactNode;
  sideBarWidth: number;
  onSideBarWidthChange: (width: number) => void;
  minSideBarWidth: number;
  maxSideBarWidth: number;
  aiPanel: ReactNode;
  aiPanelVisible: boolean;
  aiPanelWidth: number;
  onAiPanelWidthChange: (width: number) => void;
  minAiPanelWidth: number;
  maxAiPanelWidth: number;
}) => {
  const mainAreaRef = useRef<HTMLDivElement | null>(null);
  const draggingRef = useRef<{ target: DragTarget; startX: number; startWidth: number } | null>(
    null,
  );

  const handlePointerMove = useCallback(
    (pointerEvent: PointerEvent) => {
      const dragging = draggingRef.current;
      if (!dragging || !mainAreaRef.current) return;
      const mainAreaRect = mainAreaRef.current.getBoundingClientRect();
      const delta =
        dragging.target === 'ai'
          ? pointerEvent.clientX - dragging.startX
          : dragging.startX - pointerEvent.clientX;
      const proposedWidth = dragging.startWidth + delta;
      if (dragging.target === 'sidebar') {
        const otherWidth = aiPanelVisible ? aiPanelWidth : 0;
        const upperBound = Math.min(
          maxSideBarWidth,
          mainAreaRect.width - otherWidth - MIN_EDITOR_WIDTH,
        );
        onSideBarWidthChange(Math.min(upperBound, Math.max(minSideBarWidth, proposedWidth)));
      } else {
        const upperBound = Math.min(
          maxAiPanelWidth,
          mainAreaRect.width - sideBarWidth - MIN_EDITOR_WIDTH,
        );
        onAiPanelWidthChange(Math.min(upperBound, Math.max(minAiPanelWidth, proposedWidth)));
      }
    },
    [
      onSideBarWidthChange,
      onAiPanelWidthChange,
      minSideBarWidth,
      maxSideBarWidth,
      minAiPanelWidth,
      maxAiPanelWidth,
      aiPanelVisible,
      aiPanelWidth,
      sideBarWidth,
    ],
  );

  const stopDragging = useCallback(() => {
    draggingRef.current = null;
    document.body.style.cursor = '';
    document.body.style.userSelect = '';
  }, []);

  useEffect(() => {
    window.addEventListener('pointermove', handlePointerMove);
    window.addEventListener('pointerup', stopDragging);
    window.addEventListener('pointercancel', stopDragging);
    return () => {
      window.removeEventListener('pointermove', handlePointerMove);
      window.removeEventListener('pointerup', stopDragging);
      window.removeEventListener('pointercancel', stopDragging);
    };
  }, [handlePointerMove, stopDragging]);

  const startDragging =
    (target: DragTarget, startWidth: number) =>
    (pointerEvent: React.PointerEvent<HTMLDivElement>) => {
      pointerEvent.preventDefault();
      draggingRef.current = { target, startX: pointerEvent.clientX, startWidth };
      document.body.style.cursor = 'col-resize';
      document.body.style.userSelect = 'none';
    };

  return (
    <div className={S.root}>
      <div className={S.topToolbar}>{topToolBar}</div>
      <div className={S.mainArea} ref={mainAreaRef}>
        {aiPanelVisible && (
          <>
            <div className={S.sideBar} style={{ width: aiPanelWidth, minWidth: aiPanelWidth }}>
              {aiPanel}
            </div>
            <div
              className={S.splitter}
              role="separator"
              aria-orientation="vertical"
              onPointerDown={startDragging('ai', aiPanelWidth)}
            />
          </>
        )}
        <div className={S.editorArea}>{editorArea}</div>
        <div
          className={S.splitter}
          role="separator"
          aria-orientation="vertical"
          onPointerDown={startDragging('sidebar', sideBarWidth)}
        />
        <div className={S.sideBar} style={{ width: sideBarWidth, minWidth: sideBarWidth }}>
          {sideBar}
        </div>
      </div>
    </div>
  );
};

export default TopLayout;
