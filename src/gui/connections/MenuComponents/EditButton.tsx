import {observer} from "mobx-react";
import React from "react";
import S from "gui/connections/MenuComponents/EditButton.module.scss";
import CS from "gui/connections/MenuComponents/HeaderButton.module.scss";

@observer
export class EditButton extends React.Component<{
  isEnabled: boolean;
  isVisible: boolean;
  onClick: () => void;
  tooltip: string;
}> {

  getClass() {
    let className = `fas  fa-edit ${CS.headerIcon} ${S.editIcon}`;
    if (!this.props.isVisible && !this.props.isEnabled) {
      className += " " +CS.headerIconHidden;
    }
    if (this.props.isEnabled) {
      className += " " + CS.headerIconActive
    }
    return className;
  }

  render() {
    return (
      <i
        title={this.props.tooltip}
        className={this.getClass()}
        onClick={() => this.props.onClick()}
      />
    )
  }
}