import { inject, observer } from "mobx-react";
import moment from "moment";
import React from "react";
import { getIsEditing } from "../../../../model/selectors/DataView/getIsEditing";
import { getSelectedColumnIndex } from "../../../../model/selectors/TablePanelView/getSelectedColumnIndex";
import { getSelectedRowIndex } from "../../../../model/selectors/TablePanelView/getSelectedRowIndex";
import { IProperty } from "../../../../model/types/IProperty";
import { BoolEditor } from "../../../Components/ScreenElements/Editors/BoolEditor";
import { DateTimeEditor } from "../../../Components/ScreenElements/Editors/DateTimeEditor";
import { DropdownEditor } from "../../../Components/ScreenElements/Editors/DropdownEditor";
import { TextEditor } from "../../../Components/ScreenElements/Editors/TextEditor";
import { getSelectedProperty } from "../../../../model/selectors/TablePanelView/getSelectedProperty";

@inject(({ tablePanelView }) => {
  return {
    property: getSelectedProperty(tablePanelView)
  };
})
@observer
export class TableViewEditor extends React.Component<{
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
