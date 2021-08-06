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
import S from "gui/Components/Form/RadioButton.module.scss";
import {IFocusable} from "model/entities/FormFocusManager";
import { v4 as uuidv4 } from 'uuid';


export class RadioButton extends React.Component<{
  caption: string;
  top: number
  left: number;
  width: number;
  height: number;
  name: string;
  value: string;
  onSelected: (value: any) => void;
  checked: boolean;
  onKeyDown: (event: any) => void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  onClick: ()=>void;
  labelColor?: string;
}> {
  inputId = uuidv4();
  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
  };

  componentDidMount() {
    if(this.elmInput && this.props.subscribeToFocusManager){
      this.props.subscribeToFocusManager(this.elmInput);
    }
  }

  onChange(event: any){
    if(event.target.value === this.props.value){
      this.props.onSelected(this.props.value);
    }
  }

  render() {
    return (
      <div
        className={S.root}
        style={{
          top: this.props.top,
          left: this.props.left,
          width: this.props.width,
          height: this.props.height,
        }}
      >
        <input
          ref={this.refInput}
          className={S.input}
          type={"radio"}
          onClick={() => this.props.onClick()}
          id={this.inputId}
          name={this.props.name}
          value={this.props.value}
          checked={this.props.checked}
          onKeyDown={event => this.props.onKeyDown(event)}
          onChange={event => this.onChange(event)}/>
        <label
          className={S.label}
          htmlFor={this.inputId}
          style={{color: this.props.labelColor}}>
          {this.props.caption}
        </label>
      </div>
    );
  }
}