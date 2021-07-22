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

import S from "gui/Components/ScreenToolbar/ScreenToolbarAction.module.scss";
import React from "react";
import cx from "classnames";
import { action } from "mobx";

export class ScreenToolbarAction extends React.Component<{
  icon?: React.ReactNode;
  label?: string;
  isHidden?: boolean;
  rootRef?: any;
  className?: string;
  onMouseDown?(event: any): void;
  onClick?(event: any): void;
  onShortcut?(event: any): void;
  shortcutPredicate?(event: any): boolean;
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
        ref={this.props.rootRef}
        className={cx(
          S.root,
          { isLabelless: !this.props.label, isHidden: this.props.isHidden },
          this.props.className
        )}
        onMouseDown={this.props.onMouseDown}
        onClick={this.props.onClick}
        title={this.props.label}
      >
        {this.props.icon && <div className={S.icon}>{this.props.icon}</div>}
        {this.props.label && <div className={S.label}>{this.props.label}</div>}
      </div>
    );
  }
}
