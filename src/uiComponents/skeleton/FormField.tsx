import * as React from "react";
import { observer } from "mobx-react";

@observer
export class FormField extends React.Component<any> {
  public render() {
    console.log(this.props)
    const {property} = this.props;
    const {x, y, w, h, captionPosition, captionLength, entity, name} = property;

    if (!Number.isInteger(x) || !Number.isInteger(y)) {
      return null;
    }
    let captionLocation;
    if (captionPosition === "Left") {
      captionLocation = {
        left: x - captionLength,
        top: y,
        width: captionLength,
        minHeight: 20 // this.props.h,
      };
    } else if (captionPosition === "Top") {
      captionLocation = {
        left: x,
        top: y - 20,
        width: captionLength,
        minHeight: 20 // this.props.h,
      };
    } else {
      captionLocation = {
        left:
          x +
          (entity === "Boolean" ? h : w), // + this.props.captionLength,
        top: y,
        width: captionLength,
        minHeight: 20 // this.props.h,
      };
    }
    return (
      <>
        <div
          className="oui-property"
          style={{
            top: y,
            left: x,
            width:
              entity === "Boolean" ? h : w,
            height: h
          }}
        >
          {/*`Type: ${this.props.type} Name: ${this.props.name}, Id: ${
            this.props.id
          }`*/}
          {/*this.props.children*/}
          {/*this.props.name*/}
          {entity}
        </div>
        {captionPosition !== "None" && (
          <div className="oui-property-caption" style={{ ...captionLocation }}>
            {name}
          </div>
        )}
      </>
    );
  }
}
