import React, { useContext, useRef } from 'react';
import './ComponentDesigner.css';
import { RootStoreContext } from "src/main.tsx";
import { observer } from "mobx-react-lite";
import { IComponent } from "src/components/screenSectionEditor/ComponentDesignerState.tsx";
import { action } from "mobx";

const Toolbox: React.FC = () => {
  const rootStore = useContext(RootStoreContext);
  const designerState = rootStore.componentDesignerState;

  const onDragStart = (type: IComponent['type']) => {
    action(() => {
      designerState.draggedComponentType = type;
    })();
  };

  return (
    <div className="toolbox">
      <h3>Toolbox</h3>
      <div
        className="tool-item"
        draggable
        onDragStart={() => onDragStart('Label')}
      >
        Label
      </div>
      <div
        className="tool-item"
        draggable
        onDragStart={() => onDragStart('GroupBox')}
      >
        GroupBox
      </div>
    </div>
  );
};

const DesignSurface: React.FC = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const designerState = rootStore.componentDesignerState;
  const surfaceRef = useRef<HTMLDivElement>(null);

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const onDrop = (e: React.DragEvent) => {
    e.preventDefault();
    if (!designerState.draggedComponentType || !surfaceRef.current) return;

    // Always use the surface ref for position calculation
    const surfaceRect = surfaceRef.current.getBoundingClientRect();
    const dropX = e.clientX - surfaceRect.left;
    const dropY = e.clientY - surfaceRect.top;

    designerState.createDraggedComponent(dropX, dropY);
  };

  const handleComponentMouseDown = (e: React.MouseEvent, component: IComponent) => {
    if (!surfaceRef.current) return;

    // Prevent dragging when clicking resize handles
    if ((e.target as HTMLElement).classList.contains('resize-handle')) {
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

  const handleResizeStart = (e: React.MouseEvent, component: IComponent, handle: 'top' | 'right' | 'bottom' | 'left') => {
    e.stopPropagation();
    designerState.startResizing(component, handle);
  };

  return (
    <div
      ref={surfaceRef}
      className="design-surface"
      onDragOver={handleDragOver}
      onDrop={onDrop}
      onMouseMove={handleMouseMove}
      onMouseUp={handleMouseUp}
      onMouseLeave={handleMouseUp}
    >
      {designerState.components.map((component) => (
        <div
          key={component.id}
          className={`design-component ${component.type.toLowerCase()} ${
            designerState.draggingComponentId === component.id ? 'dragging' : ''
          }`}
          style={{
            left: `${component.left}px`,
            top: `${component.top}px`,
            width: `${component.width}px`,
            height: `${component.height}px`,
            cursor: designerState.draggingComponentId === component.id ? 'move' : 'default',
            zIndex: component.type === 'GroupBox' ? 0 : 1
          }}
          onMouseDown={(e) => handleComponentMouseDown(e, component)}
        >
          {component.type === 'Label' ? (
            <span>{component.text}</span>
          ) : (
            <div className="group-box-content">
              <div className="group-box-header">{component.text}</div>
            </div>
          )}

          {['top', 'right', 'bottom', 'left'].map((handle) => (
            <div
              key={handle}
              className={`resize-handle ${handle}`}
              onMouseDown={(e) =>
                handleResizeStart(e, component, handle as 'top' | 'right' | 'bottom' | 'left')
              }
            />
          ))}
        </div>
      ))}
    </div>
  );
});

export const ComponentDesigner: React.FC = () => {
  return (
    <div className="component-designer">
      <Toolbox />
      <DesignSurface />
    </div>
  );
};

export default ComponentDesigner;