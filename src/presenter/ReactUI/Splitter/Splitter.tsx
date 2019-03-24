import * as React from "react";

import { AutoSizer } from "react-virtualized";
import Measure from "react-measure";
import { Observer, observer, Provider, inject } from "mobx-react";
import { observable, action, reaction } from "mobx";
import * as _ from "lodash";

export class SplitterModel {
  constructor(initSizes?: Array<[string, number]>) {
    if (initSizes) {
      this.sizes = new Map(initSizes);
    }
  }

  @observable
  public contentRect: any = {};

  @observable
  public sizes: Map<any, number> = new Map();

  @observable
  public isMoving = false;

  @action.bound
  public adjustSize(index: number, size: number) {
    const keyLeft = Array.from(this.sizes.keys())[index];
    const keyRight = Array.from(this.sizes.keys())[index + 1];
    const newLeftSize = this.sizes.get(keyLeft)! + size;
    const newRightSize = this.sizes.get(keyRight)! - size;
    if (newLeftSize > 40 && newRightSize > 40) {
      this.sizes.set(keyLeft, newLeftSize);
      this.sizes.set(keyRight, newRightSize);
    }
  }
}

export interface ISplitterModel {
  contentRect: any;
  sizes: Map<any, number>;
  isMoving: boolean;
  adjustSize(index: number, size: number): void;
}

export interface ISplitterProps {
  horizontal?: boolean;
  name: string;
  handleSize: number;
  model?: ISplitterModel;
}

@observer
export class Splitter extends React.Component<ISplitterProps> {
  constructor(props: ISplitterProps) {
    super(props);
    if (props.model) {
      this.model = props.model;
    }
  }

  private isFirstResize = true;

  private model = new SplitterModel();

  private handleResize = _.debounce(this.handleResizeImm, 100);

  @action.bound
  private handleResizeImm(contentRect: any) {
    if (
      !contentRect.client ||
      (this.props.horizontal && contentRect.client.width === 0) ||
      (!this.props.horizontal && contentRect.client.height === 0)
    ) {
      return;
    }
    this.model.contentRect = contentRect;
    // console.log("Handle resize:", this.containerSize);

    this.adjustByContainerSize();
  }

  @action.bound
  public registerPanel(id: string, order: number) {
    if (!this.model.sizes.get(id)) {
      this.model.sizes.set(id, 1);
    }
  }

  private reAdjustSize: any;

  get containerSize() {
    return this.model.contentRect.client
      ? this.props.horizontal
        ? this.model.contentRect.client.width
        : this.model.contentRect.client.height
      : 0;
  }

  adjustByContainerSize = this.adjustByContainerSizeImm;

  lastAdjustedSize: number | undefined = undefined;
  @action.bound adjustByContainerSizeImm() {
    const size = this.containerSize;
    if(this.lastAdjustedSize !== undefined) {
      if(Math.abs(size - this.lastAdjustedSize) < 10) {
        return
      }
    }
    this.lastAdjustedSize = size;
    const sum = Array.from(this.model.sizes.values()).reduce(
      (acc, b) => acc + b,
      0
    );
    for (let key of this.model.sizes.keys()) {
      this.model.sizes.set(key, (this.model.sizes.get(key)! * size) / sum - this.props.handleSize);
    }
    const sum2 = Array.from(this.model.sizes.values()).reduce(
      (acc, b) => acc + b,
      0
    );
    console.log("After adjust: size=", size, "sum1=", sum, "sum2=", sum2);
  }

  public componentWillUnmount() {
    this.reAdjustSize && this.reAdjustSize();
  }

  public render() {
    const { horizontal } = this.props;
    const newChildren: any = [];
    const children = React.Children.toArray(this.props.children);
    React.Children.forEach(
      children,
      (child: React.ReactNode, index: number) => {
        newChildren.push(
          React.cloneElement(child as any, {
            horizontal,
            order: index,
            parent: this,
            model: this.model
          })
        );
        if (index < children.length - 1) {
          newChildren.push(
            <SplitterHandle
              key={`Handle_${index}`}
              horizontal={horizontal}
              model={this.model}
              index={index}
            />
          );
        }
      }
    );
    return (
      <Measure
        onResize={this.handleResize}
        bounds={true}
        client={true}
        margin={true}
        offset={true}
      >
        {({ measureRef, contentRect }) => {
          // console.log("MEASURE RENDER", contentRect.client);
          return (
            <Observer>
              {() => (
                <div
                  className={
                    "splitter-container" +
                    (horizontal ? " horizontal" : " vertical") +
                    (this.model.isMoving ? " moving" : "")
                  }
                  ref={measureRef}
                >
                  {newChildren}
                </div>
              )}
            </Observer>
          );
        }}
      </Measure>
    );
  }
}

@observer
export class SplitterPanel extends React.Component<{
  horizontal?: boolean;
  id: string;
  order?: number;
  parent?: Splitter;
  model?: SplitterModel;
}> {
  public componentDidMount() {
    this.props.parent!.registerPanel(this.props.id, this.props.order!);
  }

  public render() {
    const { horizontal, model } = this.props;
    return (
      <div
        className={
          "splitter-panel" + (horizontal ? " horizontal" : " vertical")
        }
        style={{
           width: horizontal ? model!.sizes.get(this.props.id) : undefined,
           height: !horizontal ? model!.sizes.get(this.props.id) : undefined
          // height: 200
        }}
      >
        {this.props.children}
      </div>
    );
  }
}

@observer
class SplitterHandle extends React.Component<{
  horizontal?: boolean;
  model: SplitterModel;
  index: number;
  onMouseDown?: (event: any) => void;
  onWindowMouseMove?: (event: any) => void;
  onWindowMouseUp?: (event: any) => void;
}> {
  private startMousePosition = 0;
  @observable private currentMousePosition = 0;

  @observable private isMoving = false;

  @action.bound
  private handleMouseDown(event: any) {
    this.props.onMouseDown && this.props.onMouseDown(event);
    this.props.model.isMoving = true;
    this.isMoving = true;
    this.startMousePosition = this.props.horizontal
      ? event.screenX
      : event.screenY;
    this.currentMousePosition = this.startMousePosition;
    window.addEventListener("mouseup", this.handleWindowMouseUp);
    window.addEventListener("mousemove", this.handleWindowMouseMove);
  }

  @action.bound
  private handleWindowMouseMove(event: any) {
    this.props.onWindowMouseMove && this.props.onWindowMouseMove(event);
    this.currentMousePosition = this.props.horizontal
      ? event.screenX
      : event.screenY;
  }

  @action.bound
  private handleWindowMouseUp(event: any) {
    this.props.onWindowMouseUp && this.props.onWindowMouseUp(event);
    this.props.model.isMoving = false;
    this.isMoving = false;
    this.props.model.adjustSize(
      this.props.index,
      this.currentMousePosition - this.startMousePosition
    );
    window.removeEventListener("mouseup", this.handleWindowMouseUp);
    window.removeEventListener("mousemove", this.handleWindowMouseMove);
  }

  public render() {
    const { horizontal } = this.props;
    return (
      <div
        onMouseDown={this.handleMouseDown}
        className={
          "splitter-handle" +
          (horizontal ? " horizontal" : " vertical") +
          (this.isMoving ? " moving" : "")
        }
        style={{
          [horizontal ? "left" : "top"]:
            this.currentMousePosition - this.startMousePosition
        }}
      />
    );
  }
}
