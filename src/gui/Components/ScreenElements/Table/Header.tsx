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
  onColumnWidthChange?: (id: string, newWidth: number) => void;
}> {
  width0: number = 0;
  mouseX0: number = 0;

  @action.bound handleHeaderWidthHandleMouseDown(event: any) {
    event.preventDefault();
    this.width0 = this.props.width;
    this.mouseX0 = event.screenX;
    window.addEventListener("mousemove", this.handleWindowMouseMove);
    window.addEventListener("mouseup", this.handleWindowMouseUp);
  }

  @action.bound handleWindowMouseMove(event: any) {
    const shVecX = event.screenX - this.mouseX0;
    const width1 = this.width0 + shVecX;
    this.props.onColumnWidthChange && this.props.onColumnWidthChange(this.props.id, width1);
  }

  @action.bound handleWindowMouseUp(event: any) {
    window.removeEventListener("mousemove", this.handleWindowMouseMove);
    window.removeEventListener("mouseup", this.handleWindowMouseUp);
  }

  render() {
    console.log('Rendering header', this.props.label)
    return (
      <>
        <div
          className={S.header}
          style={{
            minWidth: this.props.width - 4,
            maxWidth: this.props.width - 4
          }}
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
