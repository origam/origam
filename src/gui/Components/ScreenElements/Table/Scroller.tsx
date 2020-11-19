import {action, observable} from "mobx";
import * as React from "react";
import {IScrollerProps} from "./types";
import {observer} from "mobx-react";
import S from "./Scroller.module.css";
import {busyDelayMillis} from "../../../../utils/flow";

/*
  Two divs broadcasting the outer one's scroll state to scrollOffsetTarget.
*/
@observer
export default class Scroller extends React.Component<IScrollerProps> {
  @observable.ref private elmScrollerDiv: HTMLDivElement | null = null;
  private lastScrollLeft: number = 0;
  private lastScrollTop: number = 0;
  private clickHandler: SequentialSingleDoubleClickHandler =
    new SequentialSingleDoubleClickHandler((event: any) => this.runOnclick(event));

  @action.bound scrollTo(coords: { scrollLeft?: number; scrollTop?: number }) {
    if (this.elmScrollerDiv) {
      if (coords.scrollTop !== undefined) {
        this.elmScrollerDiv.scrollTop = coords.scrollTop;
      }
      if (coords.scrollLeft !== undefined) {
        this.forceScrollLeft(coords.scrollLeft); // will scroll even if this.props.scrollingDisabled is true
        this.elmScrollerDiv.scrollLeft = coords.scrollLeft;
      }
    }
  }

  @action.bound private handleScroll(event: any) {

    if(this.props.scrollingDisabled){
      event.target.scrollLeft = this.lastScrollLeft;
      event.target.scrollTop = this.lastScrollTop;
    }
    else{
      this.lastScrollLeft =  event.target.scrollLeft
      this.lastScrollTop =  event.target.scrollTop
    }

    this.props.onScroll(
      event,
      event.target.scrollLeft,
      event.target.scrollTop
    );
  }

  forceScrollLeft(left: number){
    this.lastScrollLeft = left;
  }

  set scrollTop(value: number) {
    if (this.elmScrollerDiv) {
      this.elmScrollerDiv.scrollTop = value;
    }
  }

  set scrollLeft(value: number) {
    if (this.elmScrollerDiv) {
      this.elmScrollerDiv.scrollLeft = value;
    }
  }

  get scrollTop() {
    return this.elmScrollerDiv ? this.elmScrollerDiv.scrollTop : 0;
  }

  get scrollLeft() {
    return this.elmScrollerDiv ? this.elmScrollerDiv.scrollLeft : 0;
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

  private runOnclick(event: any) {
    const scrollerRect = this.elmScrollerDiv!.getBoundingClientRect();
    this.props.onClick &&
      this.props.onClick(
        event,
        event.clientX - scrollerRect.left,
        event.clientY - scrollerRect.top
      );
  }

  @action.bound
  private handleWindowClick(event: any) {
    if (this.elmScrollerDiv && !this.elmScrollerDiv.contains(event.target)) {
      this.props.onOutsideClick && this.props.onOutsideClick(event);
    }
  }

  public componentDidMount() {
    window.addEventListener("click", this.handleWindowClick);
  }

  public componentWillUnmount() {
    window.removeEventListener("click", this.handleWindowClick);
  }

  public render() {
    return (
      <div
        className={
          S.scroller +
          (1 ? " horiz-scrollbar" : "") +
          (1 ? " vert-scrollbar" : "") +
          (!this.props.isVisible ? " hidden" : "")
        }
        tabIndex={1}
        style={{ width: this.props.width, height: this.props.height }}
        onScroll={this.handleScroll}
        onClick={(e) => this.clickHandler.handleClick(e)}
        onKeyDown={(event) => this.props.onKeyDown?.(event)}
        ref={this.refScrollerDiv}
      >
        <div
          className={S.fakeContent}
          style={{
            width: this.props.contentWidth + this.horizontalScrollbarSize,
            height: this.props.contentHeight + this.verticalScrollbarSize,
          }}
        />
      </div>
    );
  }
}

// Ensures that single click is handled before double click and
// that single click callback does not prevent double click from being registered
class SequentialSingleDoubleClickHandler {

  private timer: any = null;
  private readonly runOnclick: (event: any) => void;
  private singleClickIsRunning = false;
  firstEvent: any | undefined;
  private readonly doubleClickDelayMillis =  busyDelayMillis;

  constructor(runOnclick: (event: any) => void) {
    this.runOnclick = runOnclick;
  }

  sleep(ms: number) {
    return new Promise(resolve => this.timer = setTimeout(resolve, ms));
  }

  @action.bound
  async handleClick(event: any) {
    event.persist();
    event.preventDefault();

    if (!this.singleClickIsRunning) {
      this.singleClickIsRunning = true;
      this.singleClick(event);
      await this.sleep(this.doubleClickDelayMillis);
      this.singleClickIsRunning = false;
    } else {
      if(clickDistance(this.firstEvent, event) < 5){
        this.doubleClick(event);
      }
      this.firstEvent = undefined;
      this.singleClickIsRunning = false;
    }
  }

  private singleClick(event: any) {
    this.firstEvent = event;
    event.isDouble = false;
    this.runOnclick(event);
  }

  private doubleClick(event: any) {
    event.isDouble = true;
    this.runOnclick(event);
  }
}

function clickDistance(event1: any, event2: any) {
  return Math.sqrt((event1.screenX - event2.screenX) ** 2 + (event1.screenY - event2.screenY) ** 2);
}
