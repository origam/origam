/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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
import S from "./Button.module.scss";

export class Button extends React.Component<{
  onClick: () => void,
  label: string,
  className?: string;
  changeColorOnFocus?: boolean;
  disabled?: boolean;
}> {

  getStyle(){
    let className = S.button + " " + (this.props.className ?? "");
    if(this.props.disabled){
      className += " " + S.disabledButton;
      return className;
    }
    if(this.props.changeColorOnFocus){
      className += " " + S.focusedButton;
    }
    return className;
  }

  render() {
    return <button
      className={this.getStyle()}
      onClick={() => this.props.onClick()}
      disabled={this.props.disabled}
    >
      {this.props.label}
    </button>
  }
}