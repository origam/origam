import React, { useContext, useEffect, useRef } from 'react';
import S
  from 'src/components/editors/screenSectionEditor/ComponentDesigner.module.scss';
import { observer } from "mobx-react-lite";
import {
  IComponent,
  ResizeHandle,
  ComponentDesignerState, DesignSurfaceState, toComponentType
} from "src/components/editors/screenSectionEditor/ComponentDesignerState.tsx";
import { action } from "mobx";
import { RootStoreContext } from "src/main.tsx";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import { IEditorField } from "src/API/IArchitectApi.ts";

const Toolbox: React.FC<{
  designerState: ComponentDesignerState
}> = observer((props) => {
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const surfaceState = props.designerState.surface;
  const toolboxState = props.designerState.toolbox;

  const onDragStart = (field: IEditorField) => {
    action(() => {
      surfaceState.draggedComponentData = {
        name: field.name,
        type: toComponentType(field.type)
      };
    })();
  };

  function getToolboxComponent(field: IEditorField) {
    return (
      <div
        key={field.name}
        draggable
        onDragStart={() => onDragStart(field)}
        className={S.toolboxField}
      >
        <div className={S.toolboxFieldIcon}>

        </div>
        <div>
          {field.name}
        </div>
      </div>
    );
  }

  return (
    <div className={S.toolbox}>
      <div className={S.inputs}>
        <div className={S.inputContainer}>
          <div>
            Data Source
          </div>
          <select
            value={toolboxState.selectedDataSourceId ?? ""}
            onChange={(e) => run({generator: toolboxState.selectedDataSourceIdChanged(e.target.value)})}
          >
            {toolboxState.dataSources.map(x =>
              <option
                key={x.schemaItemId + x.name}
                value={x.schemaItemId}>{x.name}
              </option>)
            }
          </select>
        </div>
        <div className={S.inputContainer}>
          <div>
            Name
          </div>
          <input
            type="text"
            value={toolboxState.name}
            onChange={(e) => run({generator: toolboxState.nameChanged(e.target.value)})}
          />
        </div>
        <div className={S.inputContainer}>
          <div>
            Id
          </div>
          <input
            disabled={true}
            type="text"
            value={toolboxState.id}
          />
        </div>
        <div className={S.inputContainer}>
          <div>
            Package
          </div>
          <input
            disabled={true}
            type="text"
            value={toolboxState.schemaExtensionId}
          />
        </div>
      </div>

      <div className={S.draggableItems}>
        {toolboxState.fields.map(field => getToolboxComponent(field))}
        {/*<div*/}
        {/*  className={S.toolItem}*/}
        {/*  draggable*/}
        {/*  onDragStart={() => onDragStart('Label')}*/}
        {/*>*/}
        {/*  Label*/}
        {/*</div>*/}
        {/*<div*/}
        {/*  className={S.toolItem}*/}
        {/*  draggable*/}
        {/*  onDragStart={() => onDragStart('GroupBox')}*/}
        {/*>*/}
        {/*  GroupBox*/}
        {/*</div>*/}
      </div>
    </div>
  );
});

const DesignSurface: React.FC<{
  surfaceState: DesignSurfaceState
}> = observer(({surfaceState}) => {
  const surfaceRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Delete' && surfaceState.selectedComponentId) {
        surfaceState.deleteComponent(surfaceState.selectedComponentId);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
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

    surfaceState.createDraggedComponent(dropX, dropY);
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

    if (surfaceState.isDragging) {
      surfaceState.endDragging(mouseX, mouseY);
    }
    if (surfaceState.isResizing) {
      surfaceState.endResizing();
    }
  };

  const handleComponentClick = (e: React.MouseEvent, component: IComponent) => {
    e.stopPropagation();
    surfaceState.selectComponent(component.id);
  };

  const handleSurfaceClick = () => {
    surfaceState.selectComponent(null);
  };

  const handleResizeStart = (e: React.MouseEvent, component: IComponent, handle: ResizeHandle) => {
    e.stopPropagation();
    if (!surfaceRef.current) return;

    const surfaceRect = surfaceRef.current.getBoundingClientRect();
    const mouseX = e.clientX - surfaceRect.left;
    const mouseY = e.clientY - surfaceRect.top;

    surfaceState.startResizing(component, handle, mouseX, mouseY);
  };

  function getDesignSurfaceRepresentation(component: IComponent) {
    return (
      component.data.type === 'GroupBox' ? (
        <div className={S.groupBoxContent}>
          <div className={S.groupBoxHeader}>{component.data.name}</div>
        </div>
      ) : (
        <div className={S.designSurfaceEditorContainer}>
          <div style={{width: "100px"}}>{component.data.name}</div>
          <div className={S.designSurfaceEditor}></div>
        </div>
      )
    );
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
        <div
          key={component.id}
          className={`${S.designComponent} 
            ${surfaceState.draggingComponentId === component.id ? S.dragging : ''} 
            ${surfaceState.selectedComponentId === component.id ? S.selected : ''}`}
          style={{
            left: `${component.left}px`,
            top: `${component.top}px`,
            width: `${component.width}px`,
            height: `${component.height}px`,
            cursor: surfaceState.draggingComponentId === component.id ? 'move' : 'default',
            zIndex: component.type === 'GroupBox' ? 0 : 1
          }}
          onMouseDown={(e) => handleComponentMouseDown(e, component)}
          onClick={(e) => handleComponentClick(e, component)}
        >

          {getDesignSurfaceRepresentation(component)}

          {surfaceState.selectedComponentId === component.id && [
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
      <DesignSurface surfaceState={designerState.surface}/>
    </div>
  );
};

export default ComponentDesigner;