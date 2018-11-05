import * as React from 'react';
import { observer } from 'mobx-react';

@observer
export class VSplit extends React.Component<any> {
  public render() {
    let children = React.Children.map(this.props.children, child => (
      <div className="oui-vsplit-panel">{child}</div>
    ));
    children = React.Children.map(children, (child, idx) => (
      <>
        {child}
        {idx < children.length - 1 && (
          <div className="oui-vsplit-handle">
            <div className="knob" />
          </div>
        )}
      </>
    ));
    return <div className="oui-vsplit-container">{children}</div>;
  }
}
