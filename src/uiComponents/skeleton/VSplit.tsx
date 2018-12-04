import * as React from "react";
import { observer } from "mobx-react";
import { SplitterPanel, Splitter } from "../Splitter02";

@observer
export class VSplit extends React.Component<any> {
  public render() {
    return (
      <Splitter horizontal={false} handleSize={5} name="ScreenVSplit" >
        {React.Children.map(this.props.children, (child, index) => (
          <SplitterPanel key={index} id={`${index}`}>
            {child}
          </SplitterPanel>
        ))}
      </Splitter>
    );
  }
}
