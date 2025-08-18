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
import { action, observable } from "mobx";
import Measure, { BoundingRect } from "react-measure";
import { requestFocus } from "utils/focus";

@observer
export class ModalWindow extends React.Component<{
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
}> {
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

  renderFooter() {
    if (this.props.buttonsLeft || this.props.buttonsCenter || this.props.buttonsRight) {
      return (
        <div ref={this.refFooter} className={S.footer}>
          {this.props.buttonsLeft}
          {this.props.buttonsCenter ? this.props.buttonsCenter : <div className={S.pusher}/>}
          {this.props.buttonsRight}
        </div>
      );
    } else {
      return null;
    }
  }

  render() {
    return (
      <Measure bounds={true} onResize={this.handleResize}>
        {({measureRef}) => (
          <Observer>
            {() => (
              <div
                ref={measureRef}
                className={S.modalWindow}
                style={{
                  top: this.props.fullScreen ? 0 : this.top,
                  left: this.props.fullScreen ? 0 : this.left,
                  minWidth: this.props.fullScreen ? "100%" : this.props.width,
                  minHeight: this.props.fullScreen ? "100%" : this.props.height,
                }}
                tabIndex={0}
                onKeyDown={(event: any) => this.onKeyDown(event)}
              >
                {this.props.title && (
                  <div className={S.title} onMouseDown={this.handleTitleMouseDown}>
                    <div className={S.label}>
                      <div className={S.labelText}>{this.props.title}</div>
                      {this.props.titleIsWorking && (
                        <div className={S.progressIndicator}>
                          <div className={S.indefinite}/>
                        </div>
                      )}
                    </div>

                    <div className={S.buttons}>{this.props.titleButtons}</div>
                  </div>
                )}
                <div className={S.body}>{this.props.children}</div>
                {this.renderFooter()}
              </div>
            )}
          </Observer>
        )}
      </Measure>
    );
  }
}

