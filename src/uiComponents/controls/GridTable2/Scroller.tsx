import { action } from "mobx";
import * as React from "react";
import { IScrollerProps } from './types';
import { observer } from "mobx-react";


/*
  Two divs broadcasting the outer one's scroll state to scrollOffsetTarget.
*/
@observer
export default class Scroller extends React.Component<IScrollerProps> {
  @action.bound private handleScroll(event: any) {
    this.props.scrollOffsetTarget.setScrollOffset(
      event.target.scrollTop,
      event.target.scrollLeft
    );
  }

  public render() {
    return (
      <div
        className={
          "grid-table-scroller" +
          (1 ? " horiz-scrollbar" : "") +
          (1 ? " vert-scrollbar" : "")
        }
        style={{ width: this.props.width, height: this.props.height }}
        onScroll={this.handleScroll}
      >
        <div
          className="grid-table-fake-content"
          style={{
            width: this.props.contentWidth,
            height: this.props.contentHeight
          }}
        />
      </div>
    );
  }
}
