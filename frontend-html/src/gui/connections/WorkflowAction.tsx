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

import S from "./WorkflowAction.module.scss";
import React from "react";
import cx from "classnames";
import { action } from "mobx";

export class WorkflowAction extends React.Component<{
  label: string;
  className?: string;
  onClick?(event: any): void;
  onShortcut?(event: any): void;
  shortcutPredicate?(event: any): boolean;
  id?: string;
}> {
  @action.bound
  handleWindowKeyDown(event: any) {
    if (this.props.shortcutPredicate?.(event)) {
      event.preventDefault();
      this.props.onShortcut?.(event);
    }
  }

  kbdHandlerUp() {
    window.addEventListener("keydown", this.handleWindowKeyDown);
  }

  kbdHandlerDown() {
    window.removeEventListener("keydown", this.handleWindowKeyDown);
  }

  componentDidMount() {
    if (this.props.onShortcut && this.props.shortcutPredicate) {
      this.kbdHandlerUp();
    }
  }

  componentDidUpdate(prevProps: any) {
    if (
      (!prevProps.onShortcut || !prevProps.shortcutPredicate) &&
      this.props.onShortcut &&
      this.props.shortcutPredicate
    ) {
      this.kbdHandlerUp();
    } else if (
      prevProps.onShortcut &&
      prevProps.shortcutPredicate &&
      (!this.props.onShortcut || !this.props.shortcutPredicate)
    ) {
      this.kbdHandlerDown();
    }
  }

  componentWillUnmount() {
    this.kbdHandlerDown();
  }

  render() {
    return (
      <div
        className={cx(S.root,this.props.className)}
        onClick={this.props.onClick}
      >
        {this.props.label}
      </div>
    );
  }
}
