import React from "react";
import S from "./HSplit.module.css";
import { SplitterPanel, Splitter } from "../Splitter/Splitter";

export class HSplitPanel extends React.Component<{ id: any }> {
  render() {
    return (
      <SplitterPanel id={this.props.id} {...this.props}>
        {this.props.children}
      </SplitterPanel>
    );
  }
}

export class HSplit extends React.Component<{ handleClassName?: string }> {
  render() {
    return (
      <Splitter
        horizontal={true}
        name=""
        handleSize={3}
        handleClassName={this.props.handleClassName}
      >
        {this.props.children}
      </Splitter>
    );
  }
}
