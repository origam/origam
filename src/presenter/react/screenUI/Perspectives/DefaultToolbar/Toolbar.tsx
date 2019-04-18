import * as React from "react";
import { observer } from "mobx-react";
import { IToolbarButtonState, IToolbar } from "../../../../view/Perspectives/types";
import { IViewType } from "../../../../../DataView/types/IViewType";


@observer
export class ToolbarButton extends React.Component<{
  controller: IToolbarButtonState;
}> {
  render() {
    const { controller } = this.props;
    return controller.isVisible ? (
      <button
        className={
          (controller.isActive ? " active" : "") +
          (controller.isEnabled ? "" : " disabled")
        }
        onClick={controller.onClick}
      >
        {this.props.children}
      </button>
    ) : null;
  }
}

@observer
export class Toolbar extends React.Component<{ controller: IToolbar }> {
  render() {
    const { controller } = this.props;
    return (
      <div className="data-view-toolbar">
        <div className="section">
          {controller.isLoading && (
            <div className="loading">
              <i className="fas fa-sync-alt fa-spin" />
            </div>
          )}
          {controller.isError && (
            <i className="fas fa-exclamation-circle red" />
          )}
          <div className="label">{controller.label}</div>
          {controller.isFiltered && <i className="fas fa-filter red" />}
        </div>
        <div className="section">
          <ToolbarButton controller={controller.btnMoveUp}>
            <i className="fas fa-chevron-circle-up" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton controller={controller.btnMoveDown}>
            <i className="fas fa-chevron-circle-down" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className="section">
          <ToolbarButton controller={controller.btnAdd}>
            <i className="fas fa-plus-circle" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton controller={controller.btnDelete}>
            <i className="fas fa-minus-circle" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton controller={controller.btnCopy}>
            <i className="fas fa-copy" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className="section pusher" />
        <div className="section">
          <ToolbarButton controller={controller.btnFirst}>
            <i className="fas fa-step-backward" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton controller={controller.btnPrev}>
            <i className="fas fa-caret-left" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton controller={controller.btnNext}>
            <i className="fas fa-caret-right" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton controller={controller.btnLast}>
            <i className="fas fa-step-forward" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className="section">
          <span>
            {controller.recordNo} / {controller.recordTotal}
          </span>
        </div>
        <div className="section">
          {controller.btnsViews.map(btnView => {
            return (
              <ToolbarButton key={btnView.type} controller={btnView.btn}>
                {(() => {
                  switch (btnView.type) {
                    case IViewType.Table:
                      return <i className="fa fa-table" aria-hidden="true" />;
                    case IViewType.Form:
                      return (
                        <i className="fas fa-list-alt" aria-hidden="true" />
                      );
                    /*case IViewType.Map:
                      return <i className="fas fa-map" aria-hidden="true" />;*/
                  }
                })()}
              </ToolbarButton>
            );
          })}
        </div>
        <div className="section">
          <ToolbarButton controller={controller.btnFilter}>
            <i className="fas fa-filter" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton controller={controller.btnFilterDropdown}>
            <i className="fas fa-caret-down" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className="section">
          <ToolbarButton controller={controller.btnSettings}>
            <i className="fas fa-cog" aria-hidden="true" />
            <i className="fas fa-caret-down" aria-hidden="true" />
          </ToolbarButton>
        </div>
      </div>
    );
  }
}
