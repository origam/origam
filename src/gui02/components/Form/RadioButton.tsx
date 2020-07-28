import React from "react";
import S from "./FormLabel.module.scss";

export class RadioButton extends React.Component<{
  caption: string;
  top: number;
  left: number;
  width: number;
  height: number;
  name: string;
  value: string;
  onSelected: (value: any) => void;
  checked: boolean;
}> {

  onChange(event: any){
    if(event.target.value === this.props.value){
      this.props.onSelected(this.props.value);
    }
  }

  render() {
    return (
      <div
        className={S.root}
        style={{
          top: this.props.top,
          left: this.props.left,
          width: this.props.width,
          height: this.props.height
        }}
      >
        <input
          type={"radio"}
          id={this.props.value}
          name={this.props.name}
          value={this.props.value}
          checked={this.props.checked}
          onChange={event => this.onChange(event)}/>
        <label>{this.props.caption}</label>
      </div>
    );
  }
}