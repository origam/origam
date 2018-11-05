import * as React from 'react';
import { observer } from 'mobx-react';

@observer
export class HSplit extends React.Component<any> {
  public render() {
    let children = React.Children.map(this.props.children, child => (
      <div className="oui-hsplit-panel">{child}</div>
    ));
    children = React.Children.map(children, (child, idx) => (
      <>
        {child}
        {idx < children.length - 1 && (
          <div className="oui-hsplit-handle">
            <div className="knob" />
          </div>
        )}
      </>
    ));
    return <div className="oui-hsplit-container">{children}</div>;
  }
}