import React from "react";

import S from "./FilterSettingsComboBox.module.scss";
import { Observer, observer } from "mobx-react";
import { action, observable } from "mobx";
import { createPortal } from "react-dom";
import Measure from "react-measure";
import _ from "lodash";

@observer
export class FilterSettingsComboBox extends React.Component<{
  trigger: React.ReactNode;
}> {
  @observable isDroppedDown = false;

  refDropdown = React.createRef<HTMLDivElement>();

  @action.bound setDroppedDown(state: boolean) {
    if (state && !this.isDroppedDown) {
      this.measureImm();
      this.isDroppedDown = true;
      window.addEventListener("click", this.handleWindowClick);
      window.addEventListener("scroll", this.handleWindowScroll, true);
    } else if (!state && this.isDroppedDown) {
      this.isDroppedDown = false;
      window.removeEventListener("click", this.handleWindowClick);
      window.removeEventListener("scroll", this.handleWindowScroll, true);
    }
  }

  @action.bound handleTriggerClick(event: any) {
    if (this.isDroppedDown) {
      this.setDroppedDown(false);
    } else {
      this.setDroppedDown(true);
    }
  }

  @action.bound handleWindowClick(event: any) {
    if (!this.refDropdown.current || !this.refDropdown.current.contains(event.target)) {
      this.setDroppedDown(false);
    }
  }

  @action.bound handleWindowScroll(event: any) {
    this.setDroppedDown(false);
  }

  @action.bound handleDropdownClick(event: any) {
    this.setDroppedDown(false);
  }

  @action.bound measureImm() {
    this.elmMeasure?.measure();
  }

  measureThrottled = _.throttle(this.measureImm, 100);

  refMeasure = (elm: any) => (this.elmMeasure = elm);
  elmMeasure: any;

  render() {
    return (
      <Measure ref={this.refMeasure} bounds={true}>
        {({ measureRef: refTriggerMeasure, contentRect: triggerContentRect }) => (
          <Observer>
            {() => (
              <div className={S.container} ref={this.refDropdown}>
                <div
                  ref={refTriggerMeasure}
                  className={S.trigger}
                  onClick={this.handleTriggerClick}
                >
                  {this.props.trigger}
                  <div className={S.dropdownSymbol}>
                    <i className="fas fa-caret-down" />
                  </div>
                </div>
                {this.isDroppedDown &&
                  createPortal(
                    <div
                      className={S.dropdown}
                      onClick={this.handleDropdownClick}
                      style={{
                        position: "absolute",
                        top: triggerContentRect.bounds?.bottom,
                        left: triggerContentRect.bounds?.left,
                        minWidth: triggerContentRect.bounds?.width,
                      }}
                    >
                      {this.props.children}
                    </div>,
                    document.getElementById("dropdown-portal")!
                  )}
              </div>
            )}
          </Observer>
        )}
      </Measure>
    );
  }
}

export const FilterSettingsComboBoxItem: React.FC<{
  onClick?: (event: any) => void;
}> = (props) => (
  <div className={S.dropdownItem} onClick={props.onClick}>
    {props.children}
  </div>
);
