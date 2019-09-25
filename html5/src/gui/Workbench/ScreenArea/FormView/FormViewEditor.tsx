import { TextEditor } from "gui/Components/ScreenElements/Editors/TextEditor";
import { inject, observer } from "mobx-react";
import { onFieldBlur } from "model/actions/DataView/TableView/onFieldBlur";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import React from "react";
import { IProperty } from "../../../../model/entities/types/IProperty";
import { BoolEditor } from "../../../Components/ScreenElements/Editors/BoolEditor";
import { DateTimeEditor } from "../../../Components/ScreenElements/Editors/DateTimeEditor";
import { DropdownEditor } from "../../../Components/ScreenElements/Editors/DropdownEditor";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { getRowStateColumnBgColor } from "model/selectors/RowState/getRowStateColumnBgColor";
import { getRowStateAllowUpdate } from "model/selectors/RowState/getRowStateAllowUpdate";
import { getCellValue } from "model/selectors/TablePanelView/getCellValue";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";
import { getDataSourceFieldByIndex } from "model/selectors/DataSources/getDataSourceFieldByIndex";
import { onFieldChange } from "model/actions/DataView/TableView/onFieldChange";

@inject(({ property, formPanelView }) => {
  const row = getSelectedRow(formPanelView)!;
  return {
    property,
    onEditorBlur: (event: any) => onFieldBlur(formPanelView)(event),
    onChange: (event: any, value: any) =>
      onFieldChange(formPanelView)(event, row, property, value)
  };
})
@observer
export class FormViewEditor extends React.Component<{
  value?: any;
  textualValue?: any;
  property?: IProperty;
  onChange?: (event: any, value: any) => void;
  onEditorBlur?: (event: any) => void;
}> {
  getEditor() {
    const rowId = getSelectedRowId(this.props.property);
    const row = getSelectedRow(this.props.property);
    const foregroundColor = getRowStateForegroundColor(
      this.props.property,
      rowId || "",
      this.props.property!.id
    );
    const backgroundColor = getRowStateColumnBgColor(
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
    let isInvalid = false;
    let invalidMessage: string | undefined = undefined;
    if (row) {
      const dataView = getDataTable(this.props.property);
      const dsFieldErrors = getDataSourceFieldByName(
        this.props.property,
        "__Errors"
      );
      const errors = dsFieldErrors
        ? dataView.getCellValueByDataSourceField(row, dsFieldErrors)
        : null;

      const errMap: Map<number, string> | undefined = errors
        ? new Map(
            Object.entries<string>(errors.fieldErrors).map(
              ([dsIndexStr, errMsg]: [string, string]) => [
                parseInt(dsIndexStr, 10),
                errMsg
              ]
            )
          )
        : undefined;

      const errMsg =
        dsFieldErrors && errMap
          ? errMap.get(this.props.property!.dataSourceIndex)
          : undefined;
      if (errMsg) {
        isInvalid = true;
        invalidMessage = errMsg;
      }
    }

    switch (this.props.property!.column) {
      case "Number":
      case "Text":
        return (
          <TextEditor
            value={this.props.value}
            isReadOnly={readOnly}
            isInvalid={isInvalid}
            invalidMessage={invalidMessage}
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
            value={this.props.value}
            outputFormat={"DD.MM.YYYY HH:mm"}
            isReadOnly={readOnly}
            isInvalid={isInvalid}
            invalidMessage={invalidMessage}
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
            value={this.props.value}
            isReadOnly={readOnly}
            onChange={this.props.onChange}
            onClick={undefined}
            onKeyDown={undefined}
          />
        );
      case "ComboBox":
        return (
          <DropdownEditor
            value={this.props.value}
            textualValue={this.props.textualValue}
            isReadOnly={readOnly}
            isInvalid={isInvalid}
            invalidMessage={invalidMessage}
            isFocused={false}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            onTextChange={undefined}
            onItemSelect={this.props.onChange}
            DataStructureEntityId={""}
            ColumnNames={[]}
            Property={""}
            RowId={""}
            LookupId={""}
            menuItemId={""}
            api={undefined}
            onEditorBlur={this.props.onEditorBlur}
          />
        );
      default:
        return "Unknown field";
    }
  }

  render() {
    return this.getEditor();
  }
}
