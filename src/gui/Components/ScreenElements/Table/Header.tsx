import React from "react";
import { observer } from "mobx-react";
import S from "./Header.module.css";
import { IOrderByDirection } from "./types";

@observer
export class Header extends React.Component<{
  width: number;
  label: string;
  orderingDirection: IOrderByDirection;
  orderingOrder: number;
}> {
  render() {
    return (
      <div
        className={S.header}
        style={{ minWidth: this.props.width, maxWidth: this.props.width }}
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
    );
  }
}
