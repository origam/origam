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
import ReactDOM from "react-dom";
import Measure, { ContentRect } from "react-measure";
import S from "./Dropdowner.module.scss";
import { action, observable } from "mobx";
import { observer, Observer } from "mobx-react";

class DroppedBox extends React.Component<{
  triggerRect: ContentRect;
  dropdownRect: ContentRect;
  dropdownRef: any;
  openEvent?: MouseEvent;
  onCloseRequest?: (event: any) => void;
  onOutsideInteraction?: (event: any) => void;
}> {
  elmDropdown: HTMLDivElement | null = null;
  refDropdown = (elm: HTMLDivElement | null) => {
    this.elmDropdown = elm;
    this.props.dropdownRef(elm);
  };

  isStillMounted = false;

  componentDidMount() {
    this.isStillMounted = true;
    setTimeout(() => {
      if (this.isStillMounted) {
        window.addEventListener("mousedown", this.handleMaybeClose);
        window.addEventListener("mousewheel", this.handleMaybeClose);
      }
    });
  }

  componentWillUnmount() {
    this.isStillMounted = false;
    window.removeEventListener("mousedown", this.handleMaybeClose);
    window.removeEventListener("mousewheel", this.handleMaybeClose);
  }

  @action.bound
  handleMaybeClose(event: any) {
    if (this.elmDropdown && !this.elmDropdown.contains(event.target)) {
      this.props.onOutsideInteraction?.(event);
      this.props.onCloseRequest && this.props.onCloseRequest(event);
    }
  }

  calcPosition() {
    let style: any = {};

    const viewportWidth = Math.max(document.documentElement.clientWidth, window.innerWidth || 0);
    const viewportHeight = Math.max(document.documentElement.clientHeight, window.innerHeight || 0);

    const tBounds = this.props.triggerRect.bounds!;
    const dBounds = this.props.dropdownRect.bounds!;

    if (tBounds.top + tBounds.height + dBounds.height + 20 < viewportHeight) {
      style.top = tBounds.top + tBounds.height || 0;
    } else {
      style.top = tBounds.top - dBounds.height || 0;
    }
    if (tBounds!.left + dBounds.width + 20 < viewportWidth) {
      style.left = tBounds.left || 0;
    } else {
      if(tBounds.left + tBounds.width - dBounds.width < 0){
        style.left = viewportWidth / 2 - dBounds.width / 2;
      }else{
        style.left = tBounds.left + tBounds.width - dBounds.width || 0;
      }
    }
    return style;
  }

  render() {
    const style: any = this.props.openEvent
      ? {left: this.props.openEvent.clientX, top: this.props.openEvent.clientY}
      : this.calcPosition();

    return ReactDOM.createPortal(
      <div ref={this.refDropdown} style={style} className={S.droppedBox}>
        {this.props.children}
      </div>,
      document.getElementById("dropdown-portal")!
    );
  }
}

@observer
export class Dropdowner extends React.Component<{
  className?: string;
  style?: any;
  trigger: (args: {
    refTrigger: any;
    measure: () => void;
    setDropped: (state: boolean, event?: any) => void;
    isDropped: boolean;
  }) => React.ReactNode;
  content: (args: { setDropped: (state: boolean) => void }) => React.ReactNode;
  onDroppedDown?: () => void;
  onDroppedUp?: () => void;
  onContainerMouseDown?(event: any): void;
  onOutsideInteraction?(event: any): void;
}> {
  refMeasTrigger = (elm: any) => (this.elmMeasTrigger = elm);
  elmMeasTrigger: any | null = null;

  refMeasDropdown = (elm: any) => (this.elmMeasDropdown = elm);
  elmMeasDropdown: any | null = null;

  @observable _isDropped = false;
  openEvent: any;

  get isDropped() {
    return this._isDropped;
  }

  set isDropped(value: boolean) {
    if (window.localStorage.getItem("debugKeepDropdownOpen") && !value) return;
    this._isDropped = value;
  }

  @action.bound
  setDropped(state: boolean, event?: any) {
    event?.persist();
    this.openEvent = event;
    if (!this.isDropped && state) {
      this.isDropped = state;
      this.reMeasure();
      this.props.onDroppedDown && this.props.onDroppedDown();
    } else if (this.isDropped && !state) {
      this.isDropped = state;
      this.props.onDroppedUp && this.props.onDroppedUp();
    }
  }

  @action.bound
  windowMouseWheel(event: any) {
    this.setDropped(false);
  }

  @action.bound
  reMeasure() {
    this.elmMeasTrigger && this.elmMeasTrigger.measure();
  }

  componentDidUpdate() {
    if (this.isDropped) {
      this.elmMeasDropdown && this.elmMeasDropdown.measure();
    }
  }

  render() {
    (() => this.isDropped)();
    return (
      <Measure bounds={true} ref={this.refMeasTrigger}>
        {({measureRef: mRefTrigger, contentRect: cRectTrigger, measure: measureTrigger}) => (
          <Measure bounds={true} ref={this.refMeasDropdown}>
            {({
                measureRef: mRefDropdown,
                contentRect: cRectDropdown,
                measure: measureDropdown,
              }) => (
              <Observer>
                {() => (
                  <div
                    className={
                      S.dropdownerContainer +
                      (this.props.className ? ` ${this.props.className}` : "")
                    }
                    style={this.props.style}
                    onMouseDown={this.props.onContainerMouseDown}
                  >
                    {this.props.trigger({
                      refTrigger: mRefTrigger,
                      measure: this.reMeasure,
                      setDropped: this.setDropped,
                      isDropped: this.isDropped,
                    })}
                    {this.isDropped && (
                      <DroppedBox
                        openEvent={this.openEvent}
                        triggerRect={cRectTrigger}
                        dropdownRect={cRectDropdown}
                        dropdownRef={mRefDropdown}
                        onCloseRequest={() => {
                          this.setDropped(false);
                          this.props.onDroppedUp?.();
                        }}
                        onOutsideInteraction={this.props.onOutsideInteraction}
                      >
                        {this.props.content({setDropped: this.setDropped})}
                      </DroppedBox>
                    )}
                  </div>
                )}
              </Observer>
            )}
          </Measure>
        )}
      </Measure>
    );
  }
}
