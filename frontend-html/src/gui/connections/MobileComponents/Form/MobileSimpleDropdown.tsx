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

import React, { CSSProperties, useState } from "react";
import S from "gui/connections/MobileComponents/Form/MobileSimpleDropdown.module.scss";
import CS from "gui/Components/Dropdown/Dropdown.module.scss";
import { observer } from "mobx-react";
import { EditLayoutState } from "model/entities/MobileState/MobileLayoutState";
import { getMobileState } from "model/selectors/getMobileState";
import cx from "classnames";
import "gui/Components/Dropdown/Dropdown.module.scss";
import { prepareForFilter } from "model/selectors/PortalSettings/getStringFilterConfig";
import { IOption } from "gui/Components/Dialogs/SimpleDropdown";

@observer
export class MobileSimpleDropdown<T> extends React.Component<{
  width?: string,
  options: IOption<T>[],
  selectedOption: IOption<T>,
  onOptionClick: (option: IOption<T>) => void
  className?: string;
  ctx: any
}> {

  getStyle(){
    if(this.props.width){
      return {width: this.props.width}
    }
    return {}
  }

  onClick(){
    let mobileState = getMobileState(this.props.ctx);
    const previousLayout = mobileState.layoutState;
    mobileState.layoutState = new EditLayoutState(
      <FullScreenEditor
        options={this.props.options}
        onOptionClick={(option)=> {
          this.props.onOptionClick(option);
          mobileState.layoutState = previousLayout;
        }}
        ctx={this.props.ctx}
      />,
      "",
      previousLayout
    )
  }

  render() {
    return (
      <DropDownControl
        style={this.getStyle()}
        className={this.props.className}
        onMouseDown={() => this.onClick()}
        value={this.props.selectedOption.label}
      />
    );
  }
}

function DropDownControl(props: {
  onMouseDown: () => void;
  value: string;
  style: CSSProperties;
  className?: string
}) {
  return (
    <div
      style={props.style}
      onMouseDown={() => props.onMouseDown()}
      className={CS.control + " " + props.className}>
      <input
        autoComplete={"off"}
        className={S.input}
        value={props.value}
        disabled={true}
      />
      <div
        className="inputBtn lastOne">
        <i className="fas fa-caret-down"/>
      </div>
    </div>
  )
}

function FullScreenEditor<T>(props: {
  options: IOption<T>[],
  onOptionClick: (option: IOption<T>) => void,
  ctx: any
}) {

  const [value, setValue] = useState("");

  function onClick(option: IOption<T>){
    props.onOptionClick(option);
  }

  function getFilteredOptions(){
    if(value.trim() === ""){
      return props.options;
    }
    const valueForFilter = prepareForFilter(props.ctx, value) ?? "";
    return props.options
      .filter(option => {
        const labelForFilter = prepareForFilter(props.ctx, option.label) ?? "";
        return labelForFilter.includes(valueForFilter)
      });
  }

  return (
    <div className={cx(CS.table, S.root)}>
      <div className={cx(CS.control, S.inputContainer)}>
        <input
          autoComplete={"off"}
          className={cx("input", CS.input)}
          value={value}
          onChange={event => setValue(event.target.value)}
        />
      </div>
      {getFilteredOptions()
        .map((option, i) =>
          <div
            className={S.cell + " cell " + (i%2 ? "ord1" : "ord2")}
            onClick={() => onClick(option)}>
            {option.label}
          </div>
        )}
    </div>
  )
}

