import React from "react";
import S from "./TagInput.module.css";

export class TagInput extends React.Component {
  render() {
    return <div className={S.container}>{this.props.children}</div>;
  }
}

export class TagInputItem extends React.Component {
  render() {
    return <div className={S.item}>{this.props.children}</div>;
  }
}

export class TagInputItemContent extends React.Component {
  render() {
    return <div className={S.itemContent}>{this.props.children}</div>;
  }
}

export class TagInputRemoveBtn extends React.Component<{
  onClick?(event: any): void;
}> {
  render() {
    return (
      <div onClick={this.props.onClick} className={S.removeBtn}>
        X
      </div>
    );
  }
}

export class TagInputAddBtn extends React.Component<{
  domRef?: any;
  onClick?: (event: any) => void;
}> {
  render() {
    return (
      <div
        ref={this.props.domRef}
        onClick={this.props.onClick}
        className={S.addBtn}
      >
        +
      </div>
    );
  }
}

type ITagInputTextboxProps = React.DetailedHTMLProps<
  React.InputHTMLAttributes<HTMLInputElement>,
  HTMLInputElement
> & {
  dropdownControl?: React.ReactNode;
};

export class TagInputTextbox extends React.Component<ITagInputTextboxProps> {
  render() {
    return (
      <div className={S.textboxContainer}>
        <input
          {...this.props}
          className={
            S.textbox + (this.props.className ? ` ${this.props.className}` : ``)
          }
        />
        <div className={S.dropdownControlContainer}>
          {this.props.dropdownControl}
        </div>
      </div>
    );
  }
}