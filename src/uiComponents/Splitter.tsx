import * as React from "react";

import { AutoSizer } from "react-virtualized";
import Measure from "react-measure";
import { observer, Provider, inject } from "mobx-react";
import { observable, action } from "mobx";

interface ISplitterProps {
  splitterModel?: {
    containerSize: number;
  };
  isVertical: boolean;
}

interface ISplitterModel {
  containerSize: number;
  isVertical: boolean;
  sizes: Map<any, number>;
}

interface IRectSize {
  width: number;
  height: number;
}

@observer
export class Splitter extends React.Component<ISplitterProps> {
  constructor(props: ISplitterProps) {
    super(props);
    this.splitterModel.isVertical = props.isVertical;
  }

  @observable public sizes = new Map<any, number>();
  public splitterModel: ISplitterModel = {
    containerSize: 0,
    isVertical: false,
    sizes: this.sizes
  };

  @action.bound
  public handleContainerResize({ client }: { client?: IRectSize }) {
    if (!client) {
      return;
    }
    const { isVertical, containerSize } = this.splitterModel;
    if ((isVertical ? client.height : client.width) > 0) {
      if (containerSize > (this.sizes.size + 1) * 40) {
        for (const key of this.sizes.keys()) {
          this.sizes.set(
            key,
            Math.max(
              40,
              ((this.sizes.get(key) || 0) *
                (isVertical ? client.height : client.width)) /
                containerSize
            )
          );
        }
      }
      if (isVertical) {
        this.splitterModel.containerSize = client.height;
      } else {
        this.splitterModel.containerSize = client.width;
      }
    }
  }

  public render() {
    const nodes: React.ReactNode[] = [];
    const { isVertical, containerSize } = this.splitterModel;
    const children = React.Children.map(this.props.children, o => o);
    for (let i = 0; i < children.length; i++) {
      if (i < children.length - 1) {
        nodes.push(children[i]);
        nodes.push(
          <SplitHandle
            key={(children[i] as any).props.splitterId}
            prevPanelId={(children[i] as any).props.splitterId}
            nextPanelId={(children[i + 1] as any).props.splitterId}
          />
        );
      } else {
        nodes.push(React.cloneElement(children[i] as any, { isLast: true }));
      }
    }
    return (
      <Measure onResize={this.handleContainerResize} client={true}>
        {({ measureRef }) => (
          <div ref={measureRef} className={isVertical ? "vsplit" : "hsplit"}>
            <Provider splitterModel={this.splitterModel}>
              <>{nodes}</>
            </Provider>
          </div>
        )}
      </Measure>
    );
  }
}

interface ISplitPanelProps {
  initialSize?: number;
  splitterId: any;
  splitterModel?: ISplitterModel;
  isLast?: boolean;
}

@inject("splitterModel")
@observer
export class SplitPanel extends React.Component<ISplitPanelProps> {
  constructor(props: ISplitPanelProps) {
    super(props);
    if (props.initialSize && !props.isLast) {
      props.splitterModel!.sizes.set(props.splitterId, props.initialSize);
    }
  }

  public render() {
    const { isVertical, sizes } = this.props.splitterModel!;
    return (
      <div
        className={"split-panel"}
        style={{
          height: isVertical ? sizes.get(this.props.splitterId) : undefined,
          width: !isVertical ? sizes.get(this.props.splitterId) : undefined
        }}
      >
        {this.props.children}
      </div>
    );
  }
}

interface ISplitHandleProps {
  prevPanelId: any;
  nextPanelId: any;
  splitterModel?: ISplitterModel;
}

@inject("splitterModel")
@observer
class SplitHandle extends React.Component<ISplitHandleProps> {
  public startMousePosition: number;
  public originalSizePrev: number;
  public originalSizeNext: number;
  public isMoving: boolean;

  @action.bound
  public handleMouseDown(event: any) {
    const { isVertical, sizes } = this.props.splitterModel!;
    this.startMousePosition = isVertical ? event.screenY : event.screenX;
    this.originalSizePrev = sizes.get(this.props.prevPanelId)!;
    this.originalSizeNext = sizes.get(this.props.nextPanelId)!;
    this.isMoving = true;
    window.addEventListener("mouseup", this.handleWindowMouseUp);
    window.addEventListener("mousemove", this.handleWindowMouseMove);
  }

  @action.bound
  public handleWindowMouseMove(event: any) {
    if (!this.isMoving) {
      return;
    }
    event.stopPropagation();
    event.preventDefault();

    const { isVertical, sizes, containerSize } = this.props.splitterModel!;
    const { prevPanelId, nextPanelId } = this.props;

    const oldSizePrev = sizes.get(prevPanelId);
    const oldSizeNext = sizes.get(nextPanelId);
    const newSizePrev =
      this.originalSizePrev +
      (isVertical ? event.screenY : event.screenX) -
      this.startMousePosition;
    const newSizeNext =
      this.originalSizeNext -
      (isVertical ? event.screenY : event.screenX) +
      this.startMousePosition;

    if (newSizePrev < 40 || newSizeNext < 40) {
      return;
    }

    sizes.set(prevPanelId, newSizePrev);
    if (newSizeNext) {
      sizes.set(nextPanelId, newSizeNext);
    }

    let sizeSum = 0;
    for (const size of sizes.values()) {
      sizeSum = sizeSum + size;
    }
    console.log(oldSizeNext, newSizeNext, isVertical);

    if (sizeSum > containerSize - 40 && oldSizePrev) {
      sizes.set(prevPanelId, oldSizePrev);
      if (newSizeNext && oldSizeNext) {
        sizes.set(nextPanelId, oldSizeNext);
      }
    }
  }

  @action.bound
  public handleWindowMouseUp(event: any) {
    this.isMoving = false;
    window.removeEventListener("mouseup", this.handleWindowMouseUp);
    window.removeEventListener("mousemove", this.handleWindowMouseMove);
  }

  public render() {
    const { isVertical } = this.props.splitterModel!;
    return (
      <div
        onMouseDown={this.handleMouseDown}
        className={isVertical ? "vsplitdiv" : "hsplitdiv"}
      />
    );
  }
}
