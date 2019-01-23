import * as React from "react";
import { observer, inject } from "mobx-react";
import { action, computed } from "mobx";
import { IProperty } from "../../screenInterpreter/types";
import { StringFormEditor } from "../../cells/string/FormEditor";

import { IDataCursorState } from "src/Grid/types2";
import { BooleanFormEditor } from "src/cells/boolean/FormEditor";

interface IFormFieldProps {
  property: IProperty;
  dataCursorState?: IDataCursorState;
}

@inject("dataCursorState")
@observer
export class FormField extends React.Component<IFormFieldProps> {
  private elmRoot: HTMLDivElement | null;

  /*public componentDidMount() {
    window.addEventListener("click", this.handleWindowClick);
  }

  public componentWillUnmount() {
    window.removeEventListener("click", this.handleWindowClick);
  }

  @action.bound private handleWindowClick(event: any) {
    const gridPaneBacking = this.props.gridPaneBacking;
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
    const gridPaneBacking = this.props.gridPaneBacking;
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
  }*/

  @action.bound private refRoot(elm: HTMLDivElement) {
    this.elmRoot = elm;
  }

  private getEditor(): React.ReactNode {
    const { property } = this.props;
    const stdProps = {
      value: "def",
      editingRecordId: this.props.dataCursorState!.selectedRecordId,
      editingFieldId: this.props.property.id
    };
    switch (property.column) {
      case "Text":
        return <StringFormEditor {...stdProps} />;
      case "CheckBox":
        return <BooleanFormEditor {...stdProps} />;
      case "Currency":
      case "Date":
      case "ComboBox":
      default:
        return null;
    }
  }

  public render() {
    /* const gridPaneBacking = this.props.gridPaneBacking as IGridPanelBacking;
    const {
      gridInteractionSelectors,
      gridCursorView,
      gridInteractionActions,
      gridSetup,
      gridTopology
    } = gridPaneBacking;*/
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
    /*const fieldIndex = gridTopology.getColumnIndexById(id);
    const rowIndex = gridTopology.getRowIndexById(
      gridCursorView.selectedRowId!
    );*/

    if (!Number.isInteger(x!) || !Number.isInteger(y!)) {
      return null;
    }
    let captionLocation;
    if (captionPosition === "Left") {
      captionLocation = {
        left: x! - captionLength,
        top: y,
        width: captionLength,
        minHeight: 20 // this.props.h,
      };
    } else if (captionPosition === "Top") {
      captionLocation = {
        left: x,
        top: y! - 20,
        width: captionLength,
        minHeight: 20 // this.props.h,
      };
    } else {
      captionLocation = {
        left: x! + (entity === "Boolean" ? h! : w!), // + this.props.captionLength,
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
          // onClick={this.handleFieldClick}
          ref={this.refRoot}
        >
          {this.getEditor()}
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
