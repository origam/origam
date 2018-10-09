import * as React from "react";
import { IDataTableFieldStruct } from "../DataTable/types";
import { IFormCellRenderer, IFormSetup } from "./types";

export class FormComponent extends React.Component<{
  fieldCount: number;
  cellRenderer: IFormCellRenderer;
}> {
  public render() {
    const { fieldCount, cellRenderer } = this.props;
    const fields = [];
    for (let i = 0; i < fieldCount; i++) {
      fields.push(cellRenderer({ fieldIndex: i }));
    }
    return <div className="form-container">{fields}</div>;
  }
}

export class FormFieldPositioner extends React.Component<{
  formSetup: IFormSetup;
  fieldIndex: number;
}> {
  public render() {
    const { formSetup, fieldIndex } = this.props;
    return (
      <div
        className="form-cell-positioner"
        style={{
          top: formSetup.getCellTop(fieldIndex),
          left: formSetup.getCellLeft(fieldIndex),
          width: formSetup.getCellWidth(fieldIndex),
          height: formSetup.getCellHeight(fieldIndex)
        }}
      >
        {this.props.children}
      </div>
    );
  }
}

export class FormFieldLabel extends React.Component<{
  formSetup: IFormSetup;
  fieldIndex: number;
}> {
  public render() {
    const { formSetup, fieldIndex } = this.props;
    return (
      <div
        className="form-cell-label-positioner"
        style={{
          top: formSetup.getCellTop(fieldIndex),
          left: formSetup.getCellLeft(fieldIndex) - formSetup.getLabelOffset(fieldIndex) ,
          width: formSetup.getCellWidth(fieldIndex),
          height: formSetup.getCellHeight(fieldIndex)
        }}
      >
        {formSetup.getFieldLabel(fieldIndex)}:
      </div>
    );
  }
}
