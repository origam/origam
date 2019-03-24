import * as React from "react";
import * as ReactDOM from "react-dom";
import { observer, Observer } from "mobx-react";
import { observable, action } from "mobx";
import Measure, { BoundingRect } from "react-measure";

export class ModalWindowOverlay extends React.Component {
  render() {
    return ReactDOM.createPortal(
      <div className={"modal-window-overlay"}>{this.props.children}</div>,
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
export class ModalWindow extends React.Component {
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
                className="modal-window"
                style={{ top: this.top, left: this.left }}
              >
                <div className="title" onMouseDown={this.handleTitleMouseDown}>
                  <div className="label">Modal window title</div>
                  <div className="pusher" />
                  <div className="buttons">
                    <button className="btn-close">
                      <i className="fas fa-times icon" />
                    </button>
                  </div>
                </div>
                <div className="body">Modal window body</div>
                <div className="footer">
                  <button>Cancel</button>
                  <div className="pusher" />
                  <button>OK</button>
                </div>
              </div>
            )}
          </Observer>
        )}
      </Measure>
    );
  }
}
