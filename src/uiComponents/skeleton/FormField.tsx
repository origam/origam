import * as React from "react";
import { observer } from "mobx-react";

@observer
export class FormField extends React.Component<any> {
  public render() {
    if (!Number.isInteger(this.props.x) || !Number.isInteger(this.props.y)) {
      return null;
    }
    let captionLocation;
    if (this.props.captionPosition === "Left") {
      captionLocation = {
        left: this.props.x - this.props.captionLength,
        top: this.props.y,
        width: this.props.captionLength,
        minHeight: 20 // this.props.h,
      };
    } else if (this.props.captionPosition === "Top") {
      captionLocation = {
        left: this.props.x,
        top: this.props.y - 20,
        width: this.props.captionLength,
        minHeight: 20 // this.props.h,
      };
    } else {
      captionLocation = {
        left:
          this.props.x +
          (this.props.entity === "Boolean" ? this.props.h : this.props.w), // + this.props.captionLength,
        top: this.props.y,
        width: this.props.captionLength,
        minHeight: 20 // this.props.h,
      };
    }
    return (
      <>
        <div
          className="oui-property"
          style={{
            top: this.props.y,
            left: this.props.x,
            width:
              this.props.entity === "Boolean" ? this.props.h : this.props.w,
            height: this.props.h
          }}
        >
          {/*`Type: ${this.props.type} Name: ${this.props.name}, Id: ${
            this.props.id
          }`*/}
          {/*this.props.children*/}
          {/*this.props.name*/}
          {this.props.entity}
        </div>
        {this.props.captionPosition !== "None" && (
          <div className="oui-property-caption" style={{ ...captionLocation }}>
            {this.props.name}
          </div>
        )}
      </>
    );
  }
}
