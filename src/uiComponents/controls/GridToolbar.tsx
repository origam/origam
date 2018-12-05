import * as React from "react";
import { observer, inject } from "mobx-react";
import { IGridInteractionActions, GridViewType } from "src/Grid/types";
import { IGridToolbarView, IGridPanelBacking } from "../../GridPanel/types";

@inject("gridPaneBacking")
@observer
export class GridToolbar extends React.Component<any> {
  public render() {
    const { gridPaneBacking } = this.props;
    const {
      setActiveView
    } = gridPaneBacking.gridInteractionActions as IGridInteractionActions;
    const { dataLoadingStrategyActions } = gridPaneBacking as IGridPanelBacking;
    const gridToolbarView = gridPaneBacking.gridToolbarView as IGridToolbarView;
    return (
      <div
        className={"oui-grid-toolbar" + (this.props.isHidden ? " hidden" : "")}
      >
        <div className="toolbar-section">
          <span className="toolbar-caption">
            {dataLoadingStrategyActions.isLoading && (
              <i className="fa fa-cog fa-spin" style={{ marginRight: 5 }} />
            )}{" "}
            {this.props.name}
          </span>
        </div>
        <div className="toolbar-section">
          {this.props.isAddButton && (
            <button
              className="oui-toolbar-btn"
              onClick={gridToolbarView.handleAddRecordClick}
            >
              <i className="fa fa-plus-circle icon" aria-hidden="true" />
            </button>
          )}
          {this.props.isDeleteButton && (
            <button
              className="oui-toolbar-btn"
              onClick={gridToolbarView.handleRemoveRecordClick}
            >
              <i className="fa fa-minus-circle icon" aria-hidden="true" />
            </button>
          )}
          {this.props.isCopyButton && (
            <button className="oui-toolbar-btn">
              <i className="fa fa-copy icon" aria-hidden="true" />
            </button>
          )}
        </div>
        <div className="toolbar-section pusher" />
        <div className="toolbar-section">
          <button className="oui-toolbar-btn">
            <i className="fa fa-step-backward icon" aria-hidden="true" />
          </button>
          <button
            className="oui-toolbar-btn"
            onClick={gridToolbarView.handlePrevRecordClick}
          >
            <i className="fa fa-caret-left icon" aria-hidden="true" />
          </button>
          <button
            className="oui-toolbar-btn"
            onClick={gridToolbarView.handleNextRecordClick}
          >
            <i className="fa fa-caret-right icon" aria-hidden="true" />
          </button>
          <button className="oui-toolbar-btn">
            <i className="fa fa-step-forward icon" aria-hidden="true" />
          </button>
        </div>
        <div className="toolbar-section">
          <span className="oui-toolbar-text">1/6</span>
        </div>
        <div className="toolbar-section">
          <button
            className="oui-toolbar-btn"
            onClick={() => setActiveView(GridViewType.Grid)}
          >
            <i className="fa fa-table icon" aria-hidden="true" />
          </button>
          <button
            className="oui-toolbar-btn"
            onClick={() => setActiveView(GridViewType.Form)}
          >
            <i className="fa fa-list-alt icon" aria-hidden="true" />
          </button>
          <button
            className="oui-toolbar-btn"
            onClick={() => setActiveView(GridViewType.Map)}
          >
            <i className="fa fa-map-o icon" aria-hidden="true" />
          </button>
        </div>
        <div className="toolbar-section">
          <button className="oui-toolbar-btn">
            <i className="fa fa-filter icon" aria-hidden="true" />
          </button>
          <button className="oui-toolbar-btn">
            <i className="fa fa-caret-down icon" aria-hidden="true" />
          </button>
        </div>
        <div className="toolbar-section">
          <button className="oui-toolbar-btn">
            <i className="fa fa-cog icon" aria-hidden="true" />
            <i className="fa fa-caret-down icon" aria-hidden="true" />
          </button>
        </div>
      </div>
    );
  }
}
