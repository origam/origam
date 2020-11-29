import React from "react";
import { observer } from "mobx-react";
import S from "./Header.module.scss";
import { action, observable } from "mobx";
import { IOrderByDirection } from "model/entities/types/IOrderingConfiguration";
import cx from "classnames";

@observer
export class Header extends React.Component<{
  id: string;
  width: number;
  label: string;
  orderingDirection: IOrderByDirection;
  orderingOrder: number;
  isColumnOrderChanging: boolean;
  additionalHeaderContent?: () => React.ReactNode;
  onColumnWidthChange?: (id: string, newWidth: number) => void;
  onColumnWidthChangeFinished?: (id: string, newWidth: number) => void;
  onStartColumnOrderChanging?: (id: string) => void;
  onStopColumnOrderChanging?: (id: string) => void;
  onColumnOrderDrop?: (id: string) => void;
  onPossibleColumnOrderChange?: (targetId: string | undefined) => void;
  onClick?: (event: any, id: string) => void;
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
    this.props.onColumnWidthChange && this.props.onColumnWidthChange(this.props.id, this.width1);
  }

  @action.bound handleWindowMouseUpForColumnWidthChange(event: any) {
    window.removeEventListener("mousemove", this.handleWindowMouseMoveForColumnWidthChange);
    window.removeEventListener("mouseup", this.handleWindowMouseUpForColumnWidthChange);
    this.props.onColumnWidthChangeFinished &&
      this.props.onColumnWidthChangeFinished(this.props.id, this.width1);
  }

  @action.bound handleHeaderMouseDown(event: any) {
    this.isMouseIn = true;
    event.preventDefault();
    this.mouseX0 = event.screenX;
    this.mouseY0 = event.screenY;
    this.props.onStartColumnOrderChanging && this.props.onStartColumnOrderChanging(this.props.id);
    window.addEventListener("mousemove", this.handleWindowMouseMoveForColumnOrderChange);
    window.addEventListener("mouseup", this.handleWindowMouseUpForColumnOrderChange);
  }

  @action.bound handleWindowMouseMoveForColumnOrderChange(event: any) {}

  @action.bound handleWindowMouseUpForColumnOrderChange(event: any) {
    this.props.onStopColumnOrderChanging && this.props.onStopColumnOrderChanging(this.props.id);
    window.removeEventListener("mouseup", this.handleWindowMouseUpForColumnOrderChange);
    window.removeEventListener("mousemove", this.handleWindowMouseMoveForColumnOrderChange);
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
      this.props.onPossibleColumnOrderChange && this.props.onPossibleColumnOrderChange(undefined);
    }
  }

  @action.bound handleMouseUp(event: any) {
    console.log(
      this.props.isColumnOrderChanging,
      this.isMouseIn,
      event.screenX,
      this.mouseX0,
      event.screenY,
      this.mouseY0
    );
    if (this.props.isColumnOrderChanging && this.isMouseIn) {
      if (
        (event.screenX - this.mouseX0) ** 2 + (event.screenY - this.mouseY0) ** 2 <
        25 // Cursor coord change is no more than 25 px
      ) {
        this.props.onClick && this.props.onClick(event, this.props.id);
      } else {
        this.props.onColumnOrderDrop && this.props.onColumnOrderDrop(this.props.id);
      }
    }
  }

  render() {
    return (
      <>
        <div
          className={S.header /* + (this.props.isColumnOrderChanging ? " changing-order" : "")*/}
          style={{
            minWidth: this.props.width - 9,
            maxWidth: this.props.width - 9,
          }}
          onMouseUp={this.handleMouseUp}
          onMouseEnter={this.handleMouseEnter}
          onMouseLeave={this.handleMouseLeave}
          title={this.props.label}
        >
          <div className={S.inHeaderRow} onMouseDown={this.handleHeaderMouseDown}>
            <div className={S.label}>{this.props.label}</div>
            {this.props.orderingDirection !== IOrderByDirection.NONE && (
              <div className={S.order}>
                {this.props.orderingOrder > 0 && <span>{this.props.orderingOrder}</span>}
                {this.props.orderingDirection === IOrderByDirection.ASC && (
                  <i className="fas fa-caret-up" />
                )}
                {this.props.orderingDirection === IOrderByDirection.DESC && (
                  <i className="fas fa-caret-down" />
                )}
              </div>
            )}
          </div>
          {this.props.additionalHeaderContent && (
            <div className={S.inHeaderRow}>{this.props.additionalHeaderContent()}</div>
          )}
        </div>
        <div
          onMouseDown={this.handleHeaderWidthHandleMouseDown}
          className={cx(S.columnWidthHandle, {
            isColumnDropTarget: this.isMouseIn && this.props.isColumnOrderChanging,
          })}
        />
      </>
    );
  }
}
