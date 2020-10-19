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
import {getDataView} from "../../../../model/selectors/DataView/getDataView";
import uiActions from "../../../../model/actions-ui-tree";
import {isReadOnly} from "../../../../model/selectors/RowState/isReadOnly";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import {rowHeight} from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";

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
    const dataView = getDataView(this.props.property);
    const readOnly = isReadOnly(this.props.property!, rowId) ||
      dataView.orderProperty != undefined && this.props.property?.name === dataView.orderProperty.name;

    switch (this.props.property!.column) {
      case "Number":
        return (
          <NumberEditor
            value={this.props.getCellValue!()}
            isReadOnly={readOnly}
            isInvalid={false}
            isFocused={true}
            isPassword={this.props.property!.isPassword}
            maxLength={this.props.property?.maxLength}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            customNumberFormat={this.props.property!.customNumericFormat}
            refocuser={undefined}
            onChange={this.props.onChange}
            onKeyDown={this.props.onEditorKeyDown}
            onClick={undefined}
            onDoubleClick={event => this.onDoubleClick(event)}
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
            maxLength={this.props.property?.maxLength}
            refocuser={undefined}
            onChange={this.props.onChange}
            onKeyDown={this.props.onEditorKeyDown}
            onClick={undefined}
            onDoubleClick={event => this.onDoubleClick(event)}
            onEditorBlur={this.props.onEditorBlur}
            isRichText={false}
            isMultiline={this.props.property!.multiline}
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
            onDoubleClick={event => this.onDoubleClick(event)}
            onEditorBlur={this.props.onEditorBlur}
            onKeyDown={this.props.onEditorKeyDown}
          />
        );
      case "CheckBox":
        return (
          <BoolEditor
            value={this.props.getCellValue!()}
            isReadOnly={readOnly}
            isInvalid={false}
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
            onDoubleClick={event => this.onDoubleClick(event)}
            isReadOnly={readOnly}
            onKeyDown={this.props.onEditorKeyDown}
          />
        );
      case "Checklist":
        return "";
      case "TagInput":
        return (
          <div style={{height: rowHeight * 5+"px", backgroundColor: "white"}}>
            <XmlBuildDropdownEditor
              key={this.props.property!.xmlNode.$iid}
              xmlNode={this.props.property!.xmlNode}
              isReadOnly={readOnly}
              tagEditor={
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
                  onDoubleClick={event => this.onDoubleClick(event)}
                  onEditorBlur={this.props.onEditorBlur}
                />
              }
            />
          </div>
        );
      case "Blob":
        return <BlobEditor
          value={this.props.getCellValue!()}
          isInvalid={false}/>;
      default:
        return "Unknown field";
    }
  }

  onDoubleClick(event: any){
    getTablePanelView(this.props.property).setEditing(false);
    const dataView = getDataView(this.props.property);
    if (!dataView.firstEnabledDefaultAction) {
      return;
    }
    uiActions.actions.onActionClick(dataView.firstEnabledDefaultAction)(event, dataView.firstEnabledDefaultAction);
  };


  render() {
    return <Provider property={this.props.property}>{this.getEditor()}</Provider>;
  }
}
