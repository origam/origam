import * as React from "react";
import { IDataTableFieldStruct, IFieldId } from "../DataTable/types";
import {
  IFormCellRenderer,
  IFormSetup,
  IFormView,
  IFormTopology
} from "./types";
import { observer } from "mobx-react";

@observer
export class FormComponent extends React.Component<{
  fieldCount: number;
  cellRenderer: IFormCellRenderer;
  overlayElements: React.ReactNode | React.ReactNode[] | null;
}> {
  public render() {
    const { fieldCount, cellRenderer, overlayElements } = this.props;
    const fields = [];
    for (let i = 0; i < fieldCount; i++) {
      fields.push(cellRenderer({ fieldIndex: i }));
    }
    return (
      <div className="form-container">
        {fields}
        {overlayElements}
      </div>
    );
  }
}

@observer
export class FormFieldPositioner extends React.Component<{
  formSetup: IFormSetup;
  formTopology: IFormTopology;
  fieldIndex: number;
  onClick?: (event: any, field: { fieldId: IFieldId }) => void;
}> {
  public render() {
    const { formSetup, fieldIndex, onClick, formTopology } = this.props;
    return (
      <div
        className="form-cell-positioner"
        style={{
          top: formSetup.getCellTop(fieldIndex),
          left: formSetup.getCellLeft(fieldIndex),
          width: formSetup.getCellWidth(fieldIndex),
          height: formSetup.getCellHeight(fieldIndex)
        }}
        onClick={event =>
          onClick &&
          onClick(event, {
            fieldId: formTopology.getFieldIdByIndex(fieldIndex)!
          })
        }
      >
        {this.props.children}
      </div>
    );
  }
}

@observer
export class FormFieldLabel extends React.Component<{
  formSetup: IFormSetup;
  formView: IFormView;
  fieldIndex: number;
}> {
  public render() {
    const { formSetup, fieldIndex, formView } = this.props;
    return (
      <div
        className="form-cell-label-positioner"
        style={{
          top: formSetup.getCellTop(fieldIndex),
          left:
            formSetup.getCellLeft(fieldIndex) -
            formSetup.getLabelOffset(fieldIndex),
          width: formSetup.getCellWidth(fieldIndex),
          height: formSetup.getCellHeight(fieldIndex)
        }}
      >
        {formView.getFieldLabel(fieldIndex)}:
      </div>
    );
  }
}
