import React from "react";
import { observer } from "mobx-react";
import { TextEditor } from "./editors/Text";
import { DateTimeEditor } from "./editors/DateTime";
import { BoolEditor } from "./editors/Bool";
import { ITypedField } from "../../../view/Perspectives/types";

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
      default:
        return field.value;
    }
  }

  render() {
    return this.getEditor(this.props.field);
  }
}
