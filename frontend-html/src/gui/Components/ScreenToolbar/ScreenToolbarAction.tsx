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
