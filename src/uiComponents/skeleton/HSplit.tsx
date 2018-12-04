import * as React from "react";
import { observer } from "mobx-react";
import { Splitter, SplitterPanel } from "../Splitter02";

@observer
export class HSplit extends React.Component<any> {
  public render() {
    return (
      <Splitter horizontal={true} handleSize={5} name="ScreenHSplit">
        {React.Children.map(this.props.children, (child, index) => (
          <SplitterPanel key={index} id={`${index}`}>
            {child}
          </SplitterPanel>
        ))}
      </Splitter>
    );
  }
}
