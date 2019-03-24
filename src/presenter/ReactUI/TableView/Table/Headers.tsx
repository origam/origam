import * as React from "react";
import { IHeaderRowProps } from "./types";
import { observer } from "mobx-react";
import { IHeader } from "src/presenter/types/ITableViewPresenter/IHeader";
import { IOrderByDirection } from "src/presenter/types/ITableViewPresenter/IOrderByDirection";

@observer
export class HeaderRow extends React.Component<IHeaderRowProps> {
  render() {
    const headers = [];
    for (
      let i = this.props.columnStartIndex;
      i < this.props.columnEndIndex;
      i++
    ) {
      headers.push(
        renderHeader(
          i,
          this.props.gridDimensions.getColumnWidth(i),
          this.props.headers.getHeader(i)
        )
      );
    }
    return headers;
  }
}

@observer
export class Header extends React.Component<{
  width: number;
  header: IHeader;
}> {
  render() {
    return (
      <div
        className="header"
        style={{ minWidth: this.props.width, maxWidth: this.props.width }}
      >
        <div className="label">{this.props.header.label}</div>
        {this.props.header.orderBy.direction !== IOrderByDirection.NONE && (
          <div className="order">
            {this.props.header.orderBy.order > 0 && (
              <span>{this.props.header.orderBy.order}</span>
            )}
            {this.props.header.orderBy.direction === IOrderByDirection.ASC && (
              <i className="fas fa-caret-up" />
            )}
            {this.props.header.orderBy.direction === IOrderByDirection.DESC && (
              <i className="fas fa-caret-down" />
            )}
          </div>
        )}
      </div>
    );
  }
}

const renderHeader = (
  columnIndex: number,
  columnWidth: number,
  header: IHeader
) => {
  return <Header key={columnIndex} header={header} width={columnWidth} />;
};
