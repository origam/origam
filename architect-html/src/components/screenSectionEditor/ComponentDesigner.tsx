// ComponentDesigner.tsx
import React, { useState, useRef } from 'react';
import './ComponentDesigner.css';

interface Component {
  id: string;
  type: 'Label' | 'GroupBox';
  left: number;
  top: number;
  width: number;
  height: number;
  text: string;
  parentId: string | null;
  relativeLeft?: number;
  relativeTop?: number;
}

interface ResizeHandle {
  component: Component;
  handle: 'top' | 'right' | 'bottom' | 'left' | null;
}

interface DragState {
  component: Component | null;
  startX: number;
  startY: number;
  originalLeft: number;
  originalTop: number;
}

const Toolbox: React.FC<{
  onDragStart: (type: Component['type']) => void;
}> = ({ onDragStart }) => {
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
  components: Component[];
  onDrop: (e: React.DragEvent) => void;
  onComponentUpdate: (updatedComponent: Component) => void;
  onComponentsUpdate: (updatedComponents: Component[]) => void;
}> = ({ components, onDrop, onComponentUpdate, onComponentsUpdate }) => {
  const [resizing, setResizing] = useState<ResizeHandle>({ component: null, handle: null });
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

  const isPointInsideComponent = (x: number, y: number, component: Component) => {
    return (
      x >= component.left &&
      x <= component.left + component.width &&
      y >= component.top &&
      y <= component.top + component.height
    );
  };

  const updateChildrenPositions2 = (
    parentComponent: Component,
    allComponents: Component[]
  ) => {
    return allComponents.map(comp => {

      if (comp.parentId === parentComponent.id) {
        return {
          ...comp,
          left: comp.relativeLeft + parentComponent.left,
          top: comp.relativeTop + parentComponent.top
        };
      }
      return comp;
    });
  };

  const handleComponentMouseDown = (e: React.MouseEvent, component: Component) => {
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
      const updatedComponent = { ...resizing.component };

      switch (resizing.handle) {
        case 'right':
          updatedComponent.width = Math.max(50, mouseX - updatedComponent.left);
          break;
        case 'bottom':
          updatedComponent.height = Math.max(50, mouseY - updatedComponent.top);
          break;
        case 'left':
          const newWidth = updatedComponent.width + (updatedComponent.left - mouseX);
          if (newWidth >= 50) {
            updatedComponent.width = newWidth;
            updatedComponent.left = mouseX;
          }
          break;
        case 'top':
          const newHeight = updatedComponent.height + (updatedComponent.top - mouseY);
          if (newHeight >= 50) {
            updatedComponent.height = newHeight;
            updatedComponent.top = mouseY;
          }
          break;
      }

      onComponentUpdate(updatedComponent);
    }

    // Handle dragging
    else if (dragging.component) {
      const dx = mouseX - dragging.startX;
      const dy = mouseY - dragging.startY;

      const updatedComponent = {
        ...dragging.component,
        left: dragging.originalLeft + dx,
        top:  dragging.originalTop + dy
      };


      let updatedComponents = components.map(comp =>
        comp.id === updatedComponent.id ? updatedComponent : comp
      );

      // Update positions of child components if the dragged component is a GroupBox
      if (updatedComponent.type === 'GroupBox') {
        updatedComponents = updateChildrenPositions2(
          updatedComponent,
          updatedComponents
        );
      }

      onComponentsUpdate(updatedComponents);
    }
  };

  const handleMouseUp = (e: React.MouseEvent) => {
    if (dragging.component && dragging.component.type === 'Label') {
      const mouseX = e.clientX - surfaceRef.current!.getBoundingClientRect().left;
      const mouseY = e.clientY - surfaceRef.current!.getBoundingClientRect().top;

      // Find if the label is dropped inside any GroupBox
      const targetGroupBox = components.find(
        comp =>
          comp.type === 'GroupBox' &&
          isPointInsideComponent(mouseX, mouseY, comp)
      );

      if (targetGroupBox && dragging.component.parentId !== targetGroupBox.id) {
        const updatedLabel = {
          ...dragging.component,
          parentId: targetGroupBox.id,
          relativeLeft: mouseX - targetGroupBox.left,
          relativeTop: mouseY - targetGroupBox.top
        };
        onComponentUpdate(updatedLabel);
      }
    }

    setResizing({ component: null, handle: null });
    setDragging({
      component: null,
      startX: 0,
      startY: 0,
      originalLeft: 0,
      originalTop: 0
    });
  };

  const handleResizeStart = (e: React.MouseEvent, component: Component, handle: ResizeHandle['handle']) => {
    e.stopPropagation();
    setResizing({ component, handle });
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
      {components.map((component) => (
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
};

export const ComponentDesigner: React.FC = () => {
  const [components, setComponents] = useState<Component[]>([]);
  const [draggedComponentType, setDraggedComponentType] = useState<Component['type'] | null>(null);

  const handleToolboxDragStart = (type: Component['type']) => {
    setDraggedComponentType(type);
  };

  const handleDesignSurfaceDrop = (e: React.DragEvent) => {
    e.preventDefault();
    if (!draggedComponentType) return;

    const surfaceRect = (e.target as HTMLElement).getBoundingClientRect();
    const dropX = e.clientX - surfaceRect.left;
    const dropY = e.clientY - surfaceRect.top;

    const newComponent: Component = {
      id: `component-${Date.now()}`,
      type: draggedComponentType,
      left: dropX,
      top: dropY,
      width: draggedComponentType === 'Label' ? 100 : 200,
      height: draggedComponentType === 'Label' ? 30 : 150,
      text: draggedComponentType === 'Label' ? 'New Label' : 'New Group Box',
      parentId: null
    };

    setComponents([...components, newComponent]);
    setDraggedComponentType(null);
  };

  const handleComponentUpdate = (updatedComponent: Component) => {
    setComponents(
      components.map((comp) =>
        comp.id === updatedComponent.id ? updatedComponent : comp
      )
    );
  };

  const handleComponentsUpdate = (updatedComponents: Component[]) => {
    setComponents(updatedComponents);
  };

  return (
    <div className="component-designer">
      <Toolbox onDragStart={handleToolboxDragStart} />
      <DesignSurface
        components={components}
        onDrop={handleDesignSurfaceDrop}
        onComponentUpdate={handleComponentUpdate}
        onComponentsUpdate={handleComponentsUpdate}
      />
    </div>
  );
};

export default ComponentDesigner;