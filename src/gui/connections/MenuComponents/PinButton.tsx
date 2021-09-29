import {observer} from "mobx-react";
import React from "react";
import CS from "gui/connections/MenuComponents/HeaderButton.module.scss";
import {T} from "utils/translation";

@observer
export class PinButton extends React.Component<{
  isPinned: boolean
  isVisible: boolean;
  onClick: () => void;
}> {

  getClass() {
    let className = "fas fa-thumbtack " + CS.headerIcon;
    if (!this.props.isVisible) {
      className += " " + CS.headerIconHidden;
    }
    if (this.props.isPinned) {
      className += " " + CS.headerIconActive
    }
    return className;
  }

  render() {
    return (
      <i
        title={T("Pin Favourites to the Top", "pin_favorites")}
        className={this.getClass()}
        onClick={() => this.props.onClick()}
      />
    )
  }
}