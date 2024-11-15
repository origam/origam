import React, { useEffect, useRef } from 'react';
import S from 'src/components/editors/screenSectionEditor/ComponentDesigner.module.scss';
import { observer } from "mobx-react-lite";
import {
  IComponent,
  ComponentType,
  ResizeHandle,
  ComponentDesignerState
} from "src/components/editors/screenSectionEditor/ComponentDesignerState.tsx";
import { action } from "mobx";

const Toolbox: React.FC<{
  designerState: ComponentDesignerState
}> = observer(({designerState}) => {

  const onDragStart = (type: ComponentType) => {
    action(() => {
      designerState.draggedComponentType = type;
    })();
  };

  return (
    <div className={S.toolbox}>
      <h3>Toolbox</h3>
      <div
        className={S.toolItem}
        draggable
        onDragStart={() => onDragStart('Label')}
      >
        Label
      </div>
      <div
        className={S.toolItem}
        draggable
        onDragStart={() => onDragStart('GroupBox')}
      >
        GroupBox
      </div>
    </div>
  );
});

const DesignSurface: React.FC<{
  designerState: ComponentDesignerState
}> = observer(({designerState}) => {
  const surfaceRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Delete' && designerState.selectedComponentId) {
        designerState.deleteComponent(designerState.selectedComponentId);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [designerState]);

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const onDrop = (e: React.DragEvent) => {
    e.preventDefault();
    if (!designerState.draggedComponentType || !surfaceRef.current) return;

    const surfaceRect = surfaceRef.current.getBoundingClientRect();
    const dropX = e.clientX - surfaceRect.left;
    const dropY = e.clientY - surfaceRect.top;

    designerState.createDraggedComponent(dropX, dropY);
  };

  const handleComponentMouseDown = (e: React.MouseEvent, component: IComponent) => {
    if (!surfaceRef.current) return;

    // Prevent dragging when clicking resize handles
    if ((e.target as HTMLElement).classList.contains(S.resizeHandle)) {
      return;
    }

    const surfaceRect = surfaceRef.current.getBoundingClientRect();
    const mouseX = e.clientX - surfaceRect.left;
    const mouseY = e.clientY - surfaceRect.top;

    designerState.startDragging(component, mouseX, mouseY);
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!surfaceRef.current) return;
    const surfaceRect = surfaceRef.current.getBoundingClientRect();
    const mouseX = e.clientX - surfaceRect.left;
    const mouseY = e.clientY - surfaceRect.top;

    if (designerState.isResizing) {
      designerState.updateResizing(mouseX, mouseY);
    } else if (designerState.isDragging) {
      designerState.updateDragging(mouseX, mouseY);
    }
  };

  const handleMouseUp = (e: React.MouseEvent) => {
    if (!surfaceRef.current) return;

    const mouseX = e.clientX - surfaceRef.current.getBoundingClientRect().left;
    const mouseY = e.clientY - surfaceRef.current.getBoundingClientRect().top;

    if (designerState.isDragging) {
      designerState.endDragging(mouseX, mouseY);
    }
    if (designerState.isResizing) {
      designerState.endResizing();
    }
  };

  const handleComponentClick = (e: React.MouseEvent, component: IComponent) => {
    e.stopPropagation();
    designerState.selectComponent(component.id);
  };

  const handleSurfaceClick = () => {
    designerState.selectComponent(null);
  };

  const handleResizeStart = (e: React.MouseEvent, component: IComponent, handle: ResizeHandle) => {
    e.stopPropagation();
    if (!surfaceRef.current) return;

    const surfaceRect = surfaceRef.current.getBoundingClientRect();
    const mouseX = e.clientX - surfaceRect.left;
    const mouseY = e.clientY - surfaceRect.top;

    designerState.startResizing(component, handle, mouseX, mouseY);
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
      {designerState.components.map((component) => (
        <div
          key={component.id}
          className={`${S.designComponent} ${S[component.type.toLowerCase()]} 
            ${designerState.draggingComponentId === component.id ? S.dragging : ''} 
            ${designerState.selectedComponentId === component.id ? S.selected : ''}`}
          style={{
            left: `${component.left}px`,
            top: `${component.top}px`,
            width: `${component.width}px`,
            height: `${component.height}px`,
            cursor: designerState.draggingComponentId === component.id ? 'move' : 'default',
            zIndex: component.type === 'GroupBox' ? 0 : 1
          }}
          onMouseDown={(e) => handleComponentMouseDown(e, component)}
          onClick={(e) => handleComponentClick(e, component)}
        >
          {component.type === 'Label' ? (
            <span>{component.text}</span>
          ) : (
            <div className={S.groupBoxContent}>
              <div className={S.groupBoxHeader}>{component.text}</div>
            </div>
          )}

          {designerState.selectedComponentId === component.id && [
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
      ))}
    </div>
  );
});

export const ComponentDesigner: React.FC<{
  designerState: ComponentDesignerState
}> = ({designerState}) => {
  return (
    <div className={S.componentDesigner}>
      <Toolbox designerState={designerState}/>
      <DesignSurface designerState={designerState}/>
    </div>
  );
};

export default ComponentDesigner;