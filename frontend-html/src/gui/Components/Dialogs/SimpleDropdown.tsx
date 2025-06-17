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

import { observer } from "mobx-react";
import React, { CSSProperties, useContext, useState } from "react";
import { observable } from "mobx";
import { v4 as uuidv4 } from 'uuid';
import S from "./SimpleDropdown.module.scss";
import CS from "gui/Components/Dropdown/Dropdown.module.scss";
import cx from "classnames";
import { DropdownLayoutBody } from "gui/Components/Dropdown/DropdownLayoutBody";
import { DropdownLayout } from "gui/Components/Dropdown/DropdownLayout";
import { CtxDropdownRefBody, CtxDropdownRefCtrl } from "gui/Components/Dropdown/DropdownCommon";

@observer
export class SimpleDropdown<T> extends React.Component<{
  width?: string,
  options: IOption<T>[],
  selectedOption: IOption<T>,
  onOptionClick: (option: IOption<T>) => void
  className?: string;
}> {
  id = "SimpleDropdown_" + uuidv4()

  @observable
  _isDropped = false;

  get isDropped() {
    return this._isDropped;
  }

  set isDropped(value: boolean) {
    if (value) {
      document.addEventListener("mousedown", event => this.documentClickAndWheelListener(event))
      document.addEventListener("wheel", event => this.documentClickAndWheelListener(event))
    } else {
      document.removeEventListener("mousedown", event => this.documentClickAndWheelListener(event));
      document.removeEventListener("wheel", event => this.documentClickAndWheelListener(event));
    }
    this._isDropped = value;
  }

  documentClickAndWheelListener(event: any) {
    const simpleDropdown = document.getElementById(this.id);
    const dropdownPortal = document.getElementById("dropdown-portal");
    if (!dropdownPortal || !simpleDropdown) {
      document.removeEventListener("mousedown", event => this.documentClickAndWheelListener(event));
      document.removeEventListener("wheel", event => this.documentClickAndWheelListener(event));
      return;
    }
    let targetElement = event.target;
    if (!targetElement) {
      return;
    }
    if (!dropdownPortal!.contains(targetElement) && !simpleDropdown!.contains(targetElement)) {
      this.isDropped = false;
    }
  }

  onOptionClick(option: IOption<T>) {
    this.isDropped = false;
    this.props.onOptionClick(option);
  }

  getStyle(){
    if(this.props.width){
      return {width: this.props.width}
    }
    return {}
  }

  render() {
    return (
      <div
        id={this.id}>
        <DropdownLayout
          isDropped={this.isDropped}
          renderCtrl={() => (
            <DropDownControl
              style={this.getStyle()}
              className={this.props.className}
              onMouseDown={() => this.isDropped = !this.isDropped}
              value={this.props.selectedOption.label}
            />
          )}
          renderDropdown={() => (
            <DropdownLayoutBody
              minSideMargin={0}
              render={() => (
                <DropDownBody
                  style={this.getStyle()}
                  options={this.props.options}
                  selected={this.props.selectedOption}
                  onOptionClick={option => this.onOptionClick(option)}
                />
              )}
            />
          )}
        />
      </div>
    );
  }
}


function DropDownControl(props: {
  onMouseDown: () => void;
  value: string;
  style: CSSProperties;
  className?: string
}) {
  const ref = useContext(CtxDropdownRefCtrl);

  return (
    <div
      style={props.style}
      onMouseDown={() => props.onMouseDown()}
      ref={ref}
      className={CS.control + " " + props.className}>
      <input
        className={cx(S.input)}
        value={props.value}
        disabled={true}
        autoComplete={"new-password"}
      />
      <div
        className="inputBtn lastOne">
        <i className="fas fa-caret-down"/>
      </div>
    </div>
  )
}


function DropDownBody<T>(props: {
  style: CSSProperties;
  options: IOption<T>[],
  selected: IOption<T>,
  onOptionClick: (option: IOption<T>) => void
}) {
  const ref = useContext(CtxDropdownRefBody);
  const [hoveredRowIndex, setHoveredRowIndex] = useState(-1);

  return (
    <div
      ref={ref}
      className={cx(CS.body, CS.table, S.body)}
      style={props.style}>
      {props.options.map((option, i) =>
        <div
          key={option.label + option.value}
          className={cx({ isHoveredRow: i === hoveredRowIndex }, S.cell)}
          onMouseOver={(evt) => {
            setHoveredRowIndex(i);
          }}
          onMouseOut={(evt) => {
            setHoveredRowIndex(-1);
          }}
        >
          <div
            className={"cell " + (i%2 ? "ord2" : "ord1") + " " + (option.value === props.selected.value ? "withCursor" : "")}
            onClick={() => props.onOptionClick(option)}
          >
            {option.label}
          </div>
        </div>
        )
      }
    </div>
  )
}

export interface IOption<T> {
  value: T
  label: string
}