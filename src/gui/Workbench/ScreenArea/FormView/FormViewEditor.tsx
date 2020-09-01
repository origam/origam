import { TagInputEditor } from "gui/Components/ScreenElements/Editors/TagInputEditor";
import { TextEditor } from "gui/Components/ScreenElements/Editors/TextEditor";
import { NumberEditor } from "gui/Components/ScreenElements/Editors/NumberEditor";
import { inject, observer } from "mobx-react";
import { onFieldBlur } from "model/actions-ui/DataView/TableView/onFieldBlur";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getRowStateAllowUpdate } from "model/selectors/RowState/getRowStateAllowUpdate";
import { getRowStateColumnBgColor } from "model/selectors/RowState/getRowStateColumnBgColor";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import React from "react";
import { IProperty } from "../../../../model/entities/types/IProperty";
import { BoolEditor } from "../../../Components/ScreenElements/Editors/BoolEditor";
import { DateTimeEditor } from "../../../Components/ScreenElements/Editors/DateTimeEditor";
// import {DropdownEditor} from "../../../Components/ScreenElements/Editors/DropdownEditor";
import { CheckList } from "gui/Components/ScreenElements/Editors/CheckList";
import { ImageEditor } from "gui/Components/ScreenElements/Editors/ImageEditor";
import { BlobEditor } from "gui/Components/ScreenElements/Editors/BlobEditor";
import { getDataView } from "../../../../model/selectors/DataView/getDataView";
import uiActions from "../../../../model/actions-ui-tree";
import { XmlBuildDropdownEditor } from "../../../../modules/Editors/DropdownEditor/DropdownEditor";
import {isReadOnly} from "../../../../model/selectors/RowState/isReadOnly";

@inject(({ property, formPanelView }) => {
  const row = getSelectedRow(formPanelView)!;
  return {
    property,
    onEditorBlur: (event: any) => onFieldBlur(formPanelView)(event),
    onChange: (event: any, value: any) => onFieldChange(formPanelView)(event, row, property, value),
  };
})
@observer
export class FormViewEditor extends React.Component<{
  xmlNode?: any;
  value?: any;
  textualValue?: any;
  tabIndex?: number;
  property?: IProperty;
  isRichText: boolean;
  onChange?: (event: any, value: any) => void;
  onEditorBlur?: (event: any) => void;
}> {
  getEditor() {
    const rowId = getSelectedRowId(this.props.property);
    const row = getSelectedRow(this.props.property);
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
    const readOnly = isReadOnly(this.props.property!, rowId);
    let isInvalid = false;
    let invalidMessage: string | undefined = undefined;
    if (row) {
      const dataTable = getDataTable(this.props.property);
      const dsFieldErrors = getDataSourceFieldByName(this.props.property, "__Errors");
      const errors = dsFieldErrors
        ? dataTable.getCellValueByDataSourceField(row, dsFieldErrors)
        : null;

      const errMap: Map<number, string> | undefined = errors
        ? new Map(
            Object.entries<string>(
              errors.fieldErrors
            ).map(([dsIndexStr, errMsg]: [string, string]) => [parseInt(dsIndexStr, 10), errMsg])
          )
        : undefined;

      const errMsg =
        dsFieldErrors && errMap ? errMap.get(this.props.property!.dataSourceIndex) : undefined;
      if (errMsg) {
        isInvalid = true;
        invalidMessage = errMsg;
      }
    }

    const focusManager = getDataView(this.props.property).focusManager;

    switch (this.props.property!.column) {
      case "Number":
        return (
          <NumberEditor
            value={this.props.value}
            isReadOnly={readOnly}
            isInvalid={isInvalid}
            isMultiline={this.props.property!.multiline}
            isPassword={this.props.property!.isPassword}
            invalidMessage={invalidMessage}
            isFocused={false}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            customNumberFormat={this.props.property!.customNumericFormat}
            customStyle={this.props.property?.style}
            refocuser={undefined}
            onChange={this.props.onChange}
            onKeyDown={this.MakeOnKeyDownCallBack()}
            onClick={undefined}
            onEditorBlur={this.props.onEditorBlur}
            subscribeToFocusManager={(textEditor) =>
              focusManager.subscribe(textEditor, this.props.property?.id)
            }
            tabIndex={this.props.tabIndex}
          />
        );
      case "Text":
        return (
          <TextEditor
            value={this.props.value}
            isReadOnly={readOnly}
            isInvalid={isInvalid}
            isMultiline={this.props.property!.multiline}
            isPassword={this.props.property!.isPassword}
            customStyle={this.props.property?.style}
            invalidMessage={invalidMessage}
            isFocused={false}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            refocuser={undefined}
            onChange={this.props.onChange}
            onKeyDown={this.MakeOnKeyDownCallBack()}
            onClick={undefined}
            onEditorBlur={this.props.onEditorBlur}
            isRichText={this.props.isRichText}
            subscribeToFocusManager={(textEditor) =>
              focusManager.subscribe(textEditor, this.props.property?.id)
            }
            tabIndex={this.props.tabIndex}
          />
        );
      case "Date":
        return (
          <DateTimeEditor
            value={this.props.value}
            outputFormat={this.props.property!.formatterPattern}
            isReadOnly={readOnly}
            isInvalid={isInvalid}
            invalidMessage={invalidMessage}
            isFocused={false}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            refocuser={undefined}
            onChange={this.props.onChange}
            onClick={undefined}
            onEditorBlur={this.props.onEditorBlur}
            subscribeToFocusManager={(textEditor) =>
              focusManager.subscribe(textEditor, this.props.property?.id)
            }
            onKeyDown={this.MakeOnKeyDownCallBack()}
            tabIndex={this.props.tabIndex}
          />
        );
      case "CheckBox":
        return (
          <BoolEditor
            value={this.props.value}
            isReadOnly={readOnly}
            onChange={this.props.onChange}
            onClick={undefined}
            onKeyDown={undefined}
            tabIndex={this.props.tabIndex}
            subscribeToFocusManager={(textEditor) =>
              focusManager.subscribe(textEditor, this.props.property?.id)
            }
          />
        );
      case "ComboBox":
        return (
          <XmlBuildDropdownEditor
            key={this.props.xmlNode.$iid}
            xmlNode={this.props.xmlNode}
            isReadOnly={readOnly}
            subscribeToFocusManager={(textEditor) =>
              focusManager.subscribe(textEditor, this.props.property?.id)
            }
            tabIndex={this.props.tabIndex}
          />
        );
      case "TagInput":
        return (
          <XmlBuildDropdownEditor
            key={this.props.xmlNode.$iid}
            xmlNode={this.props.xmlNode}
            isReadOnly={readOnly}
            tabIndex={this.props.tabIndex}
            tagEditor={
              <TagInputEditor
                value={this.props.value}
                isReadOnly={readOnly}
                isInvalid={isInvalid}
                invalidMessage={invalidMessage}
                isFocused={false}
                backgroundColor={backgroundColor}
                foregroundColor={foregroundColor}
                refocuser={undefined}
                onChange={this.props.onChange}
                onKeyDown={this.MakeOnKeyDownCallBack()}
                onClick={undefined}
                onEditorBlur={this.props.onEditorBlur}
              />
            }
          />
        );
      case "Checklist":
        return (
          <CheckList
            value={this.props.value}
            onChange={(newValue) => this.props.onChange && this.props.onChange({}, newValue)}
            tabIndex={this.props.tabIndex}
            subscribeToFocusManager={(firstCheckInput) =>
              focusManager.subscribe(firstCheckInput, this.props.property?.id)
            }
          />
        );
      case "Image":
        return <ImageEditor
          value={this.props.value}
          tabIndex={this.props.tabIndex}/>;
      case "Blob":
        return <BlobEditor
          value={this.props.value}
          tabIndex={this.props.tabIndex}
          subscribeToFocusManager={(inputEditor) =>
            focusManager.subscribe(inputEditor, this.props.property?.id)
          }
        />;
      default:
        return "Unknown field";
    }
  }

  private MakeOnKeyDownCallBack() {
    const dataView = getDataView(this.props.property);

    return (event: any) => {
      if (this.props.property!.multiline) {
        return;
      }
      if (!dataView.defaultAction) {
        return;
      }
      if (event.key === "Enter") {
        uiActions.actions.onActionClick(dataView.defaultAction)(event, dataView.defaultAction);
      }
    };
  }

  render() {
    return this.getEditor();
  }
}
