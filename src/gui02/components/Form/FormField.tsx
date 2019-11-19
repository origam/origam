import React from "react";
import S from "./FormField.module.scss";
import { render } from "react-dom";

export enum ICaptionPosition {
  Left = "Left",
  Right = "Right",
  Top = "Top",
  None = "None"
}

export class FormField extends React.Component<{
  caption: React.ReactNode;
  captionPosition?: ICaptionPosition;
  captionLength: number;
  editor: React.ReactNode;
  left: number;
  top: number;
  width: number;
  height: number;
  isCheckbox?: boolean;
}> {
  get captionStyle() {
    switch (this.props.captionPosition) {
      default:
      case ICaptionPosition.Left:
        return {
          top: this.props.top,
          left: this.props.left - this.props.captionLength,
          width: this.props.captionLength
          //  height: this.props.height
        };
      case ICaptionPosition.Right:
        return {
          top: this.props.top,
          left: this.props.isCheckbox
            ? this.props.left + this.props.height
            : this.props.left + this.props.width,
          width: this.props.captionLength
          //  height: this.props.height
        };
      case ICaptionPosition.Top:
        return {
          top: this.props.top - 20, // TODO: Move this constant somewhere else...
          left: this.props.left,
          width: this.props.captionLength
        };
    }
  }

  get formFieldStyle() {
    return {
      left: this.props.left,
      top: this.props.top,
      width: this.props.width,
      height: this.props.height
    };
  }

  render() {
    const { props } = this;
    return (
      <>
        {this.props.captionPosition !== ICaptionPosition.None && (
          <label className={S.caption} style={this.captionStyle}>
            {props.caption}
          </label>
        )}
        <div className={S.editor} style={this.formFieldStyle}>
          {props.editor}
        </div>
      </>
    );
  }
}
