import * as React from "react";
import { TextEditor } from "../editors/Text";
import { DateTimeEditor } from "../editors/DateTime";
import { BoolEditor } from "../editors/Bool";
import { observer } from "mobx-react";
import { ITableField } from "src/presenter/types/ITableViewPresenter/ITableField";


/*
interface IEditorField {
  isReadOnly: boolean;
  isInvalid: boolean;
  isLoading: boolean;
}

type IField = IEditorField & ICellTypeDU;*/

@observer
export class Editor extends React.Component<{ field: ITableField}> {
  getEditor(field: ITableField) {
    switch (field.type) {
      case "TextCell":
        return (
          <TextEditor
            value={field.value}
            isReadOnly={field.isReadOnly}
            isInvalid={field.isInvalid}
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
          />
        );
      case "BoolCell":
        return (
          <BoolEditor
            value={field.value}
            isReadOnly={field.isReadOnly}
            // onChange={field.onChange}
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
