import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import Measure from "react-measure";
import { ContentRect } from "react-measure";
import S from "./Dropdowner.module.scss";
import { observable, action } from "mobx";
import { observer, Observer } from "mobx-react";

class DroppedBox extends React.Component<{
  triggerRect: ContentRect;
  dropdownRect: ContentRect;
  dropdownRef: any;
  onCloseRequest?: (event: any) => void;
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
      this.props.onCloseRequest && this.props.onCloseRequest(event);
    }
  }

  render() {
    let style: any = {};

    const viewportWidth = Math.max(
      document.documentElement.clientWidth,
      window.innerWidth || 0
    );
    const viewportHeight = Math.max(
      document.documentElement.clientHeight,
      window.innerHeight || 0
    );

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
      style.left = tBounds.left + tBounds.width - dBounds.width || 0;
    }

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
    setDropped: (state: boolean) => void;
  }) => React.ReactNode;
  content: (args: { setDropped: (state: boolean) => void }) => React.ReactNode;
  onDroppedDown?: () => void;
  onContainerMouseDown?(event: any): void;
}> {
  refMeasTrigger = (elm: any) => (this.elmMeasTrigger = elm);
  elmMeasTrigger: any | null = null;

  refMeasDropdown = (elm: any) => (this.elmMeasDropdown = elm);
  elmMeasDropdown: any | null = null;

  @observable isDropped = false;

  @action.bound
  setDropped(state: boolean) {
    this.isDropped = state;
    if (state) {
      this.reMeasure();
      this.props.onDroppedDown && this.props.onDroppedDown();
    } else {
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
        {({
          measureRef: mRefTrigger,
          contentRect: cRectTrigger,
          measure: measureTrigger
        }) => (
          <Measure bounds={true} ref={this.refMeasDropdown}>
            {({
              measureRef: mRefDropdown,
              contentRect: cRectDropdown,
              measure: measureDropdown
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
                      setDropped: this.setDropped
                    })}
                    {this.isDropped && (
                      <DroppedBox
                        triggerRect={cRectTrigger}
                        dropdownRect={cRectDropdown}
                        dropdownRef={mRefDropdown}
                        onCloseRequest={() => {
                          this.setDropped(false);
                        }}
                      >
                        {this.props.content({ setDropped: this.setDropped })}
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
