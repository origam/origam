import React from "react";
import S from "./TabbedPanel.module.css";
import {observer} from "mobx-react";

@observer
export class TabBody extends React.Component<{ isActive: boolean }> {
  render() {
    return (
      <div className={S.tabBody + (!this.props.isActive ? " hidden" : "")}>
        {this.props.children}
      </div>
    );
  }
}

@observer
export class TabHandle extends React.Component<{
  isActive: boolean;
  label: string;
  onClick: (event: any) => void;
}> {
  render() {
    return (
      <div
        className={S.tabHandle + (this.props.isActive ? ` active` : "")}
        onClick={this.props.onClick}
      >
        {this.props.label}
      </div>
    );
  }
}

@observer
export class TabbedPanel extends React.Component<{
  handles: React.ReactNode;
}> {
  render() {
    return (
      <div className={S.tabbedPanel}>
        <div className={S.tabHandles}>{this.props.handles}</div>
        {this.props.children}
      </div>
    );
  }
}
