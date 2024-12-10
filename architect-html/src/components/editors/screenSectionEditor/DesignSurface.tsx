import React, { useContext, useEffect, useRef } from "react";
import {
  ComponentDesignerState,
  ResizeHandle
} from "src/components/editors/screenSectionEditor/ComponentDesignerState.tsx";
import { observer } from "mobx-react-lite";
import { RootStoreContext } from "src/main.tsx";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import {
  Component
} from "src/components/editors/screenSectionEditor/Component.tsx";
import S
  from "src/components/editors/screenSectionEditor/DesignerSurface.module.scss";
import {
  ComponentType
} from "src/components/editors/screenSectionEditor/ComponentType.tsx";

export const DesignSurface: React.FC<{
  designerState: ComponentDesignerState
}> = observer(({designerState}) => {
  const surfaceState = designerState.surface;
  const surfaceRef = useRef<HTMLDivElement>(null);
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Delete' && surfaceState.selectedComponent) {
        run({generator: designerState.deleteComponent(surfaceState.selectedComponent)});
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

    run({generator: surfaceState.createDraggedComponent(dropX, dropY)});
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

    const mouseX = e.clientX - surfaceRef.current.getBoundingClientRect().left;
    const mouseY = e.clientY - surfaceRef.current.getBoundingClientRect().top;

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

  function getDesignSurfaceRepresentation(component: Component) {
    switch (component.data.type) {
      case ComponentType.GroupBox:
        return (
          <div className={S.groupBoxContent}>
            <div
              className={S.groupBoxHeader}>{component.getProperty("Text")?.value}</div>
          </div>
        );
      case ComponentType.AsPanel:
        return (
          <div className={S.panel}>
          </div>
        );
      case ComponentType.AsCheckBox:
        return (
          <div className={S.designSurfaceEditorContainer}>
            <div className={S.designSurfaceCheckbox}></div>
            <div>{component.getProperty("Text")?.value}</div>
          </div>
        );
      default:
        return (
          <div className={S.designSurfaceEditorContainer}>
            <div className={S.designSurfaceInput}></div>
          </div>
        );
    }
  }

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
      {surfaceState.components.map((component) => (
        <>
          <div
            key={component.id + "_label"}
            className={S.componentLabel}
            style={{
              ...component.getLabelStyle(),
              zIndex: component.zIndex
            }
            }
          >
            {component.data.fieldName}
          </div>
          <div
            key={component.id + "_component"}
            className={`${S.designComponent} 
            ${surfaceState.draggingComponentId === component.id ? S.dragging : ''} 
            ${surfaceState.selectedComponent?.id === component.id ? S.selected : ''}`}
            style={{
              left: `${component.absoluteLeft}px`,
              top: `${component.absoluteTop}px`,
              width: `${component.width}px`,
              height: `${component.height}px`,
              cursor: surfaceState.draggingComponentId === component.id ? 'move' : 'default',
              zIndex: component.zIndex
            }}
            onMouseDown={(e) => handleComponentMouseDown(e, component)}
            onClick={(e) => handleComponentClick(e, component)}
          >

            {getDesignSurfaceRepresentation(component)}

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
                key={handle}
                className={`${S.resizeHandle} ${S[handle]}`}
                onMouseDown={(e) =>
                  handleResizeStart(e, component, handle as ResizeHandle)
                }
              />
            ))}
          </div>
        </>
      ))}
    </div>
  );
});