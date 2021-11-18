import React from "react";
import S from "gui/Components/Form/RadioButton.module.scss";
import {IFocusAble} from "model/entities/FocusManager";
import {uuidv4} from "../../../utils/uuid";


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
  onKeyDown: (event: any) => void;
  subscribeToFocusManager?: (obj: IFocusAble) => void;
  onClick: ()=>void;
  labelColor?: string;
}> {
  inputId = uuidv4();
  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
  };

  componentDidMount() {
    if(this.elmInput && this.props.subscribeToFocusManager){
      this.props.subscribeToFocusManager(this.elmInput);
    }
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
          height: this.props.height,
        }}
      >
        <input
          ref={this.refInput}
          className={S.input}
          type={"radio"}
          onClick={() => this.props.onClick()}
          id={this.inputId}
          name={this.props.name}
          value={this.props.value}
          checked={this.props.checked}
          onKeyDown={event => this.props.onKeyDown(event)}
          onChange={event => this.onChange(event)}/>
        <label
          className={S.label}
          htmlFor={this.inputId}
          style={{color: this.props.labelColor}}>
          {this.props.caption}
        </label>
      </div>
    );
  }
}