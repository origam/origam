import * as React from "react";
import { observer, inject } from "mobx-react";
import { IDataViewType } from '../skeleton/types';


@inject("dataViewState")
@observer
export class GridToolbar extends React.Component<any> {
  public render() {
    return (
      <div
        className={"oui-grid-toolbar" + (this.props.isHidden ? " hidden" : "")}
      >
        <div className="toolbar-section">
          {/*<span className="toolbar-caption">
            {dataLoadingStrategyActions.isLoading && (
              <i className="fa fa-cog fa-spin" style={{ marginRight: 5 }} />
            )}{" "}
            {this.props.name}
            </span>*/}
        </div>
        <div className="toolbar-section">
          {this.props.isAddButton && (
            <button
              className="oui-toolbar-btn"

            >
              <i className="fa fa-plus-circle icon" aria-hidden="true" />
            </button>
          )}
          {this.props.isDeleteButton && (
            <button
              className="oui-toolbar-btn"
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
          >
            <i className="fa fa-caret-left icon" aria-hidden="true" />
          </button>
          <button
            className="oui-toolbar-btn"
          >
            <i className="fa fa-caret-right icon" aria-hidden="true" />
          </button>
          <button className="oui-toolbar-btn">
            <i className="fa fa-step-forward icon" aria-hidden="true" />
          </button>
        </div>
        <div className="toolbar-section">
          <span className="oui-toolbar-text">0</span>
        </div>
        <div className="toolbar-section">
          <button
            className="oui-toolbar-btn"
            onClick={() => this.props.dataViewState.setActiveView(IDataViewType.Table)}
          >
            <i className="fa fa-table icon" aria-hidden="true" />
          </button>
          <button
            className="oui-toolbar-btn"
            onClick={() => this.props.dataViewState.setActiveView(IDataViewType.Form)}
          >
            <i className="fa fa-list-alt icon" aria-hidden="true" />
          </button>
          <button
            className="oui-toolbar-btn"
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
