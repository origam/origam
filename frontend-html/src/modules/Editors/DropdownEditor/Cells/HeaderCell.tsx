import React from "react";
import { IHeaderCellDriver } from "../DropdownTableModel";
import { TypeSymbol } from "dic/Container";

export class DefaultHeaderCellDriver implements IHeaderCellDriver {
  constructor( private name: string) {}

  render() {
    return <div className={"header cell"}>{this.name}</div>;
  }
}
export const IDefaultHeaderCellDriver = TypeSymbol<DefaultHeaderCellDriver>(
  "IDefaultHeaderCellDriver"
);
