import { action, observable } from "mobx";
import { observer } from "mobx-react";
import * as React from "react";

@observer
class SplitterHandle extends React.Component<{
  onMouseDown: (event: any) => void;
  vertical: boolean;
}> {
  public render() {
    return (
      <div
        onMouseDown={this.props.onMouseDown}
        className={
          this.props.vertical ? "splitter-handle-horiz" : "splitter-handle-vert"
        }
      />
    );
  }
}

@observer
class SplitterCell extends React.Component<{
  refDiv: (elm: HTMLDivElement) => void;
  isLast: boolean;
  width: number;
  height: number;
  vertical: boolean;
}> {
  public render() {
    return (
      <div
        ref={this.props.refDiv}
        style={{
          width:
            this.props.isLast || this.props.width === undefined
              ? undefined
              : this.props.width,
          height:
            this.props.isLast || this.props.height === undefined
              ? undefined
              : this.props.height,
          flex:
            this.props.isLast ||
            (this.props.width === undefined && !this.props.vertical) ||
            (this.props.height === undefined && this.props.vertical)
              ? "1 1 0"
              : undefined
        }}
        className={
          this.props.vertical ? "splitter-cell-vert" : "splitter-cell-horiz"
        }
      >
        {this.props.children}
      </div>
    );
  }
}

interface ISplitterInternalProps {
  width: number;
  height: number;
  vertical: boolean;
}

@observer
class SplitterInternal extends React.Component<ISplitterInternalProps> {
  @observable
  public sizesHoriz = new Map();
  @observable
  public sizesVert = new Map();

  @observable
  public isDragging = false;

  public draggingCellIndex: number;

  public mouseMovementBaseX: number;
  public mouseMovementBaseY: number;
  public handleMovementBaseXLeft: number;
  public handleMovementBaseXRight: number;
  public handleMovementBaseYTop: number;
  public handleMovementBaseYBottom: number;

  private divRefs: Map<number, HTMLDivElement> = new Map();

  public componentDidUpdate(prevProps: ISplitterInternalProps) {
    /*
    const widthRatio = this.props.width / prevProps.width;
    const heightRatio = this.props.height / prevProps.height;
    this.recalculateSizes(widthRatio, heightRatio);*/
  }

  @action.bound
  public recalculateSizes(widthRatio: number, heightRatio: number) {
    /*if(isFinite(widthRatio)) {
      const newEntries = [];
      for(let [k, v] of this.sizesHoriz) {
        newEntries.push([k, v * widthRatio]);
      }
      this.sizesHoriz = new Map(newEntries);
    }
    if(isFinite(heightRatio)) {
      const newEntries = [];
      for(let [k, v] of this.sizesVert) {
        newEntries.push([k, v * heightRatio]);
      }
      this.sizesVert = new Map(newEntries);
    }  */
  }

  @action.bound
  public handleMouseMove(event: any) {
    if (this.isDragging) {
      const newSizeLeft =
        this.handleMovementBaseXLeft + event.clientX - this.mouseMovementBaseX;
      const newSizeRight =
        this.handleMovementBaseXRight - event.clientX + this.mouseMovementBaseX;
      const newSizeTop =
        this.handleMovementBaseYTop + event.clientY - this.mouseMovementBaseY;
      const newSizeBottom =
        this.handleMovementBaseYBottom -
        event.clientY +
        this.mouseMovementBaseY;
      if (newSizeLeft >= 100 && newSizeRight >= 100) {
        this.sizesHoriz.set(this.draggingCellIndex, newSizeLeft);
        this.sizesHoriz.set(this.draggingCellIndex + 1, newSizeRight);
      }
      if (newSizeTop >= 100 && newSizeBottom >= 100) {
        this.sizesVert.set(this.draggingCellIndex, newSizeTop);
        this.sizesVert.set(this.draggingCellIndex + 1, newSizeBottom);
      }
    }
  }

  @action.bound
  public handleHandleMouseDown(event: any, cellIndex: number) {
    event.preventDefault();

    if (this.props.vertical) {
      if (this.sizesVert.get(cellIndex) === undefined) {
        React.Children.forEach(this.props.children, (child, index) => {
          const height = parseFloat(
            window.getComputedStyle(this.divRefs.get(index)!).height!
          );
          this.sizesVert.set(index, height);
        });
      }
    } else {
      if (this.sizesHoriz.get(cellIndex) === undefined) {
        React.Children.forEach(this.props.children, (child, index) => {
          const width = parseFloat(
            window.getComputedStyle(this.divRefs.get(index)!).width!
          );
          this.sizesHoriz.set(index, width);
        });
      }
    }

    this.draggingCellIndex = cellIndex;
    this.mouseMovementBaseX = event.clientX;
    this.mouseMovementBaseY = event.clientY;
    this.handleMovementBaseXLeft = this.sizesHoriz.get(cellIndex);
    this.handleMovementBaseXRight = this.sizesHoriz.get(cellIndex + 1);
    this.handleMovementBaseYTop = this.sizesVert.get(cellIndex);
    this.handleMovementBaseYBottom = this.sizesVert.get(cellIndex + 1);
    this.isDragging = true;
    window.addEventListener("mousemove", this.handleMouseMove);
    window.addEventListener("mouseup", this.handleWindowMouseUp);
  }

  @action.bound
  public handleWindowMouseUp(event: any) {
    this.stopDragging();
  }

  @action.bound
  public handleWindowMouseLeave(event: any) {
    this.stopDragging();
  }

  @action.bound
  public stopDragging() {
    this.isDragging = false;
    window.removeEventListener("mousemove", this.handleMouseMove);
    window.removeEventListener("mouseup", this.handleWindowMouseUp);
    // window.document.body.removeEventListener("mouseleave", this.handleWindowMouseLeave);
  }

  public getWidth(i: number) {
    return this.sizesHoriz.get(i);
  }

  public getHeight(i: number) {
    return this.sizesVert.get(i);
  }

  @action.bound
  public refDiv(elm: HTMLDivElement, i: number) {
    this.divRefs.set(i, elm);
    return;
    if (elm && (!this.sizesHoriz.has(i) || !this.sizesVert.has(i))) {
      const width = parseFloat(window.getComputedStyle(elm).width!);
      const height = parseFloat(window.getComputedStyle(elm).height!);
      this.sizesHoriz.set(i, width);
      this.sizesVert.set(i, height);
    }
  }

  public render() {
    const { width, height } = this.props;
    const children = React.Children.map(this.props.children, o => o) || [];
    const resultingChildren = [];
    for (let i = 0; i < children.length; i++) {
      resultingChildren.push(
        <SplitterCell
          refDiv={elm => this.refDiv(elm, i)}
          key={`c${i}`}
          vertical={this.props.vertical}
          width={!this.props.vertical && this.getWidth(i)}
          height={this.props.vertical && this.getHeight(i)}
          isLast={i === children.length - 1}
        >
          {children[i]}
        </SplitterCell>
      );
      if (i < children.length - 1) {
        resultingChildren.push(
          <SplitterHandle
            vertical={this.props.vertical}
            key={`h${i}`}
            onMouseDown={event => this.handleHandleMouseDown(event, i)}
          />
        );
      }
    }

    return (
      <div
        style={{ width, height }}
        className={
          this.props.vertical
            ? "splitter-container-vert"
            : "splitter-container-horiz"
        }
      >
        {resultingChildren}
      </div>
    );
  }
}

@observer
export class Splitter extends React.Component<{
  width: number;
  height: number;
  vertical: boolean;
}> {
  public render() {
    return (
      <div className="splitter-container-topmost">
        {/*<AutoSizer>
          {({ width, height }) => (*/}
        <SplitterInternal /*width={width} height={height}*/ {...this.props} />
        {/*})}
        </AutoSizer>*/}
      </div>
    );
  }
}
