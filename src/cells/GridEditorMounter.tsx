import * as React from "react";
import { observer } from "mobx-react";
import { observable } from "mobx";
import { IGridCursorView } from "../Grid/types";

@observer
export class GridEditorMounter extends React.Component<{
  cursorView: IGridCursorView;
}> {
  constructor(props: any) {
    super(props);
    this.setNewRenderer();
  }

  private oldEditingColumnId: string | undefined;
  private oldEditingRowId: string | undefined;
  private renderer: React.StatelessComponent;

  private setNewRenderer() {
    this.renderer = (props => props.children) as React.StatelessComponent;
  }

  private getRenderer() {
    const { cursorView } = this.props;
    if (
      cursorView.editingColumnId !== this.oldEditingColumnId ||
      cursorView.editingRowId !== this.oldEditingRowId
    ) {
      this.setNewRenderer();
    }
    this.oldEditingColumnId = cursorView.editingColumnId;
    this.oldEditingRowId = cursorView.editingRowId;
    return this.renderer;
  }

  public render() {
    if (this.props.cursorView.isCellEditing) {
      return React.createElement(this.getRenderer(), {}, this.props.children);
    } else {
      return null;
    }
  }
}
