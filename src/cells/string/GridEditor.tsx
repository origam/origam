import * as React from "react";
import { action } from "mobx";

export class StringGridEditor extends React.Component {
  public componentDidMount() {
    this.elmInput!.focus();
    setTimeout(() => {
      this.elmInput && this.elmInput.select();
    }, 10);
  }

  private elmInput: HTMLInputElement | null;

  @action.bound
  private refInput(elm: HTMLInputElement) {
    this.elmInput = elm;
  }

  public render() {
    return (
      <input
      ref={this.refInput}
        style={{
          width: "100%",
          height: "100%",
          border: "none",
          padding: "0px 0px 0px 15px",
          margin: 0
        }}
        value="asdf"
      />
    );
  }
}
