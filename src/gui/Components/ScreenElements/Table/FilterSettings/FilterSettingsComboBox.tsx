/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

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
        {({measureRef: refTriggerMeasure, contentRect: triggerContentRect}) => (
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
                    <i className="fas fa-caret-down"/>
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
