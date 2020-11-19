import { observer } from "mobx-react";
import React from "react";
import { IDataView } from "../../../model/entities/types/IDataView";
import { computed, observable } from "mobx";
import S from "./TreeView.module.css";
import cx from "classnames";
import { isTreeDataTable, TreeDataTable } from "../../../model/entities/TreeDataTable";

@observer
export class TreeView extends React.Component<{ dataView: IDataView }> {
  constructor(props: Readonly<{ dataView: IDataView }>) {
    super(props);

    if (!isTreeDataTable(this.props.dataView.dataTable)) {
      throw new Error("TreeView requires TreeDataTable to work properly");
    }
  }

  @computed
  get nodes(){
    const nodes = this.props.dataView.dataTable.rows.map(
      (row) => new Node(this.getRowId(row), this.getLabel(row), row)
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

  onRowClick(node: Node) {
    this.props.dataView.selectRowById(node.id);
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
  @observable
  isExpanded: boolean = false;

  constructor(id: string, label: string, row: any[]) {
    this.id = id;
    this.label = label;
    this.row = row;
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
}> {
  getIndent() {
    return this.props.node.level * 20 + "px";
  }

  onExpandClick() {
    this.props.node.isExpanded = !this.props.node.isExpanded;
  }

  render() {
    const { isExpanded } = this.props.node;
    return (
      <div
        className={S.node + " " + (this.props.isSelected ? S.nodeSelected : "")}
        onClick={this.props.onRowClick}
      >
        <div className={S.nodeLabel} style={{ paddingLeft: this.getIndent() }}>
          {this.props.node.label}
        </div>
        {this.props.node.isFolder ? (
          <div className={S.noSelect + " " + S.expander} onClick={() => this.onExpandClick()}>
            <i
              className={cx({ "fas fa-caret-right": !isExpanded, "fas fa-caret-down": isExpanded })}
            />
          </div>
        ) : (
          ""
        )}
      </div>
    );
  }
}
