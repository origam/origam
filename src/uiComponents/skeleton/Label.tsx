import * as React from 'react';
import { observer } from 'mobx-react';

@observer
export class Label extends React.Component<any> {
  public render() {
    return (
      <div
        className="oui-label"
        style={{
          maxWidth: this.props.w,
          minWidth: this.props.w,
          maxHeight: this.props.h,
          minHeight: this.props.h
        }}
      >
        {this.props.name}
      </div>
    );
  }
}