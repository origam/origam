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