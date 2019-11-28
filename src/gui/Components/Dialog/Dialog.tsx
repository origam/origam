import S from "./Dialog.module.scss";
import React from "react";

import * as ReactDOM from "react-dom";
import { observer, Observer } from "mobx-react";
import { observable, action } from "mobx";
import Measure, { BoundingRect } from "react-measure";
import { Icon } from "gui02/components/Icon/Icon";

export class ModalWindowOverlay extends React.Component {
  render() {
    return ReactDOM.createPortal(
      <div className={S.modalWindowOverlay}>{this.props.children}</div>,
      document.getElementById("modal-window-portal")!
    );
  }
}

export class ModalWindowNoOverlay extends React.Component {
  render() {
    return ReactDOM.createPortal(
      this.props.children,
      document.getElementById("modal-window-portal")!
    );
  }
}

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
}> {
  @observable top: number = 0;
  @observable left: number = 0;
  @observable isDragging = false;

  dragStartMouseX = 0;
  dragStartMouseY = 0;
  dragStartPosX = 0;
  dragStartPosY = 0;

  isInitialized = false;

  @action.bound handleResize(contentRect: { bounds: BoundingRect }) {
    if (
      !this.isInitialized &&
      contentRect.bounds!.height &&
      contentRect.bounds!.width
    ) {
      this.isInitialized = true;
      this.top = window.innerHeight / 2 - contentRect.bounds!.height / 2;
      this.left = window.innerWidth / 2 - contentRect.bounds!.width / 2;
    }
  }

  @action.bound handleTitleMouseDown(event: any) {
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

  render() {
    return (
      <Measure bounds={true} onResize={this.handleResize}>
        {({ measureRef }) => (
          <Observer>
            {() => (
              <div
                ref={measureRef}
                className={S.modalWindow}
                style={{
                  top: this.top,
                  left: this.left,
                  minWidth: this.props.width,
                  minHeight: this.props.height
                }}
              >
                <div
                  className={S.title}
                  onMouseDown={this.handleTitleMouseDown}
                >
                  <div className={S.label}>
                    <div className={S.labelText}>{this.props.title}</div>
                    {this.props.titleIsWorking && (
                      <div className={S.progressIndicator}>
                        <div className={S.indefinite} />
                      </div>
                    )}
                  </div>

                  <div className={S.buttons}>{this.props.titleButtons}</div>
                </div>
                <div className={S.body}>{this.props.children}</div>
                <div className={S.footer}>
                  {this.props.buttonsLeft}
                  {this.props.buttonsCenter ? (
                    this.props.buttonsCenter
                  ) : (
                    <div className={S.pusher} />
                  )}
                  {this.props.buttonsRight}
                </div>
              </div>
            )}
          </Observer>
        )}
      </Measure>
    );
  }
}

export const CloseButton = (props: { onClick?: (event: any) => void }) => (
  <button className={S.btnClose} onClick={props.onClick}>
    <div className={S.btnIconContainer}>
      <Icon src="./icons/close.svg" />
    </div>
  </button>
);
