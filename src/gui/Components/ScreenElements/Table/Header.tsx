import React from "react";
import { observer } from "mobx-react";
import S from "./Header.module.css";
import { IOrderByDirection } from "./types";
import { action } from "mobx";

@observer
export class Header extends React.Component<{
  id: string;
  width: number;
  label: string;
  orderingDirection: IOrderByDirection;
  orderingOrder: number;
  isColumnOrderChanging: boolean;
  onColumnWidthChange?: (id: string, newWidth: number) => void;
  onStartColumnOrderChanging?: (id: string) => void;
  onStopColumnOrderChanging?: (id: string) => void;
  onColumnOrderDrop?: (id: string) => void;
  onPossibleColumnOrderChange?: (targetId: string | undefined) => void;
}> {
  width0: number = 0;
  mouseX0: number = 0;
  isMouseIn: boolean = false;

  @action.bound handleHeaderWidthHandleMouseDown(event: any) {
    event.preventDefault();
    this.width0 = this.props.width;
    this.mouseX0 = event.screenX;
    window.addEventListener(
      "mousemove",
      this.handleWindowMouseMoveForColumnWidthChange
    );
    window.addEventListener(
      "mouseup",
      this.handleWindowMouseUpForColumnWidthChange
    );
  }

  @action.bound handleWindowMouseMoveForColumnWidthChange(event: any) {
    const shVecX = event.screenX - this.mouseX0;
    const width1 = this.width0 + shVecX;
    this.props.onColumnWidthChange &&
      this.props.onColumnWidthChange(this.props.id, width1);
  }

  @action.bound handleWindowMouseUpForColumnWidthChange(event: any) {
    window.removeEventListener(
      "mousemove",
      this.handleWindowMouseMoveForColumnWidthChange
    );
    window.removeEventListener(
      "mouseup",
      this.handleWindowMouseUpForColumnWidthChange
    );
  }

  @action.bound handleHeaderMouseDown(event: any) {
    event.preventDefault();
    this.props.onStartColumnOrderChanging &&
      this.props.onStartColumnOrderChanging(this.props.id);
    window.addEventListener(
      "mousemove",
      this.handleWindowMouseMoveForColumnOrderChange
    );
    window.addEventListener(
      "mouseup",
      this.handleWindowMouseUpForColumnOrderChange
    );
  }

  @action.bound handleWindowMouseMoveForColumnOrderChange(event: any) {}

  @action.bound handleWindowMouseUpForColumnOrderChange(event: any) {
    this.props.onStopColumnOrderChanging &&
      this.props.onStopColumnOrderChanging(this.props.id);
    window.removeEventListener(
      "mouseup",
      this.handleWindowMouseUpForColumnOrderChange
    );
    window.removeEventListener(
      "mousemove",
      this.handleWindowMouseMoveForColumnOrderChange
    );
  }

  @action.bound handleMouseEnter(event: any) {
    this.isMouseIn = true;
    if (this.props.isColumnOrderChanging) {
      this.props.onPossibleColumnOrderChange &&
        this.props.onPossibleColumnOrderChange(this.props.id);
    }
  }

  @action.bound handleMouseLeave(event: any) {
    this.isMouseIn = false;
    if (this.props.isColumnOrderChanging) {
      this.props.onPossibleColumnOrderChange &&
        this.props.onPossibleColumnOrderChange(undefined);
    }
  }

  @action.bound handleMouseUp(event: any) {
    if (this.props.isColumnOrderChanging && this.isMouseIn) {
      this.props.onColumnOrderDrop &&
        this.props.onColumnOrderDrop(this.props.id);
    }
  }

  render() {
    console.log("Rendering header", this.props.label);
    return (
      <>
        <div
          className={
            S.header +
            (this.props.isColumnOrderChanging ? " changing-order" : "")
          }
          style={{
            minWidth: this.props.width - 4,
            maxWidth: this.props.width - 4
          }}
          onMouseDown={this.handleHeaderMouseDown}
          onMouseUp={this.handleMouseUp}
          onMouseEnter={this.handleMouseEnter}
          onMouseLeave={this.handleMouseLeave}
        >
          <div className={S.label}>{this.props.label}</div>
          {this.props.orderingDirection !== IOrderByDirection.NONE && (
            <div className={S.order}>
              {this.props.orderingOrder > 0 && (
                <span>{this.props.orderingOrder}</span>
              )}
              {this.props.orderingDirection === IOrderByDirection.ASC && (
                <i className="fas fa-caret-up" />
              )}
              {this.props.orderingDirection === IOrderByDirection.DESC && (
                <i className="fas fa-caret-down" />
              )}
            </div>
          )}
        </div>
        <div
          onMouseDown={this.handleHeaderWidthHandleMouseDown}
          className={S.columnWidthHandle}
        />
      </>
    );
  }
}
