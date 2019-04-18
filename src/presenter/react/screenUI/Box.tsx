import React from "react";
import { observer, inject, Provider } from "mobx-react";
import { TabbedPanel } from "./TabbedPanel";
import { computed } from "mobx";
import style from './ScreenElements.module.css';

@inject(({ tabbedPanel }) => {
  return { tabbedPanel };
})
@observer
export class Box extends React.Component<{
  Id: string;
  Name?: string;
  tabbedPanel?: TabbedPanel;
}> {
  componentDidMount() {
    if (this.props.tabbedPanel) {
      this.props.tabbedPanel.registerBox(this.props.Id, this.props.Name!);
    }
  }

  @computed get isHidden() {
    if (this.props.tabbedPanel) {
      return this.props.tabbedPanel.activePanelId !== this.props.Id;
    } else {
      return false;
    }
  }

  render() {
    return (
      <Provider tabbedPanel={null}>
        <div className={style.Box + (this.isHidden ? ` ${style.hidden}` : "")}>
          {this.props.children}
        </div>
      </Provider>
    );
  }
}
