import * as React from "react";
import { observer } from "mobx-react";
import { ISplitterModel, SplitterPanel, Splitter } from "../controls/Splitter/Splitter";


@observer
export class VSplit extends React.Component {
  render() {
    return (
      <Splitter
        horizontal={false}
        name="abc"
        handleSize={3}
      >
        {React.Children.map(this.props.children, (child: any, idx: number) => {
          return <SplitterPanel id={`${idx}`}>{child}</SplitterPanel>;
        })}
      </Splitter>
    );
  }
}
