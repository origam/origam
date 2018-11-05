import * as React from 'react';
import { observer } from 'mobx-react';

@observer
export class VBox extends React.Component<any> {
  public render() {
    return (
      <div
        className="oui-vbox"
        style={{
          maxWidth: this.props.w,
          maxHeight: this.props.h
        }}
      >
        {this.props.children}
      </div>
    );
  }
}