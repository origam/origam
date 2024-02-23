/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

import { observer } from "mobx-react";
import React from "react";
import { IDataView } from "../../../model/entities/types/IDataView";
import { action, computed, observable } from "mobx";
import S from "./TreeView.module.scss";
import cx from "classnames";
import { isTreeDataTable, TreeDataTable } from "../../../model/entities/TreeDataTable";
import { runGeneratorInFlowWithHandler } from "utils/runInFlowWithHandler";

@observer
export class TreeView extends React.Component<{ dataView: IDataView }> {
  constructor(props: Readonly<{ dataView: IDataView }>) {
    super(props);

    if (!isTreeDataTable(this.props.dataView.dataTable)) {
      throw new Error("TreeView requires TreeDataTable to work properly");
    }
  }

  @computed
  get nodes() {
    const nodes = this.props.dataView.dataTable.rows.map(
      (row) => new Node({
        id: this.getRowId(row),
        label: this.getLabel(row),
        row: row,
        expansionGetter: (id: string) => this.expanded.includes(id)
      })
    );

    for (let node of nodes) {
      const parentId = this.getParentId(node.row);
      if (parentId) {
        node.parent = nodes.find((otherNode) => otherNode.id === parentId);
      }
    }
    return nodes;
  }

  getRowId(row: any) {
    return this.props.dataView.dataTable.getRowId(row);
  }

  getParentId(row: any) {
    return (this.props.dataView.dataTable as TreeDataTable).getParentId(row);
  }

  private getLabel(row: any[]) {
    return (this.props.dataView.dataTable as TreeDataTable).getLabel(row);
  }

  @observable
  expanded: string[] = []

  onRowClick(node: Node) {
    const self = this;
    runGeneratorInFlowWithHandler({
      ctx: this.props.dataView,
      generator: function*(){
        yield*self.props.dataView.setSelectedRowId  (node.id);
      }()
    })
  }

  onCaretClick(node: Node) {
    if (this.expanded.includes(node.id)) {
      this.expanded.remove(node.id);
    } else {
      this.expanded.push(node.id);
    }
  }

  render() {
    return (
      <div className={S.treeView}>
        {this.nodes
          .filter((node) => node.isVisible)
          .map((node) => (
            <Row
              key={node.id}
              node={node}
              isSelected={node.id === this.props.dataView.selectedRowId}
              onRowClick={() => this.onRowClick(node)}
              onCaretClick={() => this.onCaretClick(node)}
            />
          ))}
      </div>
    );
  }
}

class Node {
  id: string;
  label: string;
  _parent: Node | undefined;
  row: any[];
  isFolder: boolean = false;
  expansionGetter: (nodeId: string) => boolean;

  get isExpanded() {
    return this.expansionGetter(this.id);
  }

  constructor(args: { id: string, label: string, row: any[], expansionGetter: (nodeId: string) => boolean }) {
    this.id = args.id;
    this.label = args.label;
    this.row = args.row;
    this.expansionGetter = args.expansionGetter;
  }

  get level() {
    return this.countParents(this, 0);
  }

  get parent(): Node | undefined {
    return this._parent;
  }

  set parent(value: Node | undefined) {
    this._parent = value;
    if (this._parent) {
      this._parent.isFolder = true;
    }
  }

  @computed
  get isVisible() {
    if (!this.parent) {
      return true;
    }
    return this.parent.isExpanded;
  }

  private countParents(node: Node, parentCount: number): number {
    if (!node.parent) {
      return parentCount;
    }
    parentCount++;
    return this.countParents(node.parent, parentCount);
  }
}

class Row extends React.Component<{
  node: Node;
  isSelected: boolean;
  onRowClick: () => void;
  onCaretClick: () => void;
}> {
  getIndent() {
    return this.props.node.level * 20 + "px";
  }

  @action.bound
  handleRowClick(event: any) {
    this.props.onRowClick?.();
    if (!this.props.node?.isExpanded) {
      event.stopPropagation();
      this.props.onCaretClick?.();
    }
  }

  @action.bound
  handleCaretClick(event: any) {
    this.props.onCaretClick?.();
    event.stopPropagation();
  }

  render() {
    const {isExpanded} = this.props.node;
    return (
      <div
        className={S.node + " " + (this.props.isSelected ? S.nodeSelected : "")}
        onClick={this.handleRowClick}
      >
        <div className={S.nodeLabel} style={{paddingLeft: this.getIndent()}}>
          {this.props.node.label}
        </div>
        {this.props.node.isFolder ? (
          <div className={S.noSelect + " " + S.expander} onClick={this.handleCaretClick}>
            <i
              className={cx({"fas fa-caret-right": !isExpanded, "fas fa-caret-down": isExpanded})}
            />
          </div>
        ) : (
          ""
        )}
      </div>
    );
  }
}
