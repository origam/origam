import * as React from "react";
import { observer } from "mobx-react";
import { ISplitterModel, Splitter, SplitterPanel } from "../Splitter/Splitter";

@observer
export class VSplit extends React.Component<{ model?: ISplitterModel }> {
  render() {
    return (
      <Splitter
        horizontal={false}
        name="abc"
        handleSize={3}
        model={this.props.model}
      >
        {React.Children.map(this.props.children, (child: any, idx: number) => {
          return <SplitterPanel id={`${idx}`}>{child}</SplitterPanel>;
        })}
      </Splitter>
    );
  }
}
