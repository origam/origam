import * as React from "react";
import { inject, observer } from "mobx-react";

@inject(({ tabParent: { activeTabId } }) => {
  return {
    activeTabId
  };
})
@observer
export class Box extends React.Component<any> {
  public render() {
    return (
      <div
        className="oui-box"
        style={{
          display: this.props.activeTabId !== this.props.id ? "none" : undefined
        }}
      >
        {this.props.children}
      </div>
    );
  }
}
