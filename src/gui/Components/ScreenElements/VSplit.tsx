import React from "react";
import {Splitter, SplitterPanel} from "../Splitter/Splitter";

export class VSplitPanel extends React.Component<{ id: any }> {
  render() {
    return (
      <SplitterPanel id={this.props.id} {...this.props}>
        {this.props.children}
      </SplitterPanel>
    );
  }
}

export class VSplit extends React.Component<{ handleClassName?: string }> {
  render() {
    return (
      <Splitter
        horizontal={false}
        name=""
        handleSize={3}
        handleClassName={this.props.handleClassName}
      >
        {this.props.children}
      </Splitter>
    );
  }
}
