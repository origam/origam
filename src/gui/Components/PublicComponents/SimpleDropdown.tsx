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

import {observer} from "mobx-react";
import React, {useContext} from "react";
import {observable} from "mobx";
import {DropdownLayout} from "modules/Editors/DropdownEditor/Dropdown/DropdownLayout";
import {DropdownLayoutBody} from "modules/Editors/DropdownEditor/Dropdown/DropdownLayoutBody";
import {
  CtxDropdownRefBody,
  CtxDropdownRefCtrl
} from "modules/Editors/DropdownEditor/Dropdown/DropdownCommon";
import S from "./SimpleDropdown.module.scss";
import CS from "modules/Editors/DropdownEditor/Dropdown/Dropdown.module.scss";
import { v4 as uuidv4 } from 'uuid';
@observer
export class SimpleDropdown<T> extends React.Component<{
  width: string,
  options: IOption<T>[],
  selectedOption: IOption<T>,
  onOptionClick: (option: IOption<T>) => void
}> {
  id = "SimpleDropdown_"+uuidv4()

  @observable
  _isDropped = false;

  get isDropped(){
    return this._isDropped;
  }

  set isDropped(value: boolean){
    if(value){
      document.addEventListener("mousedown", event => this.documentClickAndWheelListener(event))
      document.addEventListener("wheel", event => this.documentClickAndWheelListener(event))
    }else{
      document.removeEventListener("mousedown", event => this.documentClickAndWheelListener(event));
      document.removeEventListener("wheel", event => this.documentClickAndWheelListener(event));
    }
    this._isDropped = value;
  }

  documentClickAndWheelListener(event: any) {
    const simpleDropdown = document.getElementById(this.id);
    const dropdownPortal = document.getElementById("dropdown-portal");
    if (!dropdownPortal || !simpleDropdown){
      document.removeEventListener("mousedown", event => this.documentClickAndWheelListener(event));
      document.removeEventListener("wheel", event => this.documentClickAndWheelListener(event));
      return;
    }
    let targetElement = event.target;
    if(!targetElement){
      return;
    }
    if(!dropdownPortal!.contains(targetElement) && !simpleDropdown!.contains(targetElement)){
      this.isDropped = false;
    }
  }

  onOptionClick(option: IOption<T>){
    this.isDropped = false;
    this.props.onOptionClick(option);
  }

  render() {
    return (
      <div
        id={this.id}>
        <DropdownLayout
          isDropped={this.isDropped}
          renderCtrl={() => (
            <DropDownControl
              width={this.props.width}
              onMouseDown={() => this.isDropped = !this.isDropped}
              value={this.props.selectedOption.label}
            />
          )}
          renderDropdown={() => (
            <DropdownLayoutBody
              minSideMargin={0}
              render={() => (
                <DropDownBody
                  width={this.props.width}
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


export function DropDownControl(props: {
  onMouseDown: ()=> void;
  value: string;
  width: string
}) {
  const ref = useContext(CtxDropdownRefCtrl);

  return (
    <div
      style={{width: props.width, height: "19px"}}
      onMouseDown={()=>props.onMouseDown()}
      ref={ref}
      className={CS.control}>
      <input
        className="input"
        value={props.value}
        disabled={true}
      />
      <div
        className="inputBtn lastOne">
        <i className="fas fa-caret-down"></i>
      </div>
    </div>
  )
}


export function DropDownBody<T>(props: {
  width: string,
  options: IOption<T>[],
  selected: IOption<T>,
  onOptionClick: (option: IOption<T>) => void
}) {
  const ref = useContext(CtxDropdownRefBody);

  return (
    <div
      ref={ref}
      className={S.body}
      style={{width: props.width}}>
      {props.options.map((option, i) =>
        <div
          className={S.cell + " " + (i%2 ? S.ord2 : S.ord1) + " " + (option.value === props.selected.value ? S.selected : "")}
          onClick={() => props.onOptionClick(option)}>{option.label}</div>)
      }
    </div>
  )
}

export interface IOption<T>{
  value: T
  label: string
}
