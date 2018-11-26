import * as React from "react";
import { observer, inject } from "mobx-react";
import { IGridPanelBacking } from "../../GridPanel/types";
import { GridViewType } from "src/Grid/types";
import { GridEditorMounter } from "src/cells/GridEditorMounter";
import { StringGridEditor } from "src/cells/string/GridEditor";
import { action, computed } from "mobx";

@inject("gridPaneBacking")
@observer
export class FormField extends React.Component<any> {
  private elmRoot: HTMLDivElement | null;

  public componentDidMount() {
    window.addEventListener("click", this.handleWindowClick);
  }

  public componentWillUnmount() {
    window.removeEventListener("click", this.handleWindowClick);
  }

  @action.bound private handleWindowClick(event: any) {
    const gridPaneBacking = this.props.gridPaneBacking as IGridPanelBacking;
    const {
      gridInteractionSelectors,
      gridCursorView,
      gridInteractionActions,
      gridSetup,
      gridTopology
    } = gridPaneBacking;
    if(!(this.elmRoot && this.elmRoot.contains(event.target))) {
      gridInteractionActions.handleFormOutsideClick(event);
    }
  }

  @action.bound
  private handleFieldClick(event: any) {
    const gridPaneBacking = this.props.gridPaneBacking as IGridPanelBacking;
    const {
      gridInteractionSelectors,
      gridCursorView,
      gridInteractionActions,
      gridSetup,
      gridTopology
    } = gridPaneBacking;
    const { property } = this.props;
    const { id } = property;
    event.stopPropagation();
    if (!this.isThisEditing) {
      gridInteractionActions.select(gridCursorView.selectedRowId!, id);
      gridInteractionActions.editSelectedCell();
    }
  }

  @computed
  private get isThisEditing() {
    const gridPaneBacking = this.props.gridPaneBacking as IGridPanelBacking;
    const {
      gridInteractionSelectors,
      gridCursorView,
      gridInteractionActions,
      gridSetup,
      gridTopology
    } = gridPaneBacking;
    const { property } = this.props;
    const { id } = property;
    return (
      gridCursorView.isCellEditing &&
      id === gridCursorView.editingColumnId &&
      gridInteractionSelectors.activeView === GridViewType.Form
    );
  }

  @action.bound private refRoot(elm: HTMLDivElement) {
    this.elmRoot = elm;
  }

  public render() {
    const gridPaneBacking = this.props.gridPaneBacking as IGridPanelBacking;
    const {
      gridInteractionSelectors,
      gridCursorView,
      gridInteractionActions,
      gridSetup,
      gridTopology
    } = gridPaneBacking;
    const { property } = this.props;
    const {
      x,
      y,
      w,
      h,
      captionPosition,
      captionLength,
      entity,
      name,
      id
    } = property;
    const fieldIndex = gridTopology.getColumnIndexById(id);
    const rowIndex = gridTopology.getRowIndexById(
      gridCursorView.selectedRowId!
    );

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
        left: x + (entity === "Boolean" ? h : w), // + this.props.captionLength,
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
            width: entity === "Boolean" ? h : w,
            height: h
          }}
          onClick={this.handleFieldClick}
          ref={this.refRoot}
        >
          {/*`Type: ${this.props.type} Name: ${this.props.name}, Id: ${
            this.props.id
          }`*/}
          {/*this.props.children*/}
          {/*this.props.name*/}
          {/*entity*/}
          {this.isThisEditing && (
            <GridEditorMounter cursorView={gridCursorView}>
              {this.isThisEditing && (
                <StringGridEditor
                  editingRecordId={gridCursorView.editingRowId!}
                  editingFieldId={gridCursorView.editingColumnId!}
                  value={gridCursorView.editingOriginalCellValue}
                  onKeyDown={gridInteractionActions.handleDumbEditorKeyDown}
                  onDataCommit={gridCursorView.handleDataCommit}
                />
              )}
            </GridEditorMounter>
          )}
          {!this.isThisEditing && gridSetup.getCellValue(rowIndex, fieldIndex)}
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
