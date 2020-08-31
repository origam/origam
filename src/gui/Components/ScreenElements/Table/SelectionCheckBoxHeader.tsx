import { observer } from "mobx-react";
import React from "react";
import { setAllSelectionStates } from "model/actions-tree/setAllSelectionStates";

@observer
export class SelectionCheckBoxHeader extends React.Component<{
  width: number;
  dataView: any;
}> {
  onClick(event: any) {
    this.props.dataView.selectAllCheckboxChecked = !this.props.dataView.selectAllCheckboxChecked;
    setAllSelectionStates(this.props.dataView, this.props.dataView.selectAllCheckboxChecked);
  }

  render() {
    return (
      <div style={{ minWidth: this.props.width + "px" }}>
        <input
          type={"checkBox"}
          onClick={(event) => this.onClick(event)}
          checked={this.props.dataView.selectAllCheckboxChecked}
        />
      </div>
    );
  }
}

