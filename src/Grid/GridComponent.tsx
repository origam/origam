import * as React from "react";
import { observer } from "mobx-react";
import { IGridProps } from "./types";

export const GridComponent = observer(
  class GridComponentOriginal extends React.Component<IGridProps> {
    public componentDidMount() {
      this.view.componentDidMount(this.props, this);
    }

    public componentDidUpdate(prevProps: IGridProps) {
      this.view.componentDidUpdate(prevProps, this.props, this);
    }

    public componentWillUnmount() {
      this.view.componentWillUnmount();
    }

    private get view() {
      return this.props.view;
    }

    private get canvasProps() {
      return this.view.canvasProps;
    }

    public render() {
      const {
        width,
        height,

        overlayElements
      } = this.props;

      const {
        handleGridScroll,
        handleGridKeyDown,
        handleGridClick,
        refRoot,
        refScroller,
        refCanvas,
      } = this.view;

      const { contentWidth, contentHeight } = this.view;

      const { canvasProps } = this;

      return (
        <div
          className="grid-view-container"
          style={{ width, height }}
          onKeyDown={handleGridKeyDown}
        >
          <div
            className="grid-view-root"
            ref={refRoot}
            tabIndex={-1}
            onClick={handleGridClick}
          >
            <canvas {...canvasProps} ref={refCanvas} />
            <div
              className="grid-view-scroller"
              style={{ width, height }}
              onScroll={handleGridScroll}
              ref={refScroller}
            >
              <div
                style={{
                  width: contentWidth + 100,
                  height: contentHeight + 100
                }}
              />
            </div>
            {overlayElements}
          </div>
          <div className="grid-view-editor-portal" /*ref={refEditorPortal}*/ />
        </div>
      );
    }
  }
);
