import React from "react";
import { observer } from "mobx-react";
import S from "./DataViewToolbar.module.css";

@observer
export class Toolbar extends React.Component<{}> {
  render() {
    return (
      <div className={S.dataViewToolbar}>
        <div className={S.section}>
          {false && (
            <div className={S.loading}>
              <i className="fas fa-sync-alt fa-spin" />
            </div>
          )}
          {false && <i className="fas fa-exclamation-circle red" />}
          <div className={S.label}>{"Data view label"}</div>
          {false && <i className="fas fa-filter red" />}
        </div>
        <div className={S.section}>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-chevron-circle-up" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-chevron-circle-down" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className="section">
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-plus-circle" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-minus-circle" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-copy" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className={S.section + " " + S.pusher} />
        <div className={S.section}>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-step-backward" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-caret-left" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-caret-right" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-step-forward" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className={S.section}>
          <span>
            {0} / {0}
          </span>
        </div>
        <div className={S.section}>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fa fa-table" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-list-alt" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className={S.section}>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-filter" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-caret-down" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className={S.section}>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-cog" aria-hidden="true" />
            <i className="fas fa-caret-down" aria-hidden="true" />
          </ToolbarButton>
        </div>
      </div>
    );
  }
}

@observer
export class ToolbarButton extends React.Component<{
  isActive: boolean;
  isEnabled: boolean;
  isVisible: boolean;
  onClick?: (event: any) => void;
}> {
  render() {
    const { isActive, isEnabled, isVisible, onClick } = this.props;
    return isVisible ? (
      <button
        className={S.toolbarBtn + (isActive ? " active" : "") + (isEnabled ? "" : " disabled")}
        onClick={onClick}
      >
        {this.props.children}
      </button>
    ) : null;
  }
}
