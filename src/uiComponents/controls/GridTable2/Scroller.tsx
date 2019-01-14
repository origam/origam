import { action, observable } from "mobx";
import * as React from "react";
import { IScrollerProps } from "./types";
import { observer } from "mobx-react";
import { SSL_OP_ALLOW_UNSAFE_LEGACY_RENEGOTIATION } from "constants";

/*
  Two divs broadcasting the outer one's scroll state to scrollOffsetTarget.
*/
@observer
export default class Scroller extends React.Component<IScrollerProps> {
  @observable.ref private elmScrollerDiv: HTMLDivElement | null;

  @action.bound private handleScroll(event: any) {
    this.props.scrollOffsetTarget.setScrollOffset(
      event.target.scrollTop,
      event.target.scrollLeft
    );
  }

  public set scrollTop(value: number) {
    if (this.elmScrollerDiv) {
      this.elmScrollerDiv.scrollTop = value;
    }
  }

  public set scrollLeft(value: number) {
    if (this.elmScrollerDiv) {
      this.elmScrollerDiv.scrollLeft = value;
    }
  }

  public get horizontalScrollbarSize() {
    return this.elmScrollerDiv
      ? this.elmScrollerDiv.offsetHeight - this.elmScrollerDiv.clientHeight
      : 0;
  }

  public get verticalScrollbarSize() {
    return this.elmScrollerDiv
      ? this.elmScrollerDiv.offsetWidth - this.elmScrollerDiv.clientWidth
      : 0;
  }

  public focus() {
    this.elmScrollerDiv && this.elmScrollerDiv.focus();
  }

  @action.bound
  private refScrollerDiv(elm: HTMLDivElement) {
    this.elmScrollerDiv = elm;
  }

  @action.bound private handleClick(event: any) {
    const scrollerRect = this.elmScrollerDiv!.getBoundingClientRect();
    this.props.onClick &&
      this.props.onClick(
        event,
        event.clientX - scrollerRect.left,
        event.clientY - scrollerRect.top
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
        tabIndex={-1}
        style={{ width: this.props.width, height: this.props.height }}
        onScroll={this.handleScroll}
        onClick={this.handleClick}
        onKeyDown={this.props.onKeyDown}
        ref={this.refScrollerDiv}
      >
        <div
          className="grid-table-fake-content"
          style={{
            width: this.props.contentWidth + this.horizontalScrollbarSize,
            height: this.props.contentHeight + this.verticalScrollbarSize
          }}
        />
      </div>
    );
  }
}
