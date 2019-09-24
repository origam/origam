import { inject, observer, Provider } from "mobx-react";
import moment from "moment";
import React from "react";
import { getIsEditing } from "../../../../model/selectors/DataView/getIsEditing";
import { getSelectedColumnIndex } from "../../../../model/selectors/TablePanelView/getSelectedColumnIndex";
import { getSelectedRowIndex } from "../../../../model/selectors/TablePanelView/getSelectedRowIndex";
import { IProperty } from "../../../../model/entities/types/IProperty";
import { BoolEditor } from "../../../Components/ScreenElements/Editors/BoolEditor";
import { DateTimeEditor } from "../../../Components/ScreenElements/Editors/DateTimeEditor";
import { DropdownEditor } from "../../../Components/ScreenElements/Editors/DropdownEditor";

import { getSelectedProperty } from "../../../../model/selectors/TablePanelView/getSelectedProperty";
import { getDataView } from "../../../../model/selectors/DataView/getDataView";
import { getSelectedRow } from "../../../../model/selectors/DataView/getSelectedRow";
import {
  getCellValueByIdx,
  getCellValue
} from "../../../../model/selectors/TablePanelView/getCellValue";
import { TextEditor } from "gui/Components/ScreenElements/Editors/TextEditor";
import { onFieldBlur } from "../../../../model/actions/DataView/TableView/onFieldBlur";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { getRowStateBackgroundColor } from "model/selectors/RowState/getRowStateBackgroundColor";
import { getRowStateAllowUpdate } from "model/selectors/RowState/getRowStateAllowUpdate";

@inject(({ tablePanelView }) => {
  const row = getSelectedRow(tablePanelView)!;
  const property = getSelectedProperty(tablePanelView)!;
  const { onFieldChange } = getDataView(tablePanelView);
  return {
    property,
    getCellValue: () => getCellValue(tablePanelView, row, property),
    onChange: (event: any, value: any) =>
      onFieldChange(event, row, property, value),
    onEditorBlur: (event: any) => onFieldBlur(tablePanelView)(event)
  };
})
@observer
export class TableViewEditor extends React.Component<{
  property?: IProperty;
  getCellValue?: () => any;
  onChange?: (event: any, value: any) => void;
  onEditorBlur?: (event: any) => void;
}> {
  getEditor() {
    const rowId = getSelectedRowId(this.props.property);
    const foregroundColor = getRowStateForegroundColor(
      this.props.property,
      rowId || "",
      this.props.property!.id
    );
    const backgroundColor = getRowStateBackgroundColor(
      this.props.property,
      rowId || "",
      this.props.property!.id
    );
    const readOnly =
      this.props.property!.readOnly ||
      !getRowStateAllowUpdate(
        this.props.property,
        rowId || "",
        this.props.property!.id
      );

    switch (this.props.property!.column) {
      case "Number":
      case "Text":
        return (
          <TextEditor
            value={this.props.getCellValue!()}
            isReadOnly={readOnly}
            isInvalid={false}
            isFocused={false}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            refocuser={undefined}
            onChange={this.props.onChange}
            onKeyDown={undefined}
            onClick={undefined}
            onEditorBlur={this.props.onEditorBlur}
          />
        );
      case "Date":
        return (
          <DateTimeEditor
            value={this.props.getCellValue!()}
            outputFormat={"DD.MM.YYYY HH:mm"}
            isReadOnly={readOnly}
            isInvalid={false}
            isFocused={false}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            refocuser={undefined}
            onChange={this.props.onChange}
            onClick={undefined}
            onEditorBlur={this.props.onEditorBlur}
          />
        );
      case "CheckBox":
        return (
          <BoolEditor
            value={this.props.getCellValue!()}
            isReadOnly={readOnly}
            onChange={this.props.onChange}
            onClick={undefined}
            onKeyDown={undefined}
          />
        );
      case "ComboBox":
        return (
          <DropdownEditor
            value={this.props.getCellValue!()}
            // textualValue={""}
            isReadOnly={readOnly}
            isInvalid={false}
            isFocused={false}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            onTextChange={undefined}
            onItemSelect={this.props.onChange}
            onEditorBlur={this.props.onEditorBlur}
            // DataStructureEntityId={""}
            // ColumnNames={[]}
            // Property={""}
            // RowId={""}
            // LookupId={""}
            // menuItemId={""}
            // api={undefined}
          />
        );
      default:
        return "Unknown field";
    }
  }

  render() {
    return (
      <Provider property={this.props.property}>{this.getEditor()}</Provider>
    );
  }
}
