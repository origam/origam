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

import { action, observable } from "mobx";
import * as React from "react";
import { IScrollerProps } from "./types";
import { MobXProviderContext, observer } from "mobx-react";
import S from "./Scroller.module.css";
import { busyDelayMillis } from "../../../../utils/flow";
import { requestFocus } from "utils/focus";

/*
  Two divs broadcasting the outer one's scroll state to scrollOffsetTarget.
*/
@observer
export default class Scroller extends React.Component<IScrollerProps> {
  static contextType = MobXProviderContext;

  @observable.ref private elmScrollerDiv: HTMLDivElement | null = null;
  private lastScrollLeft: number = 0;
  private lastScrollTop: number = 0;
  private clickHandler: SequentialSingleDoubleClickHandler = new SequentialSingleDoubleClickHandler(
    (event: any) => this.runOnclick(event)
  );

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

  @action.bound
  private handleScroll(event: any) {
    if (this.props.scrollingDisabled) {
      event.target.scrollLeft = this.lastScrollLeft;
      event.target.scrollTop = this.lastScrollTop;
    } else {
      this.lastScrollLeft = event.target.scrollLeft;
      this.lastScrollTop = event.target.scrollTop;
    }

    this.props.onScroll(event, event.target.scrollLeft, event.target.scrollTop);
  }

  forceScrollLeft(left: number) {
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
    setTimeout(()=>{
      if(this.elmScrollerDiv?.style["width"] === "0px"){
        console.warn("Focus was requested on an invisible table. This should not happen.");
        return;
      }
      if (this.props.canFocus()) {
        requestFocus(this.elmScrollerDiv);
      }
    });
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

  lastMouseX = 0;
  lastMouseY = 0;
  timeout: NodeJS.Timeout | undefined;

  @action.bound
  handleMouseMove(event: React.MouseEvent<HTMLDivElement, MouseEvent>) {
    event.persist();
    const scrollerRect = this.elmScrollerDiv!.getBoundingClientRect();
    this.props.onMouseMove?.(
      event,
      event.clientX - scrollerRect.left,
      event.clientY - scrollerRect.top
    );
    const distanceSinceLastMove = Math.sqrt(
      (event.clientX - this.lastMouseX) ** 2 + (event.clientY - this.lastMouseY) ** 2
    );
    this.lastMouseX = event.clientX;
    this.lastMouseY = event.clientY;

    if (distanceSinceLastMove > 1) {
      if (this.timeout) {
        clearTimeout(this.timeout);
      }

      const boundingRectangle = this.elmScrollerDiv!.getBoundingClientRect();
      this.timeout = setTimeout(() => {
        this.props.onMouseOver(event, boundingRectangle);
      }, 500);
    }
  }

  handleMouseLeave(event: any) {
    if (this.timeout) {
      clearTimeout(this.timeout);
    }
    this.props.onMouseLeave(event);
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
        style={{width: this.props.width, height: this.props.height}}
        onScroll={this.handleScroll}
        onClick={(e) => this.clickHandler.handleClick(e)}
        onMouseMove={(event) => this.handleMouseMove(event)}
        onMouseLeave={(event) => this.handleMouseLeave(event)}
        onKeyDown={(event) => this.props.onKeyDown?.(event)}
        onFocus={() => this.props.onFocus()}
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
  private readonly doubleClickDelayMillis = busyDelayMillis;

  constructor(runOnclick: (event: any) => void) {
    this.runOnclick = runOnclick;
  }

  sleep(ms: number) {
    return new Promise((resolve) => (this.timer = setTimeout(resolve, ms)));
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
      if (clickDistance(this.firstEvent, event) < 5) {
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
