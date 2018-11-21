import * as React from "react";
import { observer } from "mobx-react";
import { SplitPanel, Splitter } from "../Splitter";

@observer
export class VSplit extends React.Component<any> {
  public render() {
    return (
      <Splitter isVertical={true}>
        {React.Children.map(this.props.children, (child, index) => (
          <SplitPanel key={index} splitterId={index} initialSize={200}>
            {child}
          </SplitPanel>
        ))}
      </Splitter>
    );
  }
}
