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

const TopLayout = ({
  topToolBar,
  editorArea,
  sideBar,
  sideBarWidth,
  onSideBarWidthChange,
  minSideBarWidth,
  maxSideBarWidth,
}: {
  topToolBar: ReactNode;
  editorArea: ReactNode;
  sideBar: ReactNode;
  sideBarWidth: number;
  onSideBarWidthChange: (width: number) => void;
  minSideBarWidth: number;
  maxSideBarWidth: number;
}) => {
  const mainAreaRef = useRef<HTMLDivElement | null>(null);
  const draggingRef = useRef(false);

  const handlePointerMove = useCallback(
    (pointerEvent: PointerEvent) => {
      if (!draggingRef.current || !mainAreaRef.current) return;
      const mainAreaRect = mainAreaRef.current.getBoundingClientRect();
      const proposedWidth = mainAreaRect.right - pointerEvent.clientX;
      const upperBound = Math.min(maxSideBarWidth, mainAreaRect.width - 200);
      const clampedWidth = Math.min(upperBound, Math.max(minSideBarWidth, proposedWidth));
      onSideBarWidthChange(clampedWidth);
    },
    [onSideBarWidthChange, minSideBarWidth, maxSideBarWidth],
  );

  const stopDragging = useCallback(() => {
    draggingRef.current = false;
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

  const onSplitterPointerDown = (pointerEvent: React.PointerEvent<HTMLDivElement>) => {
    pointerEvent.preventDefault();
    draggingRef.current = true;
    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';
  };

  return (
    <div className={S.root}>
      <div className={S.topToolbar}>{topToolBar}</div>
      <div className={S.mainArea} ref={mainAreaRef}>
        <div className={S.editorArea}>{editorArea}</div>
        <div
          className={S.splitter}
          role="separator"
          aria-orientation="vertical"
          onPointerDown={onSplitterPointerDown}
        />
        <div className={S.sideBar} style={{ width: sideBarWidth, minWidth: sideBarWidth }}>
          {sideBar}
        </div>
      </div>
    </div>
  );
};

export default TopLayout;
