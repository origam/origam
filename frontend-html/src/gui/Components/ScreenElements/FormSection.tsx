import S from "./FormSection.module.css";
import React from "react";

export class FormSection extends React.Component<{
  width: number;
  height: number;
  x: number;
  y: number;
  title: string;
}> {
  render() {
    return (
      <div
        className={S.formSection}
        style={{
          top: this.props.y,
          left: this.props.x,
          width: this.props.width,
          height: this.props.height
        }}
      >
        <div className={S.sectionTitle}>{this.props.title}</div>
        {this.props.children}
      </div>
    );
  }
}
