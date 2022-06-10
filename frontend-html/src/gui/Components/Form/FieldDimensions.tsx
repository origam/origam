import React from "react";
import { IProperty } from "model/entities/types/IProperty";

export class FieldDimensions {

  top: number | undefined;
  left: number | undefined;
  width: number | undefined;
  height: number | undefined;

  constructor(args?:{
    top: number;
    left: number;
    width: number;
    height: number;
  }){
    if(args) {
      this.top = args.top;
      this.left = args.left;
      this.width = args.width;
      this.height = args.height;
    }
  }
  get isUnset(){
    return this.top === undefined  || this.left === undefined ||
      this.width === undefined || this.height === undefined ||
      this.top === null  || this.left === null ||
      this.width === null || this.height === null
  }

  asStyle(){
    if(this.isUnset){
      return {
        top: "unset",
        left: "unset",
        height: "unset",
        width: "unset",
        position: "relative"
      } as React.CSSProperties
    }
    else{
      return {
        top: this.top,
        left: this.left,
        height: this.height,
        width: this.width,
      } as React.CSSProperties
    }
  }
}

export function dimensionsFromXmlNode(xmlNode: any) {
  return new FieldDimensions({
    left: parseInt(xmlNode.attributes.X, 10),
    top: parseInt(xmlNode.attributes.Y, 10),
    width: parseInt(xmlNode.attributes.Width, 10),
    height: parseInt(xmlNode.attributes.Height, 10)
  });
}

export function dimensionsFromProperty(property: IProperty){
  return new FieldDimensions({
    height: property.height,
    width: property.width,
    left: property.x,
    top: property.y
  });
}
