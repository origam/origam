import React from "react";
import S from "./Splitter.module.scss";
import {action, computed, observable, runInAction} from "mobx";
import {observer, Observer} from "mobx-react";

import Measure, {ContentRect} from "react-measure";
import _ from "lodash";
import cx from "classnames";

@observer
class SplitterPanel extends React.Component<{
  type: "isHoriz" | "isVert";
  size: number;
  className?: string;
}> {
  refPanel = (elm: any) => (this.elmPanel = elm);
  elmPanel: HTMLDivElement | null = null;

  componentDidMount() {}

  @computed get style() {
    switch (this.props.type) {
      case "isHoriz":
        return { width: this.props.size };
      case "isVert":
        return { height: this.props.size };
    }
  }

  render() {
    return (
      <div
        ref={this.refPanel}
        className={cx(this.props.className || S.splitterPanel, this.props.type)}
        style={this.style}
      >
        {this.props.children}
      </div>
    );
  }
}

@observer
class SplitterDivider extends React.Component<{
  type: "isHoriz" | "isVert";
  className?: string;
  domRef: any;
  isDragging: boolean;
  relativeLoc: number;
  onMouseDown?(event: any): void;
}> {
  @computed get style() {
    if (!this.props.isDragging) return {};
    switch (this.props.type) {
      case "isVert":
        return { top: this.props.relativeLoc };
      case "isHoriz":
        return { left: this.props.relativeLoc };
    }
  }

  render() {
    return (
      <div
        ref={this.props.domRef}
        onMouseDown={this.props.onMouseDown}
        className={cx(
          this.props.className || S.splitterDivider,
          this.props.type,
          {
            isDragging: this.props.isDragging
          }
        )}
        style={this.style}
      >
        <div className="dividerLine" />
      </div>
    );
  }
}

@observer
export class Splitter extends React.Component<{
  type: "isHoriz" | "isVert";
  sizeOverrideFirstPanel?: number;
  id?: string;
  panels: Array<[any, number, React.ReactNode]>;
  onSizeChangeFinished?(
    panelId1: any,
    panelId2: any,
    panelSize1: number,
    panelSize2: number
  ): void;
  STYLE?: any;
}> {
  @observable containerWidth = 0;
  @observable containerHeight = 0;
  @observable isInitialized = false;
  @observable sizeMap: Map<any, number> = new Map();
  @observable dividerSizeMap: Map<any, number> = new Map();
  @observable isResizing = false;
  @observable mouseLocStart = 0;
  @observable dividerRelativeLoc = 0;
  draggingDividerId: any;

  constructor(props: any) {
    super(props);
    runInAction(() => {
      for (let i = 0; i < this.props.panels.length; i++) {
        const panel = this.props.panels[i];
        this.sizeMap.set(panel[0], panel[1]);
        if (i < this.props.panels.length - 1) {
          this.dividerSizeMap.set(panel[0], 0);
        }
      }
    });
  }

  @action.bound
  handleResize(contentRect: ContentRect) {
    const { bounds } = contentRect;
    if (bounds) {
      this.anounceContainerSize(bounds.width, bounds.height);
    }
  }

  @action.bound
  anounceContainerSize(width: number, height: number) {
    if (width > 0 && height > 0) {
      this.containerWidth = width;
      this.containerHeight = height;
      this.initSizes();
      this.isInitialized = true;
    }
  }

  @action.bound initSizesImm() {
    // TODO: Fix nested splitter initSize (first panel overriden by initial size by resising its container)

    //console.log(Array.from(this.dividerSizeMap.entries()));
    const containerSize =
      this.props.type === "isVert" ? this.containerHeight : this.containerWidth;
    const dividerSizeSum = Array.from(this.dividerSizeMap.values()).reduce(
      (a, b) => a + b,
      0
    );
    if (this.props.sizeOverrideFirstPanel === undefined) {
      //console.log("Divider size sum", dividerSizeSum);
      const sizeToDivide = containerSize - dividerSizeSum; // TODO: Include handle sizes?

      const initSizeSum = Array.from(this.sizeMap.values()).reduce(
        (a, b) => a + b,
        0
      );
      const sizeRatio = sizeToDivide / initSizeSum;
      for (let [key, value] of this.sizeMap.entries()) {
        this.sizeMap.set(key, value * sizeRatio);
        //console.log("Set", key, value * sizeRatio);
      }
    } else if (this.props.panels.length > 0) {
      const firstPanelSize = Math.max(this.props.sizeOverrideFirstPanel, 20);
      const sizeToDivide = containerSize - dividerSizeSum - firstPanelSize;
      const initSizeSum = this.props.panels
        .slice(1)
        .reduce((acc, panel) => acc + (this.sizeMap.get(panel[0]) || 0), 0);
      const sizeRatio = sizeToDivide / initSizeSum;

      for (let [key, value] of this.sizeMap.entries()) {
        if (key === this.props.panels[0][0]) continue;
        this.sizeMap.set(key, value * sizeRatio);
        console.log("Set", key, value * sizeRatio);
      }
      this.sizeMap.set(this.props.panels[0][0], firstPanelSize);
      for (let [key, value] of this.sizeMap.entries()) {
        console.log("Size", key, "=", value);
      }
    }
  }

  initSizes = _.debounce(this.initSizesImm, 10);

  @action.bound handleDividerMouseDown(event: any, handleId: any) {
    this.draggingDividerId = handleId;
    this.isResizing = true;
    this.dividerRelativeLoc = 0;
    this.mouseLocStart =
      this.props.type === "isVert" ? event.screenY : event.screenX;
    window.addEventListener("mouseup", this.handleWindowMouseUp);
    window.addEventListener("mousemove", this.handleWindowMouseMove);
  }

  @action.bound handleWindowMouseMove(event: any) {
    event.preventDefault();
    const dividerRelativeLoc =
      (this.props.type === "isVert" ? event.screenY : event.screenX) -
      this.mouseLocStart;
    const [size1, size2] = this.computeNewSizes(dividerRelativeLoc);
    if (size1 >= 20 && size2 >= 20) {
      this.dividerRelativeLoc = dividerRelativeLoc;
    }
  }

  get draggingIds() {
    const id1 = this.draggingDividerId;
    const id2 = this.props.panels[
      this.props.panels.findIndex(
        panel => panel[0] === this.draggingDividerId
      )! + 1
    ][0];
    return [id1, id2];
  }

  computeNewSizes(dividerRelativeLoc: number) {
    const [id1, id2] = this.draggingIds;
    return [
      this.sizeMap.get(id1)! + dividerRelativeLoc,
      this.sizeMap.get(id2)! - dividerRelativeLoc
    ];
  }

  get newSizes() {
    return this.computeNewSizes(this.dividerRelativeLoc);
  }

  @action.bound handleWindowMouseUp(event: any) {
    this.isResizing = false;
    window.removeEventListener("mouseup", this.handleWindowMouseUp);
    window.removeEventListener("mousemove", this.handleWindowMouseMove);
    const [id1, id2] = this.draggingIds;
    const [size1, size2] = this.newSizes;
    this.sizeMap.set(id1, size1);
    this.sizeMap.set(id2, size2);
    if (this.props.onSizeChangeFinished) {
      this.props.onSizeChangeFinished(id1, id2, size1, size2);
    }
  }

  @action.bound handleDividerResize(contentRect: ContentRect, dividerId: any) {
    //console.log("Divider resize", contentRect.bounds, dividerId);
    if (contentRect.bounds) {
      this.dividerSizeMap.set(
        dividerId,
        this.props.type === "isVert"
          ? contentRect.bounds.height
          : contentRect.bounds.width
      );
      this.initSizes();
    }
  }

  render() {
    const content: React.ReactNode[] = [];
    for (let i = 0; i < this.props.panels.length; i++) {
      const panel = this.props.panels[i];
      content.push(
        <SplitterPanel
          className={(this.props.STYLE || S).panel}
          type={this.props.type}
          size={this.sizeMap.get(panel[0])!}
          key={`S${panel[0]}`}
        >
          {panel[2]}
        </SplitterPanel>
      );
      if (i < this.props.panels.length - 1) {
        content.push(
          <Measure
            bounds={true}
            onResize={contentRect =>
              this.handleDividerResize(contentRect, panel[0])
            }
          >
            {({ measureRef }) => (
              <Observer>
                {() => (
                  <SplitterDivider
                    className={(this.props.STYLE || S).divider}
                    domRef={measureRef}
                    key={`D${panel[0]}`}
                    isDragging={
                      this.isResizing && this.draggingDividerId === panel[0]
                    }
                    relativeLoc={this.dividerRelativeLoc}
                    type={this.props.type}
                    onMouseDown={event =>
                      this.handleDividerMouseDown(event, panel[0])
                    }
                  />
                )}
              </Observer>
            )}
          </Measure>
        );
      }
    }
    return (
      <Measure bounds={true} onResize={this.handleResize}>
        {({ measureRef }) => (
          <Observer>
            {() => (
              <div
                ref={measureRef}
                className={cx((this.props.STYLE || S).root, this.props.type)}
              >
                {this.isInitialized && content}
              </div>
            )}
          </Observer>
        )}
      </Measure>
    );
  }
}
