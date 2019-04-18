import * as React from "react";
import { observer } from "mobx-react";
import { Splitter, ISplitterModel, SplitterPanel } from "../controls/Splitter/Splitter";


@observer
export class HSplit extends React.Component {
  render() {
    return (
      <Splitter
        horizontal={true}
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
