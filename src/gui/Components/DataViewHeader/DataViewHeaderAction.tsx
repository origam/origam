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

import React, { PropsWithChildren, useContext } from "react";
import S from "gui/Components/DataViewHeader/DataViewHeaderAction.module.scss";
import cx from "classnames";
import { action } from "mobx";
import { CtxDataView, DataViewContext } from "../ScreenElements/DataView";

interface IDataViewHeaderActionProps {
  onMouseDown?(event: any): void;

  onClick?(event: any): void;

  className?: string;
  isActive?: boolean;
  isDisabled?: boolean;
  refDom?: any;

  onShortcut?(event: any): void;

  shortcutPredicate?(event: any): boolean;
}

export class DataViewHeaderActionInner extends React.Component<IDataViewHeaderActionProps & { dataViewContext?: DataViewContext }> {
  @action.bound
  handleMouseDown(event: any) {
    if (!this.props.isDisabled && this.props.onMouseDown) {
      this.props.onMouseDown(event);
    }
  }

  @action.bound
  handleClick(event: any) {
    if (!this.props.isDisabled && this.props.onClick) {
      this.props.onClick(event);
    }
  }

  @action.bound
  handleTriggerAreaKeyDown(event: any) {
    if (this.props.shortcutPredicate?.(event)) {
      event.preventDefault();
      this.props.onShortcut?.(event);
    }
  }

  _disposer: any;

  kbdHandlerUp() {
    this._disposer?.();
    if (this.props.dataViewContext) {
      this._disposer = this.props.dataViewContext.subscribeTableKeyDownHandler(
        this.handleTriggerAreaKeyDown
      );
    }
  }

  kbdHandlerDown() {
    this._disposer?.();
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
        className={cx(
          S.root,
          this.props.className,
          {isActive: this.props.isActive},
          this.props.isDisabled ? S.isDisabled : ""
        )}
        onMouseDown={this.handleMouseDown}
        onClick={this.handleClick}
        ref={this.props.refDom}
      >
        {this.props.children}
      </div>
    );
  }
}

export function DataViewHeaderAction(props: PropsWithChildren<IDataViewHeaderActionProps>) {
  const dataViewContext = useContext(CtxDataView);
  return <DataViewHeaderActionInner {...props} dataViewContext={dataViewContext}/>;
}
