/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
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
