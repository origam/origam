import * as React from "react";
import { observer, Provider } from "mobx-react";
import { action, observable } from "mobx";

@observer
export class Tab extends React.Component<any> {
  @observable
  public activeTabId: string = this.props.firstTabId;

  @action.bound
  public handleHandleClick(event: any, tabId: string) {
    this.activeTabId = tabId;
    console.log("Handle click", this.activeTabId);
  }

  public render() {
    return (
      <Provider tabParent={this}>
        <div className="oui-tab">
          <div className="oui-tab-handles">{this.props.handles}</div>
          <div className="oui-tab-panels">{this.props.children}</div>
        </div>
      </Provider>
    );
  }
}
