import * as React from "react";
import { observer, inject, Observer, Provider } from "mobx-react";
import { ITabPanelProps, ITabsProps } from "./types";
import { action, observable } from "mobx";

@observer
export class TabPanel extends React.Component<ITabPanelProps> {
  render() {
    return (
      <div
        className={
          "tabs-body" +
          (this.props.activePanelId !== this.props.id ? " hidden" : "")
        }
      >
        {this.props.children}
      </div>
    );
  }
}

@observer
export class TabbedPanel extends React.Component<ITabsProps> {
  @observable boxes: Map<string, string> = new Map();

  @action.bound
  registerBox(id: string, name: string) {
    this.boxes.set(id, name);
    if (!this.activePanelId) {
      this.activePanelId = id;
    }
  }

  @observable activePanelId: string | undefined;

  @action.bound handleHandleClick(event: any, id: string) {
    this.activePanelId = id;
  }

  render() {
    return (
      <div className="tabs">
        <Observer>
          {() => (
            <div className="tabs-handles">
              {Array.from(this.boxes.entries()).map(([id, name]) => (
                <div
                  className={
                    "tab-handle" + (this.activePanelId === id ? ` active` : "")
                  }
                  key={id}
                  onClick={(event: any) => this.handleHandleClick(event, id)}
                >
                  {name}
                </div>
              ))}
            </div>
          )}
        </Observer>
        <Provider tabbedPanel={this}>
          <>{this.props.children}</>
        </Provider>
      </div>
    );
  }
}
