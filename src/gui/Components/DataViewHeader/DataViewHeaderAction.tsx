import React, { useContext, PropsWithChildren } from "react";
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

export class DataViewHeaderActionInner extends React.Component<
  IDataViewHeaderActionProps & { dataViewContext?: DataViewContext }
> {
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
    console.log("***", event);
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
          { isActive: this.props.isActive },
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
  return <DataViewHeaderActionInner {...props} dataViewContext={dataViewContext} />;
}
