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
import S from "gui/Components/Splitter/Splitter.module.scss";
import { action, computed, observable, runInAction } from "mobx";
import { observer, Observer } from "mobx-react";

import Measure, { ContentRect } from "react-measure";
import _ from "lodash";
import cx from "classnames";
import { IPanelData } from "gui/Components/Splitter/IPanelData";

@observer
class SplitterPanel extends React.Component<{
  type: "isHoriz" | "isVert";
  size: number;
  className?: string;
}> {
  refPanel = (elm: any) => (this.elmPanel = elm);
  elmPanel: HTMLDivElement | null = null;

  componentDidMount() {
  }

  @computed get style() {
    switch (this.props.type) {
      case "isHoriz":
        return {width: this.props.size};
      case "isVert":
        return {height: this.props.size};
    }
    return undefined;
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
        return {top: this.props.relativeLoc};
      case "isHoriz":
        return {left: this.props.relativeLoc};
    }
    return undefined;
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
        <div className="dividerLine"/>
      </div>
    );
  }
}

@observer
export class Splitter extends React.Component<{
  type: "isHoriz" | "isVert";
  sizeOverrideFirstPanel?: number;
  dontPrintLeftPane?: boolean;
  id?: string;
  panels: Array<IPanelData>;
  onSizeChangeFinished?(
    panelId1: any,
    panelId2: any,
    panel1Ratio: number,
    panel2Ratio: number
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
        this.sizeMap.set(panel.id, panel.positionRatio);
        if (i < this.props.panels.length - 1) {
          this.dividerSizeMap.set(panel.id, 0);
        }
      }
    });
  }

  @action.bound
  handleResize(contentRect: ContentRect) {
    const {bounds} = contentRect;
    if (bounds) {
      this.announceContainerSize(bounds.width, bounds.height);
    }
  }

  @action.bound
  announceContainerSize(width: number, height: number) {
    if (width > 0 && height > 0 && this.differentSizeRequested(width, height)) {
      this.containerWidth = width;
      this.containerHeight = height;
      this.initSizes();
      this.isInitialized = true;
    }
  }

  differentSizeRequested(newWidth: number, newHeight: number) {
    return Math.abs(newHeight - this.containerHeight) > 0.001 ||
      Math.abs(newWidth - this.containerWidth) > 0.001
  }

  @action.bound initSizesImm() {
    // TODO: Fix nested splitter initSize (first panel overridden by initial size by resizing its container)

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
        .reduce((acc, panel) => acc + (this.sizeMap.get(panel.id) || 0), 0);
      const sizeRatio = sizeToDivide / initSizeSum;

      for (let [key, value] of this.sizeMap.entries()) {
        if (key === this.props.panels[0].id) continue;
        this.sizeMap.set(key, value * sizeRatio);
      }
      this.sizeMap.set(this.props.panels[0].id, firstPanelSize);
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
      panel => panel.id === this.draggingDividerId
    )! + 1
      ].id;
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
      if (this.props.type === "isHoriz") {
        this.props.onSizeChangeFinished(id1, id2, size1 / this.containerWidth, size2);
      } else {
        this.props.onSizeChangeFinished(id1, id2, size1 / this.containerHeight, size2);
      }
    }
  }

  @action.bound handleDividerResize(contentRect: ContentRect, dividerId: any) {
    //console.log("Divider resize", contentRect.bounds, dividerId);
    if (contentRect.bounds) {
      const value = this.props.type === "isVert"
        ? contentRect.bounds.height
        : contentRect.bounds.width
      if (this.dividerSizeMap.get(dividerId) !== value) {
        this.dividerSizeMap.set(dividerId, value);
        this.initSizes();
      }
    }
  }

  render() {
    const content: React.ReactNode[] = [];
    for (let i = 0; i < this.props.panels.length; i++) {
      const panel = this.props.panels[i];
      content.push(
        <SplitterPanel
          className={(this.props.STYLE || S).panel + (i === 0 && this.props.dontPrintLeftPane ? " noPrint" : "")}
          type={this.props.type}
          size={this.sizeMap.get(panel.id)!}
          key={`S${panel.id}`}
        >
          {panel.element}
        </SplitterPanel>
      );
      if (i < this.props.panels.length - 1) {
        content.push(
          <Measure
            key={i} // Assuming panel structure will not change.
            bounds={true}
            onResize={contentRect =>
              this.handleDividerResize(contentRect, panel.id)
            }
          >
            {({measureRef}) => (
              <Observer>
                {() => (
                  <SplitterDivider
                    className={(this.props.STYLE || S).divider}
                    domRef={measureRef}
                    key={`D${panel.id}`}
                    isDragging={
                      this.isResizing && this.draggingDividerId === panel.id
                    }
                    relativeLoc={this.dividerRelativeLoc}
                    type={this.props.type}
                    onMouseDown={event =>
                      this.handleDividerMouseDown(event, panel.id)
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
        {({measureRef}) => (
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
