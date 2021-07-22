import React from "react";
import S from "./Button.module.scss";

export class Button extends React.Component<{
  onClick: () => void,
  label: string,
  changeColorOnFocus?: boolean
}> {
  render() {
    return <button
      className={S.button + " " + (this.props.changeColorOnFocus ? S.focusedButton : "") }
      onClick={() => this.props.onClick()}>{this.props.label}</button>
  }
}