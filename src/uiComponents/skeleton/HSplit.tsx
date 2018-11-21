import * as React from "react";
import { observer } from "mobx-react";
import { Splitter, SplitPanel } from "../Splitter";

@observer
export class HSplit extends React.Component<any> {
  public render() {
    return (
      <Splitter isVertical={false}>
        {React.Children.map(this.props.children, (child, index) => (
          <SplitPanel key={index} splitterId={index} initialSize={200}>
            {child}
          </SplitPanel>
        ))}
      </Splitter>
    );
  }
}
