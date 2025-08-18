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
import S from "gui/Components/Dropdown/DropdownItem.module.scss";
import cx from "classnames";

export const DropdownItem: React.FC<{
  className?: string;
  onClick?(event: any): void;
  isDisabled?: boolean;
  isSelected?: boolean;
  id?: string;
}> = props => {
  function getStyle() {
    if (props.isDisabled) {
      return "isDisabled"
    }
    return props.isSelected ? S.isSelected : ""
  }

  function onClick(event: any){
    if(!props.isDisabled){
      props.onClick?.(event);
    }
  }

  return <div
    id={props.id}
    onClick={onClick}
    className={cx(S.root, getStyle(), props.className, "dropdownItem")}
  >
    {props.children}
  </div>
};
