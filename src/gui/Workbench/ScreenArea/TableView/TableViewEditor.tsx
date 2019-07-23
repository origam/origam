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
import { TextEditor } from "../../../Components/ScreenElements/Editors/TextEditor";
import { getSelectedProperty } from "../../../../model/selectors/TablePanelView/getSelectedProperty";
import { getDataView } from "../../../../model/selectors/DataView/getDataView";
import { getSelectedRow } from "../../../../model/selectors/DataView/getSelectedRow";
import {
  getCellValueByIdx,
  getCellValue
} from "../../../../model/selectors/TablePanelView/getCellValue";

@inject(({ tablePanelView }) => {
  const row = getSelectedRow(tablePanelView)!;
  const property = getSelectedProperty(tablePanelView)!;
  const { onFieldChange } = getDataView(tablePanelView);
  return {
    property,
    getCellValue: () => getCellValue(tablePanelView, row, property),
    onChange: (event: any, value: any) =>
      onFieldChange(event, row, property, value)
  };
})
@observer
export class TableViewEditor extends React.Component<{
  property?: IProperty;
  getCellValue?: () => any;
  onChange?: (event: any, value: any) => void;
}> {
  getEditor() {
    switch (this.props.property!.column) {
      case "Number":
      case "Text":
        return (
          <TextEditor
            value={this.props.getCellValue!()}
            isReadOnly={this.props.property!.readOnly}
            isInvalid={false}
            isFocused={false}
            refocuser={undefined}
            onChange={this.props.onChange}
            onKeyDown={undefined}
            onClick={undefined}
          />
        );
      case "Date":
        return (
          <DateTimeEditor
            value={this.props.getCellValue!()}
            outputFormat={"DD.MM.YYYY HH:mm"}
            isReadOnly={this.props.property!.readOnly}
            isInvalid={false}
            isFocused={false}
            refocuser={undefined}
            onChange={this.props.onChange}
            onClick={undefined}
          />
        );
      case "CheckBox":
        return (
          <BoolEditor
            value={this.props.getCellValue!()}
            isReadOnly={this.props.property!.readOnly}
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
            isReadOnly={this.props.property!.readOnly}
            isInvalid={false}
            isFocused={false}
            onTextChange={undefined}
            onItemSelect={this.props.onChange}
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
