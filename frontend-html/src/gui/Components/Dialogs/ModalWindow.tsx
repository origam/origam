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

import S from "./ModalWindow.module.scss";
import React from "react";

import { observer, Observer } from "mobx-react";
import { action, observable,
  makeObservable
} from "mobx";
import Measure, { BoundingRect } from "react-measure";
import { requestFocus } from "utils/focus";

@observer
export class ModalWindow extends React.Component<React.PropsWithChildren<{
  title: React.ReactNode;
  titleButtons: React.ReactNode;
  titleIsWorking?: boolean;
  buttonsLeft: React.ReactNode;
  buttonsRight: React.ReactNode;
  buttonsCenter: React.ReactNode;
  width?: number;
  height?: number;
  fullScreen?: boolean;
  topPosiotionProc?: number;
  onKeyDown?: (event: any) => void;
  onWindowMove?: (top: number, left: number)=>void;
}>> {
  constructor(props: any, context?: any) {
    super(props, context);
    makeObservable(this);
  }

  @observable _top: number = window.screen.height + 50;
  set top(value: number){
    this._top = value;
    if(this.props.onWindowMove && this.reportingWindowMove){
      this.props.onWindowMove(this._top, this._left);
    }
  }
  get top(){
    return this._top;
  }
  @observable _left: number = window.screen.width + 50;
  set left(value: number){
    this._left = value;
    if(this.props.onWindowMove && this.reportingWindowMove){
      this.props.onWindowMove(this._top, this._left);
    }
  }
  get left(){
    return this._left;
  }
  @observable isDragging = false;

  reportingWindowMove = false;
  dragStartMouseX = 0;
  dragStartMouseY = 0;
  dragStartPosX = 0;
  dragStartPosY = 0;

  @action.bound handleResize(contentRect: { bounds: BoundingRect }) {
    if (this.props.topPosiotionProc) {
      this.top = window.innerHeight * this.props.topPosiotionProc / 100;
    } else {
      this.top = window.innerHeight / 2 - contentRect.bounds!.height / 2;
    }
    this.left = window.innerWidth / 2 - contentRect.bounds!.width / 2;
  }

  @action.bound handleTitleMouseDown(event: any) {
    if(!this.reportingWindowMove){
      this.reportingWindowMove = true;
      if(this.props.onWindowMove){
        this.props.onWindowMove(this._top, this._left);
      }
    }
    window.addEventListener("mousemove", this.handleWindowMouseMove);
    window.addEventListener("mouseup", this.handleWindowMouseUp);
    this.isDragging = true;
    this.dragStartMouseX = event.screenX;
    this.dragStartMouseY = event.screenY;
    this.dragStartPosX = this.left;
    this.dragStartPosY = this.top;
  }

  @action.bound handleWindowMouseMove(event: any) {
    this.top = this.dragStartPosY + event.screenY - this.dragStartMouseY;
    this.left = this.dragStartPosX + event.screenX - this.dragStartMouseX;
    event.preventDefault();
    event.stopPropagation();
  }

  @action.bound handleWindowMouseUp(event: any) {
    window.removeEventListener("mousemove", this.handleWindowMouseMove);
    window.removeEventListener("mouseup", this.handleWindowMouseUp);
    this.isDragging = false;
  }

  onKeyDown(event: any) {
    this.props.onKeyDown?.(event);
  }

  _focusHookIsOn = false;

  footerFocusHookEnsureOn() {
    if (this.elmFooter && !this._focusHookIsOn) {
      this.elmFooter.addEventListener(
        "keydown",
        (evt: any) => {
          if (evt.key === "Tab") {
            evt.preventDefault();
            if (evt.shiftKey) {
              if (evt.target.previousSibling) {
                requestFocus(evt.target.previousSibling);
              } else {
                requestFocus(this.elmFooter?.lastChild);
              }
            } else {
              if (evt.target.nextSibling) {
                requestFocus(evt.target.nextSibling);
              } else {
                requestFocus(this.elmFooter?.firstChild);
              }
            }
          }
        },
        true
      );
      this._focusHookIsOn = true;
    }
  }

  componentDidMount() {
    this.footerFocusHookEnsureOn();
  }

  componentWillUnmount() {
  }

  refFooter = (elm: any) => {
    this.elmFooter = elm;
    if (elm) {
      this.footerFocusHookEnsureOn();
    }
  };
  elmFooter: any;

  renderFooter(buttonsLeft: React.ReactNode, buttonsCenter: React.ReactNode, buttonsRight: React.ReactNode) {
    if (buttonsLeft || buttonsCenter || buttonsRight) {
      return (
        <div ref={this.refFooter} className={S.footer}>
          {buttonsLeft}
          {buttonsCenter ? buttonsCenter : <div className={S.pusher}/>}
          {buttonsRight}
        </div>
      );
    } else {
      return null;
    }
  }

  render() {
    const {
      buttonsCenter,
      buttonsLeft,
      buttonsRight,
      children,
      fullScreen,
      height,
      title,
      titleButtons,
      titleIsWorking,
      width,
    } = this.props;
    const footer = this.renderFooter(buttonsLeft, buttonsCenter, buttonsRight);
    return (
      <Measure bounds={true} onResize={this.handleResize}>
        {({measureRef}) => (
          <Observer>
            {() => (
              <div
                ref={measureRef}
                className={S.modalWindow}
                style={{
                  top: fullScreen ? 0 : this.top,
                  left: fullScreen ? 0 : this.left,
                  minWidth: fullScreen ? "100%" : width,
                  minHeight: fullScreen ? "100%" : height,
                }}
                tabIndex={0}
                onKeyDown={(event: any) => this.onKeyDown(event)}
              >
                {title && (
                  <div className={S.title} onMouseDown={this.handleTitleMouseDown}>
                    <div className={S.label}>
                      <div className={S.labelText}>{title}</div>
                      {titleIsWorking && (
                        <div className={S.progressIndicator}>
                          <div className={S.indefinite}/>
                        </div>
                      )}
                    </div>

                    <div className={S.buttons}>{titleButtons}</div>
                  </div>
                )}
                <div className={S.body}>{children}</div>
                {footer}
              </div>
            )}
          </Observer>
        )}
      </Measure>
    );
  }
}

