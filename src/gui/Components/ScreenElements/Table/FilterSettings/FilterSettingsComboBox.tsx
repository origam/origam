import React from "react";

import S from "./FilterSettingsComboBox.module.scss";
import { observer } from "mobx-react";
import { observable, action } from "mobx";

@observer
export class FilterSettingsComboBox extends React.Component<{
  trigger: React.ReactNode;
}> {
  @observable isDroppedDown = false;

  refDropdown = React.createRef<HTMLDivElement>();

  @action.bound setDroppedDown(state: boolean) {
    if (state && !this.isDroppedDown) {
      this.isDroppedDown = true;
      window.addEventListener("mousedown", this.handleWindowMouseDown);
    } else if (!state && this.isDroppedDown) {
      this.isDroppedDown = false;
      window.removeEventListener("mousedown", this.handleWindowMouseDown);
    }
  }

  @action.bound handleTriggerClick(event: any) {
    if (this.isDroppedDown) {
      this.setDroppedDown(false);
    } else {
      this.setDroppedDown(true);
    }
  }

  @action.bound handleWindowMouseDown(event: any) {
    if (
      !this.refDropdown.current ||
      !this.refDropdown.current.contains(event.target)
    ) {
      this.setDroppedDown(false);
    }
  }

  @action.bound handleDropdownClick(event: any) {
    this.setDroppedDown(false);
  }

  render() {
    return (
      <div className={S.container} ref={this.refDropdown}>
        <div className={S.trigger} onClick={this.handleTriggerClick}>
          {this.props.trigger}
          <div className={S.dropdownSymbol}>
            <i className="fas fa-caret-down" />
          </div>
        </div>
        {this.isDroppedDown && (
          <div className={S.dropdown} onClick={this.handleDropdownClick}>
            {this.props.children}
          </div>
        )}
      </div>
    );
  }
}

export const FilterSettingsComboBoxItem: React.FC<{
  onClick?: (event: any) => void;
}> = props => (
  <div className={S.dropdownItem} onClick={props.onClick}>
    {props.children}
  </div>
);
