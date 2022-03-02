/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

import React from "react";
import { getTableViewProperties } from "model/selectors/TablePanelView/getTableViewProperties";
import { IDataView } from "model/entities/types/IDataView";
import { DragDropContext, Draggable, DraggableProvided, Droppable, DropResult } from "react-beautiful-dnd";
import { onColumnOrderChangeFinished } from "model/actions-ui/DataView/TableView/onColumnOrderChangeFinished";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import S from "./ColumnOrderEditor.module.scss";
import { observer } from "mobx-react";

export const ColumnOrderEditor: React.FC<{
  dataView: IDataView
}> = observer((props) => {

  let properties = getTableViewProperties(props.dataView);

  function onColumnDragEnd(result: DropResult) {
    if (!result.destination) {
      return;
    }
    const tablePanelView = getTablePanelView(props.dataView);
    onColumnOrderChangeFinished(
      tablePanelView,
      tablePanelView.tablePropertyIds[result.source.index],
      tablePanelView.tablePropertyIds[result.destination.index]
    );
  }

  function renderRow(caption: string, provided: DraggableProvided){
    return(
      <div
        className={S.row}
        ref={provided?.innerRef}
        {...provided?.draggableProps}
        {...provided.dragHandleProps}
      >
        {caption}
      </div>
    );
  }

  return (

      <DragDropContext onDragEnd={(result) => onColumnDragEnd(result)}>
        <Droppable droppableId="mobileHeaders" direction="vertical">
          {(provided) =>
            <div  className={S.root} {...provided.droppableProps} ref={provided.innerRef}>
              {
                properties.map((prop, i) =>
                  <Draggable draggableId={prop.id} index={i} key={prop.id}>
                    {(provided) => renderRow(prop.name, provided)}
                  </Draggable>
                )
              }
              {provided.placeholder}
            </div>
          }
        </Droppable>
      </DragDropContext>
  );
});
