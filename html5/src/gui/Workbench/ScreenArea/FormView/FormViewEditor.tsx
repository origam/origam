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

@inject(({ property, formPanelView }) => {
  const row = getSelectedRow(formPanelView)!;
  const { onFieldChange } = getDataView(formPanelView);
  return {
    property,
    onEditorBlur: (event: any) => onFieldBlur(formPanelView)(event),
    onChange: (event: any, value: any) =>
      onFieldChange(event, row, property, value)
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
    switch (this.props.property!.column) {
      case "Number":
      case "Text":
        return (
          <TextEditor
            value={this.props.value}
            isReadOnly={this.props.property!.readOnly}
            isInvalid={false}
            isFocused={false}
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
            isReadOnly={this.props.property!.readOnly}
            isInvalid={false}
            isFocused={false}
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
            isReadOnly={this.props.property!.readOnly}
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
            isReadOnly={this.props.property!.readOnly}
            isInvalid={false}
            isFocused={false}
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
