import { TagInputEditor } from "gui/Components/ScreenElements/Editors/TagInputEditor";
import { TextEditor } from "gui/Components/ScreenElements/Editors/TextEditor";
import { inject, observer, Provider } from "mobx-react";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";
import { onFieldKeyDown } from "model/actions-ui/DataView/TableView/onFieldKeyDown";
import { getRowStateAllowUpdate } from "model/selectors/RowState/getRowStateAllowUpdate";
import { getRowStateColumnBgColor } from "model/selectors/RowState/getRowStateColumnBgColor";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import React from "react";
import { onFieldBlur } from "../../../../model/actions-ui/DataView/TableView/onFieldBlur";
import { IProperty } from "../../../../model/entities/types/IProperty";
import { getSelectedRow } from "../../../../model/selectors/DataView/getSelectedRow";
import { getCellValue } from "../../../../model/selectors/TablePanelView/getCellValue";
import { getSelectedProperty } from "../../../../model/selectors/TablePanelView/getSelectedProperty";
import { BoolEditor } from "../../../Components/ScreenElements/Editors/BoolEditor";
import { DateTimeEditor } from "../../../Components/ScreenElements/Editors/DateTimeEditor";
import { DropdownEditor } from "../../../Components/ScreenElements/Editors/DropdownEditor";
import { NumberEditor } from "gui/Components/ScreenElements/Editors/NumberEditor";
import { BlobEditor } from "gui/Components/ScreenElements/Editors/BlobEditor";
import { XmlBuildDropdownEditor } from "../../../../modules/Editors/DropdownEditor/DropdownEditor";

@inject(({ tablePanelView }) => {
  const row = getSelectedRow(tablePanelView)!;
  const property = getSelectedProperty(tablePanelView)!;
  return {
    property,
    getCellValue: () => getCellValue(tablePanelView, row, property),
    onChange: (event: any, value: any) =>
      onFieldChange(tablePanelView)(event, row, property, value),
    onEditorBlur: (event: any) => onFieldBlur(tablePanelView)(event),
    onEditorKeyDown: (event: any) => onFieldKeyDown(tablePanelView)(event),
  };
})
@observer
export class TableViewEditor extends React.Component<{
  property?: IProperty;
  getCellValue?: () => any;
  onChange?: (event: any, value: any) => void;
  onEditorBlur?: (event: any) => void;
  onEditorKeyDown?: (event: any) => void;
}> {
  getEditor() {
    const rowId = getSelectedRowId(this.props.property);
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
      !getRowStateAllowUpdate(this.props.property, rowId || "", this.props.property!.id);

    switch (this.props.property!.column) {
      case "Number":
        return (
          <NumberEditor
            value={this.props.getCellValue!()}
            isReadOnly={readOnly}
            isInvalid={false}
            isFocused={true}
            isPassword={this.props.property!.isPassword}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            customNumberFormat={this.props.property!.customNumericFormat}
            refocuser={undefined}
            onChange={this.props.onChange}
            onKeyDown={this.props.onEditorKeyDown}
            onClick={undefined}
            onEditorBlur={this.props.onEditorBlur}
          />
        );
      case "Text":
        return (
          <TextEditor
            value={this.props.getCellValue!()}
            isReadOnly={readOnly}
            isInvalid={false}
            isFocused={true}
            isPassword={this.props.property!.isPassword}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            refocuser={undefined}
            onChange={this.props.onChange}
            onKeyDown={this.props.onEditorKeyDown}
            onClick={undefined}
            onEditorBlur={this.props.onEditorBlur}
            isRichText={false}
          />
        );
      case "Date":
        return (
          <DateTimeEditor
            value={this.props.getCellValue!()}
            outputFormat={this.props.property!.formatterPattern}
            isReadOnly={readOnly}
            isInvalid={false}
            isFocused={true}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            refocuser={undefined}
            onChange={this.props.onChange}
            onClick={undefined}
            onEditorBlur={this.props.onEditorBlur}
            onKeyDown={this.props.onEditorKeyDown}
          />
        );
      case "CheckBox":
        return (
          <BoolEditor
            value={this.props.getCellValue!()}
            isReadOnly={readOnly}
            onChange={this.props.onChange}
            onClick={undefined}
            onKeyDown={this.props.onEditorKeyDown}
          />
        );
      case "ComboBox":
        return (
          <XmlBuildDropdownEditor
            key={this.props.property!.xmlNode.$iid}
            xmlNode={this.props.property!.xmlNode}
            isReadOnly={readOnly}
          />
        );
      case "Checklist":
        return "";
      case "TagInput":
        return (
          <TagInputEditor
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
      case "Blob":
        return <BlobEditor value={this.props.getCellValue!()} />;
      default:
        return "Unknown field";
    }
  }

  render() {
    return <Provider property={this.props.property}>{this.getEditor()}</Provider>;
  }
}
