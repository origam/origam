import React from "react";
import { observer, inject } from "mobx-react";
import { IProperty } from "../../../../model/types/IProperty";
import { IPropertyColumn } from "../../../../model/types/IPropertyColumn";
import { TextEditor } from "../../../Components/ScreenElements/Editors/TextEditor";
import { DateTimeEditor } from "../../../Components/ScreenElements/Editors/DateTimeEditor";
import moment from "moment";
import { BoolEditor } from "../../../Components/ScreenElements/Editors/BoolEditor";
import { DropdownEditor } from "../../../Components/ScreenElements/Editors/DropdownEditor";

@inject(({ property }) => {
  return { property };
})
@observer
export class FormViewEditor extends React.Component<{
  property?: IProperty;
}> {
  getEditor() {
    switch (this.props.property!.column) {
      case "Number":
      case "Text":
        return (
          <TextEditor
            value={""}
            isReadOnly={false}
            isInvalid={false}
            isFocused={false}
            refocuser={undefined}
            onChange={undefined}
            onKeyDown={undefined}
            onClick={undefined}
          />
        );
      case "Date":
        return (
          <DateTimeEditor
            value={moment().toISOString()}
            outputFormat={"DD.MM.YYYY HH:mm"}
            isReadOnly={false}
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
            value={true}
            isReadOnly={false}
            onChange={undefined}
            onClick={undefined}
            onKeyDown={undefined}
          />
        );
      case "ComboBox":
        return (
          <DropdownEditor
            value={""}
            textualValue={""}
            isReadOnly={false}
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
