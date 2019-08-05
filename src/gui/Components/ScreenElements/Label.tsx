import React from "react";
import S from "./Label.module.css";

export class Label extends React.Component<{ height?: number; text: string }> {
  render() {
    return (
      <div className={S.label} style={{ height: this.props.height }}>
        {this.props.text}
      </div>
    );
  }
}
