import React, { useContext, useRef, useState } from 'react';
import './ComponentDesigner.css';
import { RootStoreContext } from "src/main.tsx";
import { observer } from "mobx-react-lite";
import {
  Component,
  ComponentType,
  IComponent
} from "src/components/screenSectionEditor/ComponentDesignerState.tsx";

interface ResizeHandle {
  component: IComponent | null;
  handle: 'top' | 'right' | 'bottom' | 'left' | null;
}

interface DragState {
  component: IComponent | null;
  startX: number;
  startY: number;
  originalLeft: number;
  originalTop: number;
}

const Toolbox: React.FC<{
  onDragStart: (type: IComponent['type']) => void;
}> = ({onDragStart}) => {
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

const DesignSurface: React.FC<{
  onDrop: (e: React.DragEvent) => void;
}> = observer(({onDrop}) => {
  const designerState = useContext(RootStoreContext).componentDesignerState;
  const [resizing, setResizing] = useState<ResizeHandle>({
    component: null,
    handle: null
  });
  const [dragging, setDragging] = useState<DragState>({
    component: null,
    startX: 0,
    startY: 0,
    originalLeft: 0,
    originalTop: 0
  });
  const surfaceRef = useRef<HTMLDivElement>(null);

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const isPointInsideComponent = (x: number, y: number, component: IComponent) => {
    return (
      x >= component.left &&
      x <= component.left + component.width &&
      y >= component.top &&
      y <= component.top + component.height
    );
  };

  const handleComponentMouseDown = (e: React.MouseEvent, component: IComponent) => {
    if (!surfaceRef.current) return;

    // Prevent dragging when clicking resize handles
    if ((e.target as HTMLElement).classList.contains('resize-handle')) return;

    const surfaceRect = surfaceRef.current.getBoundingClientRect();
    setDragging({
      component,
      startX: e.clientX - surfaceRect.left,
      startY: e.clientY - surfaceRect.top,
      originalLeft: component.left,
      originalTop: component.top
    });
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!surfaceRef.current) return;
    const surfaceRect = surfaceRef.current.getBoundingClientRect();
    const mouseX = e.clientX - surfaceRect.left;
    const mouseY = e.clientY - surfaceRect.top;

    // Handle resizing
    if (resizing.component && resizing.handle) {
      const component = resizing.component;
      switch (resizing.handle) {
        case 'right':
          component.width = Math.max(50, mouseX - component.left);
          break;
        case 'bottom':
          component.height = Math.max(50, mouseY - component.top);
          break;
        case 'left': {
          const newWidth = component.width + (component.left - mouseX);
          if (newWidth >= 50) {
            component.width = newWidth;
            component.left = mouseX;
          }
          break;
        }
        case 'top': {
          const newHeight = component.height + (component.top - mouseY);
          if (newHeight >= 50) {
            component.height = newHeight;
            component.top = mouseY;
          }
          break;
        }
      }
    }

    // Handle dragging
    else if (dragging.component) {
      const dx = mouseX - dragging.startX;
      const dy = mouseY - dragging.startY;
      dragging.component.left = dragging.originalLeft + dx;
      dragging.component.top = dragging.originalTop + dy;

      console.log('dragging.component.left:', dragging.component.left);
      console.log('dragging.component.top:', dragging.component.top);

      if (dragging.component.type === 'GroupBox') {
        for (const comp of designerState.components) {
          if (comp.parentId === dragging.component.id) {
            comp.left = comp.relativeLeft + dragging.component.left;
            comp.top = comp.relativeTop + dragging.component.top;
          }
        }
      }
    }
  };

  const handleMouseUp = (e: React.MouseEvent) => {
    if (dragging.component && dragging.component.type === 'Label') {
      const mouseX = e.clientX - surfaceRef.current!.getBoundingClientRect().left;
      const mouseY = e.clientY - surfaceRef.current!.getBoundingClientRect().top;

      const targetGroupBox = designerState.components.find(
        comp =>
          comp.type === 'GroupBox' &&
          isPointInsideComponent(mouseX, mouseY, comp)
      );

      if (targetGroupBox && dragging.component.parentId !== targetGroupBox.id) {
        dragging.component.parentId = targetGroupBox.id;
        dragging.component.relativeLeft = mouseX - targetGroupBox.left;
        dragging.component.relativeTop = mouseY - targetGroupBox.top;
      }
    }

    setResizing({component: null, handle: null});
    setDragging({
      component: null,
      startX: 0,
      startY: 0,
      originalLeft: 0,
      originalTop: 0
    });
  };

  const handleResizeStart = (e: React.MouseEvent, component: IComponent, handle: ResizeHandle['handle']) => {
    e.stopPropagation();
    setResizing({component, handle});
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
            dragging.component?.id === component.id ? 'dragging' : ''
          }`}
          style={{
            left: `${component.left}px`,
            top: `${component.top}px`,
            width: `${component.width}px`,
            height: `${component.height}px`,
            cursor: dragging.component?.id === component.id ? 'move' : 'default',
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
              onMouseDown={(e) => handleResizeStart(e, component, handle as ResizeHandle['handle'])}
            />
          ))}
        </div>
      ))}
    </div>
  );
});

export const ComponentDesigner: React.FC = () => {
  const designerState = useContext(RootStoreContext).componentDesignerState;
  const [draggedComponentType, setDraggedComponentType] = useState<ComponentType | null>(null);

  const handleToolboxDragStart = (type: IComponent['type']) => {
    setDraggedComponentType(type);
  };

  const handleDesignSurfaceDrop = (e: React.DragEvent) => {
    e.preventDefault();
    if (!draggedComponentType) return;

    const surfaceRect = (e.target as HTMLElement).getBoundingClientRect();
    const dropX = e.clientX - surfaceRect.left;
    const dropY = e.clientY - surfaceRect.top;

    const newComponent = new Component({
      id: `component-${Date.now()}`,
      type: draggedComponentType,
      left: dropX,
      top: dropY,
      width: draggedComponentType === 'Label' ? 100 : 200,
      height: draggedComponentType === 'Label' ? 30 : 150,
      text: draggedComponentType === 'Label' ? 'New Label' : 'New Group Box'
    });

    designerState.components.push(newComponent);
    setDraggedComponentType(null);
  };


  return (
    <div className="component-designer">
      <Toolbox onDragStart={handleToolboxDragStart}/>
      <DesignSurface
        onDrop={handleDesignSurfaceDrop}
      />
    </div>
  );
};

export default ComponentDesigner;