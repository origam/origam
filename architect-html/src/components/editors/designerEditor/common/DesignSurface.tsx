import React, { useContext, useEffect, useRef } from "react";
import { observer } from "mobx-react-lite";
import { RootStoreContext } from "src/main.tsx";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import {
  Component,
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";
import S
  from "src/components/editors/designerEditor/common/DesignerSurface.module.scss";
import {
  IDesignerEditorState
} from "src/components/editors/designerEditor/common/IDesignerEditorState.tsx";
import {
  ResizeHandle
} from "src/components/editors/designerEditor/common/DesignSurfaceState.tsx";
import {
  ComponentType
} from "src/components/editors/designerEditor/common/ComponentType.tsx";
import { Item, Menu } from "react-contexify";

export const DesignSurface: React.FC<{
  designerState: IDesignerEditorState
}> = observer(({designerState}) => {
  const surfaceState = designerState.surface;
  const surfaceRef = useRef<HTMLDivElement>(null);
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Delete' && surfaceState.selectedComponent) {
        run({generator: designerState.delete([surfaceState.selectedComponent])});
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
      surfaceState.onClose();
    }
  }, [surfaceState]);

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const onDrop = (e: React.DragEvent) => {
    e.preventDefault();
    if (!surfaceState.draggedComponentData || !surfaceRef.current) {
      return;
    }
    const surfaceRect = surfaceRef.current.getBoundingClientRect();
    const dropX = e.clientX - surfaceRect.left;
    const dropY = e.clientY - surfaceRect.top;

    run({generator: designerState.create(dropX, dropY)});
  };

  const handleComponentMouseDown = (e: React.MouseEvent, component: Component) => {
    if (!surfaceRef.current) return;

    // Prevent dragging when clicking resize handles
    if ((e.target as HTMLElement).classList.contains(S.resizeHandle)) {
      return;
    }

    const surfaceRect = surfaceRef.current.getBoundingClientRect();
    const mouseX = e.clientX - surfaceRect.left;
    const mouseY = e.clientY - surfaceRect.top;

    surfaceState.startDragging(component, mouseX, mouseY);
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!surfaceRef.current) return;
    const surfaceRect = surfaceRef.current.getBoundingClientRect();
    const mouseX = e.clientX - surfaceRect.left;
    const mouseY = e.clientY - surfaceRect.top;

    if (surfaceState.isResizing) {
      surfaceState.updateResizing(mouseX, mouseY);
    } else if (surfaceState.isDragging) {
      surfaceState.updateDragging(mouseX, mouseY);
    }
  };

  const handleMouseUp = (e: React.MouseEvent) => {
    if (!surfaceRef.current) return;

    const rect = surfaceRef.current.getBoundingClientRect();
    const mouseX = e.clientX - rect.left + surfaceRef.current.scrollLeft;
    const mouseY = e.clientY - rect.top + surfaceRef.current.scrollTop;

    run({generator: surfaceState.onDesignerMouseUp(mouseX, mouseY)});
  };

  const handleComponentClick = (e: React.MouseEvent, component: Component) => {
    e.stopPropagation();
    surfaceState.selectComponent(component);
  };

  const handleSurfaceClick = () => {
    surfaceState.selectComponent(null);
  };

  const handleResizeStart = (e: React.MouseEvent, component: Component, handle: ResizeHandle) => {
    e.stopPropagation();
    if (!surfaceRef.current) return;

    const surfaceRect = surfaceRef.current.getBoundingClientRect();
    const mouseX = e.clientX - surfaceRect.left;
    const mouseY = e.clientY - surfaceRect.top;

    surfaceState.startResizing(component, handle, mouseX, mouseY);
  };

  return (
    <div
      ref={surfaceRef}
      className={S.designSurface}
      onDragOver={handleDragOver}
      onDrop={onDrop}
      onMouseMove={handleMouseMove}
      onMouseUp={handleMouseUp}
      onMouseLeave={handleMouseUp}
      onClick={handleSurfaceClick}
    >
      {surfaceState.components
        .filter(component => component.designerRepresentation)
        .map((component) => (
          <React.Fragment key={component.id}>
            <div
              className={S.componentLabel}
              style={{
                ...component.getLabelStyle(),
                zIndex: component.zIndex
              }
              }
            >
              {component.data.identifier}
            </div>
            <div
              className={`${S.designComponent} ${component.id} 
            ${surfaceState.draggingComponentId === component.id ? S.dragging : ''} 
            ${surfaceState.selectedComponent?.id === component.id ? S.selected : ''}`}
              style={{
                left: `${component.absoluteLeft || 15}px`,
                top: `${component.absoluteTop || 15}px`,
                width: `${component.width}px`,
                height: `${component.height}px`,
                cursor: surfaceState.draggingComponentId === component.id ? 'move' : 'default',
                zIndex: component.zIndex
              }}
              onMouseDown={(e) => handleComponentMouseDown(e, component)}
              onClick={(e) => handleComponentClick(e, component)}
            >
              {/* Wrapping renderDesignerRepresentation looks like something that could be in the SectionItem component.
            I tried moving it there, but I ran into performance problems and the
             result did not look very pretty.*/}
              {component.data.type === ComponentType.FormPanel
                ? <div
                  className={S.innerContainer}
                  style={{
                    width: `${component.width}px`,
                    height: `${component.height}px`,
                    cursor: surfaceState.draggingComponentId === component.id ? 'move' : 'default',
                    zIndex: component.zIndex
                  }}>
                  {component.designerRepresentation}
                </div>
                : component.designerRepresentation
              }
              {surfaceState.selectedComponent?.id === component.id && [
                'top',
                'right',
                'bottom',
                'left',
                'topLeft',
                'topRight',
                'bottomRight',
                'bottomLeft'
              ].map((handle) => (
                <div
                  key={component.id + handle}
                  className={`${S.resizeHandle} ${S[handle]}`}
                  onMouseDown={(e) =>
                    handleResizeStart(e, component, handle as ResizeHandle)
                  }
                />
              ))}
            </div>
          </React.Fragment>
        ))}
      <Menu id={"TAB_LABEL_MENU"} animation="fade">
        <Item onClick={({ props }) => props.onDelete()}>
          Delete
        </Item>
        <Item onClick={({ props }) => props.onAdd()}>
          Add New
        </Item>
      </Menu>
    </div>
  );
});