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

import { RootStoreContext } from '@/main';
import { ComponentType } from '@editors/designerEditor/common/ComponentType';
import { Component } from '@editors/designerEditor/common/designerComponents/Component';
import S from '@editors/designerEditor/common/DesignerSurface.module.scss';
import { ResizeHandle } from '@editors/designerEditor/common/DesignSurfaceState';
import { IDesignerEditorState } from '@editors/designerEditor/common/IDesignerEditorState';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { isTypingTarget } from '@/utils/keyShortcuts';
import { observer } from 'mobx-react-lite';
import React, { useContext, useEffect, useRef } from 'react';
import { Item, Menu } from '@origam/react-contexify';

export const DesignSurface: React.FC<{
  designerState: IDesignerEditorState;
}> = observer(({ designerState }) => {
  const surfaceState = designerState.surface;
  const surfaceRef = useRef<HTMLDivElement>(null);
  const ignoreNextSurfaceClick = useRef(false);
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (
        e.key === 'Delete' &&
        surfaceState.selectedComponent &&
        designerState.isActive &&
        !isTypingTarget(e)
      ) {
        run({ generator: designerState.delete([surfaceState.selectedComponent]) });
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [designerState, run, surfaceState]);

  useEffect(() => {
    return () => {
      surfaceState.onClose();
    };
  }, [surfaceState]);

  const toSurfacePoint = (e: { clientX: number; clientY: number }) => {
    const surface = surfaceRef.current!;
    const surfaceRect = surface.getBoundingClientRect();
    return {
      x: e.clientX - surfaceRect.left + surface.scrollLeft,
      y: e.clientY - surfaceRect.top + surface.scrollTop,
    };
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const onDrop = (e: React.DragEvent) => {
    e.preventDefault();
    if (!surfaceState.draggedComponentData || !surfaceRef.current) {
      return;
    }
    const dropPoint = toSurfacePoint(e);
    run({ generator: designerState.create(dropPoint.x, dropPoint.y) });
  };

  const trackPointerUntilRelease = () => {
    const handleMouseMove = (e: MouseEvent) => {
      if (!surfaceRef.current) return;
      const point = toSurfacePoint(e);
      if (surfaceState.isResizing) {
        surfaceState.updateResizing(point.x, point.y);
      } else if (surfaceState.isDragging) {
        surfaceState.updateDragging(point.x, point.y);
      }
    };
    const handleMouseUp = (e: MouseEvent) => {
      window.removeEventListener('mousemove', handleMouseMove);
      window.removeEventListener('mouseup', handleMouseUp);
      if (!surfaceRef.current) return;
      if (surfaceState.dragState.didDrag || surfaceState.resizeState.didResize) {
        ignoreNextSurfaceClick.current = true;
        setTimeout(() => (ignoreNextSurfaceClick.current = false));
      }
      const point = toSurfacePoint(e);
      run({ generator: surfaceState.onDesignerMouseUp(point.x, point.y) });
    };
    window.addEventListener('mousemove', handleMouseMove);
    window.addEventListener('mouseup', handleMouseUp);
  };

  const handleComponentMouseDown = (e: React.MouseEvent, component: Component) => {
    if (e.button !== 0 || !surfaceRef.current) return;

    // Prevent dragging when clicking resize handles
    if ((e.target as HTMLElement).classList.contains(S.resizeHandle)) {
      return;
    }

    const point = toSurfacePoint(e);
    surfaceState.startDragging(component, point.x, point.y);
    trackPointerUntilRelease();
  };

  const handleComponentClick = (event: React.MouseEvent, component: Component) => {
    event.stopPropagation();
    const componentToSelect = (event as any).clickedComponent
      ? (event as any).clickedComponent
      : component;
    surfaceState.selectComponent(componentToSelect);
  };

  const handleSurfaceClick = () => {
    if (ignoreNextSurfaceClick.current) {
      return;
    }
    surfaceState.selectComponent(null);
  };

  const handleResizeStart = (e: React.MouseEvent, component: Component, handle: ResizeHandle) => {
    e.stopPropagation();
    if (e.button !== 0 || !surfaceRef.current) return;

    const point = toSurfacePoint(e);
    surfaceState.startResizing(component, handle, point.x, point.y);
    trackPointerUntilRelease();
  };

  return (
    <div
      ref={surfaceRef}
      data-test-id="design-surface"
      className={S.designSurface}
      onDragOver={handleDragOver}
      onDrop={onDrop}
      onClick={handleSurfaceClick}
    >
      {surfaceState.components
        .filter(component => component.designerRepresentation)
        .map(component => (
          <React.Fragment key={component.id}>
            <div
              className={S.componentLabel}
              style={{
                ...component.getLabelStyle(),
                zIndex: component.zIndex,
              }}
            >
              {component.data.identifier}
            </div>
            <div
              data-test-id="design-component"
              className={`${S.designComponent} ${component.id}
            ${component.hasBorder ? '' : S.borderless}
            ${surfaceState.draggingComponentId === component.id ? S.dragging : ''}
            ${surfaceState.selectedComponent?.id === component.id ? S.selected : ''}`}
              style={{
                left: `${component.absoluteLeft}px`,
                top: `${component.absoluteTop}px`,
                width: `${component.width}px`,
                height: `${component.height}px`,
                cursor: surfaceState.draggingComponentId === component.id ? 'move' : 'default',
                zIndex: component.zIndex,
              }}
              onMouseDown={e => handleComponentMouseDown(e, component)}
              onClick={e => handleComponentClick(e, component)}
            >
              {/* Wrapping renderDesignerRepresentation looks like something that could be in the SectionItem component.
            I tried moving it there, but I ran into performance problems and the
             result did not look very pretty.*/}
              {component.data.type === ComponentType.FormPanel ? (
                <div
                  className={S.innerContainer}
                  style={{
                    width: `${component.width}px`,
                    height: `${component.height}px`,
                    cursor: surfaceState.draggingComponentId === component.id ? 'move' : 'default',
                    zIndex: component.zIndex,
                  }}
                >
                  {component.designerRepresentation}
                </div>
              ) : (
                component.designerRepresentation
              )}
              {surfaceState.selectedComponent?.id === component.id &&
                (component.canResizeHeight
                  ? [
                      'top',
                      'right',
                      'bottom',
                      'left',
                      'topLeft',
                      'topRight',
                      'bottomRight',
                      'bottomLeft',
                    ]
                  : ['right', 'left']
                ).map(handle => (
                  <div
                    key={component.id + handle}
                    className={`${S.resizeHandle} ${S[handle]}`}
                    onMouseDown={e => handleResizeStart(e, component, handle as ResizeHandle)}
                  />
                ))}
            </div>
          </React.Fragment>
        ))}
      <Menu id={'TAB_LABEL_MENU'} animation="fade">
        <Item
          disabled={({ props }) => props.deleteDisabled}
          onClick={({ props }) => props.onDelete()}
        >
          Delete
        </Item>
        <Item onClick={({ props }) => props.onAdd()}>Add New</Item>
      </Menu>
    </div>
  );
});
