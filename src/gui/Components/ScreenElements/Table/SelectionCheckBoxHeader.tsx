import { observer } from "mobx-react";
import React from "react";
import { setAllSelectionStates } from "model/actions-tree/setAllSelectionStates";
import S from "./SelectionCheckboxHeader.module.scss";
import { action } from "mobx";

@observer
export class SelectionCheckBoxHeader extends React.Component<{
  width: number;
  dataView: any;
}> {
  @action.bound
  onClick(event: any) {
    this.props.dataView.selectAllCheckboxChecked = !this.props.dataView.selectAllCheckboxChecked;
    setAllSelectionStates(this.props.dataView, this.props.dataView.selectAllCheckboxChecked);
  }

  render() {
    const isChecked = this.props.dataView.selectAllCheckboxChecked;
    return (
      <div style={{ minWidth: this.props.width + "px" }} className={S.root} onClick={this.onClick}>
        {isChecked ? <i className="far fa-check-square" /> : <i className="far fa-square" />}
      </div>
    );
  }
}
