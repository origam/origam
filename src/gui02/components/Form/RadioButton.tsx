import React from "react";
import S from "./RadioButton.module.scss";
import {IFocusable} from "../../../model/entities/FocusManager";


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
  subscribeToFocusManager?: (obj: IFocusable) => (()=>void);
}> {
  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
  };
  unsubscribeFromFocusManager?: () => void;

  componentDidMount() {
    if(this.elmInput && this.props.subscribeToFocusManager){
      this.unsubscribeFromFocusManager = this.props.subscribeToFocusManager(this.elmInput);
    }
  }

  componentWillUnmount() {
    this.unsubscribeFromFocusManager && this.unsubscribeFromFocusManager();
  }

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
          ref={this.refInput}
          className={S.input}
          type={"radio"}
          id={this.props.value}
          name={this.props.name}
          value={this.props.value}
          checked={this.props.checked}
          onChange={event => this.onChange(event)}/>
        <label htmlFor={this.props.value}>
          {this.props.caption}
        </label>
      </div>
    );
  }
}