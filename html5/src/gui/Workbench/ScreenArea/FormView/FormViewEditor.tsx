import React from "react";
import { observer, inject } from "mobx-react";
import { IProperty } from "../../../../model/entities/types/IProperty";
import { IPropertyColumn } from "../../../../model/entities/types/IPropertyColumn";

import { DateTimeEditor } from "../../../Components/ScreenElements/Editors/DateTimeEditor";
import moment from "moment";
import { BoolEditor } from "../../../Components/ScreenElements/Editors/BoolEditor";
import { DropdownEditor } from "../../../Components/ScreenElements/Editors/DropdownEditor";
import { TextEditor } from "gui/Components/ScreenElements/Editors/TextEditor";
import { onFieldBlur } from "model/actions/DataView/TableView/onFieldBlur";

@inject(({ property, formPanelView }) => {
  return {
    property,
    onEditorBlur: (event: any) => onFieldBlur(formPanelView)(event)
  };
})
@observer
export class FormViewEditor extends React.Component<{
  value?: any;
  textualValue?: any;
  property?: IProperty;
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
            onChange={undefined}
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
            onChange={undefined}
            onClick={undefined}
          />
        );
      case "CheckBox":
        return (
          <BoolEditor
            value={this.props.value}
            isReadOnly={this.props.property!.readOnly}
            onChange={undefined}
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
            onItemSelect={undefined}
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
