import React from "react";
import { observer } from "mobx-react";
import { TextEditor } from "./editors/Text";
import { DateTimeEditor } from "./editors/DateTime";
import { BoolEditor } from "./editors/Bool";
import { ITypedField } from "../../../view/Perspectives/types";
import { DropdownEditor } from "./editors/Dropdown";

// TODO: Refactor - this duplicates with Editor in FormView/FormView - Editor
@observer
export class Editor extends React.Component<{ field: ITypedField }> {
  getEditor(field: ITypedField) {
    switch (field.type) {
      case "TextCell":
        return (
          <TextEditor
            value={field.value}
            isReadOnly={field.isReadOnly}
            isInvalid={field.isInvalid}
            isFocused={field.isFocused}
            onChange={field.onChange}
          />
        );
      case "DateTimeCell":
        return (
          <DateTimeEditor
            value={field.value}
            inputFormat={field.inputFormat}
            outputFormat={field.outputFormat}
            isReadOnly={field.isReadOnly}
            isInvalid={field.isInvalid}
            isFocused={field.isFocused}
            onChange={field.onChange}
          />
        );
      case "BoolCell":
        return (
          <BoolEditor
            value={field.value}
            isReadOnly={field.isReadOnly}
            onChange={field.onChange}
          />
        );
      case "DropdownCell":
        return (
          <DropdownEditor
            value={field.value}
            textualValue={field.textualValue}
            isReadOnly={field.isReadOnly}
            isInvalid={field.isInvalid}
            isFocused={field.isFocused}
            onTextChange={field.onTextChange}
            onItemSelect={field.onItemSelect}
            DataStructureEntityId={field.DataStructureEntityId}
            ColumnNames={field.ColumnNames}
            Property={field.Property}
            RowId={field.RowId}
            LookupId={field.LookupId}
            menuItemId={field.menuItemId}
            api={field.api}
          />
        );
      default:
        return field.value;
    }
  }

  render() {
    return this.getEditor(this.props.field);
  }
}
