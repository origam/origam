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

import * as React from "react";
import { observer } from "mobx-react";
import S from "./BoolEditor.module.scss";
import cx from "classnames";
import { IFocusable } from "../../../../model/entities/FormFocusManager";

@observer
export class BoolEditor extends React.Component<{
  value: boolean;
  isReadOnly: boolean;
  readOnlyNoGrey?: boolean;
  onChange?(event: any, value: boolean): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onBlur?: () => void;
  onFocus?: () => void;
  id?: string;
  subscribeToFocusManager?: (obj: IFocusable) => void;
}> {
  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
  };

  componentDidMount() {
    if (this.elmInput && this.props.subscribeToFocusManager) {
      this.props.subscribeToFocusManager(this.elmInput);
    }
  }

  readOnly = (): string => {
    return (this.props.isReadOnly) ? "readOnly" : "";
  }

  render() {
    return (
      <div className={cx(S.editorContainer, this.readOnly())}>
        <input
          id={this.props.id ? this.props.id : undefined}
          className="editor"
          type="checkbox"
          checked={this.props.value || false}
          readOnly={!this.props.readOnlyNoGrey && this.props.isReadOnly}
          disabled={!this.props.readOnlyNoGrey && this.props.isReadOnly}
          onChange={(event: any) => {
            this.props.onChange &&
            !this.props.isReadOnly &&
            this.props.onChange(event, event.target.checked);
          }}
          autoComplete={"off"}
          onKeyDown={this.props.onKeyDown}
          onClick={this.props.onClick}
          onBlur={this.props.onBlur}
          onFocus={this.props.onFocus}
          ref={this.refInput}
          tabIndex={0}
        />
      </div>
    );
  }
}
