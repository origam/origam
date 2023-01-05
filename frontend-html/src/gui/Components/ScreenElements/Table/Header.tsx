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
import { observer } from "mobx-react";
import S from "./Header.module.scss";
import { IOrderByDirection } from "model/entities/types/IOrderingConfiguration";
import { Draggable, DraggableProvided } from "react-beautiful-dnd";
import { action, observable } from "mobx";

const MIN_COLUMN_WIDTH = 30;

@observer
export class Header extends React.Component<{
  id: string;
  width: number;
  label: string;
  isFixed: boolean;
  isFirst: boolean;
  isLast: boolean;
  columnIndex: number;
  orderingDirection: IOrderByDirection;
  orderingOrder: number;
  additionalHeaderContent?: () => React.ReactNode;
  onColumnWidthChange: (id: string, newWidth: number) => void;
  onColumnWidthChangeFinished: (id: string, newWidth: number) => void;
  onClick?: (event: any, id: string) => void;
  isDragDisabled: boolean;
}> {
  width0: number = 0;
  mouseX0: number = 0;
  mouseY0: number = 0;
  @observable isMouseIn: boolean = false;
  width1: number = 0;

  @action.bound handleHeaderWidthHandleMouseDown(event: any) {
    event.preventDefault();
    this.width0 = this.props.width;
    this.width1 = this.props.width;
    this.mouseX0 = event.screenX;
    this.mouseY0 = event.screenY;
    window.addEventListener("mousemove", this.handleWindowMouseMoveForColumnWidthChange);
    window.addEventListener("mouseup", this.handleWindowMouseUpForColumnWidthChange);
  }

  @action.bound handleWindowMouseMoveForColumnWidthChange(event: any) {
    const shVecX = event.screenX - this.mouseX0;
    this.width1 = this.width0 + shVecX;
    if (this.width1 >= MIN_COLUMN_WIDTH) {
      this.props.onColumnWidthChange && this.props.onColumnWidthChange(this.props.id, this.width1);
    }
  }

  @action.bound handleWindowMouseUpForColumnWidthChange(event: any) {
    window.removeEventListener("mousemove", this.handleWindowMouseMoveForColumnWidthChange);
    window.removeEventListener("mouseup", this.handleWindowMouseUpForColumnWidthChange);
    this.props.onColumnWidthChangeFinished &&
    this.props.onColumnWidthChangeFinished(this.props.id, this.width1);
  }

  makeHeaderStyle() {
    const headerDividerWidth = this.props.isLast ? 3 : 5;
    const style = {
      minWidth: this.props.width - headerDividerWidth,
      maxWidth: this.props.width - headerDividerWidth,
    } as any;
    if (this.props.isFirst) {
      style["paddingLeft"] = "1.9em";
    }
    return style;
  }

  render() {
    if (this.props.isFixed) {
      return this.renderHeader();
    }
    return (
      <Draggable
        draggableId={this.props.id}
        index={this.props.columnIndex}
        key={this.props.id}
        isDragDisabled={this.props.isDragDisabled}
      >
        {(provided) => this.renderHeader(provided)}
      </Draggable>
    );
  }

  private renderHeader(provided?: DraggableProvided) {
    return <div className={S.root} ref={provided?.innerRef} {...provided?.draggableProps}>
      <div
        className={S.leftSeparator}
      />
      <div
        {...provided?.dragHandleProps}
        className={S.header}
        onClick={(event) => this.props.onClick && this.props.onClick(event, this.props.id)}
        style={this.makeHeaderStyle()}
        title={this.props.label}
      >
        <div className={S.inHeaderRow}>
          <div
            className={S.label}
              >
            {this.props.label}
          </div>
          {this.props.orderingDirection !== IOrderByDirection.NONE && (
            <div className={S.order}>
              {this.props.orderingOrder > 0 && <span>{this.props.orderingOrder}</span>}
              {this.props.orderingDirection === IOrderByDirection.ASC && (
                <i className="fas fa-caret-up"/>
              )}
              {this.props.orderingDirection === IOrderByDirection.DESC && (
                <i className="fas fa-caret-down"/>
              )}
            </div>
          )}
        </div>
        {this.props.additionalHeaderContent && (
          <div className={S.additionalContentsRow}>{this.props.additionalHeaderContent()}</div>
        )}
      </div>
      <div
        onMouseDown={this.handleHeaderWidthHandleMouseDown}
        className={S.columnWidthHandle + " " + S.rightSeparator}
      />
    </div>;
  }
}
