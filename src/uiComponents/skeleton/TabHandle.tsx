import * as React from "react";
import { inject, observer } from "mobx-react";

@inject(stores => {
  const { tabParent } = stores as any;
  const { activeTabId, handleHandleClick } = tabParent;
  return {
    activeTabId,
    onHandleClick: handleHandleClick
  };
})
@observer
export class TabHandle extends React.Component<any> {
  public render() {
    return (
      <div
        onClick={event => this.props.onHandleClick(event, this.props.id)}
        className={
          "oui-tab-handle" +
          (this.props.id === this.props.activeTabId ? " active" : "")
        }
      >
        {this.props.name}
      </div>
    );
  }
}
