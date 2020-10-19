import { BlobEditor } from "gui/Components/ScreenElements/Editors/BlobEditor";
import { CheckList } from "gui/Components/ScreenElements/Editors/CheckList";
import { ImageEditor } from "gui/Components/ScreenElements/Editors/ImageEditor";
import { NumberEditor } from "gui/Components/ScreenElements/Editors/NumberEditor";
import { TagInputEditor } from "gui/Components/ScreenElements/Editors/TagInputEditor";
import { TextEditor } from "gui/Components/ScreenElements/Editors/TextEditor";
import { inject, observer } from "mobx-react";
import { onFieldBlur } from "model/actions-ui/DataView/TableView/onFieldBlur";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";
import { getFieldErrorMessage } from "model/selectors/DataView/getFieldErrorMessage";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getRowStateColumnBgColor } from "model/selectors/RowState/getRowStateColumnBgColor";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import React from "react";
import uiActions from "model/actions-ui-tree";
import { IProperty } from "model/entities/types/IProperty";
import { getDataView } from "model/selectors/DataView/getDataView";
import { isReadOnly } from "model/selectors/RowState/isReadOnly";
import { XmlBuildDropdownEditor } from "modules/Editors/DropdownEditor/DropdownEditor";
import { BoolEditor } from "gui/Components/ScreenElements/Editors/BoolEditor";
import { DateTimeEditor } from "gui/Components/ScreenElements/Editors/DateTimeEditor";
import {FocusManager} from "model/entities/FocusManager";
import {DomEvent} from "leaflet";


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
  property?: IProperty;
  isRichText: boolean;
  onChange?: (event: any, value: any) => void;
  onEditorBlur?: (event: any) => void;
}> {

  focusManager: FocusManager;

  constructor(props: any){
    super(props);
    this.focusManager = getDataView(this.props.property).focusManager;
  }

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
    const readOnly = !row || isReadOnly(this.props.property!, rowId);
    let isInvalid = false;
    let invalidMessage: string | undefined = undefined;

    const errMsg = row
      ? getFieldErrorMessage(this.props.property!)(row, this.props.property!)
      : undefined;

    if (errMsg) {
      isInvalid = true;
      invalidMessage = errMsg;
    }

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
              this.focusManager.subscribe(textEditor, this.props.property?.id, this.props.property?.tabIndex)
            }
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
            maxLength={this.props.property?.maxLength}
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
              this.focusManager.subscribe(textEditor, this.props.property?.id, this.props.property?.tabIndex)
            }
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
              this.focusManager.subscribe(textEditor, this.props.property?.id, this.props.property?.tabIndex)
            }
            onKeyDown={this.MakeOnKeyDownCallBack()}
          />
        );
      case "CheckBox":
        return (
          <BoolEditor
            value={this.props.value}
            isReadOnly={readOnly}
            onChange={this.props.onChange}
            onClick={undefined}
            isInvalid={isInvalid}
            invalidMessage={invalidMessage}
            onKeyDown={undefined}
            subscribeToFocusManager={(textEditor) =>
              this.focusManager.subscribe(textEditor, this.props.property?.id, this.props.property?.tabIndex)
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
              this.focusManager.subscribe(textEditor, this.props.property?.id, this.props.property?.tabIndex)
            }
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            customStyle={this.props.property?.style}
            isInvalid={isInvalid}
            invalidMessage={invalidMessage}
            onKeyDown={this.MakeOnKeyDownCallBack()}
          />
        );
      case "TagInput":
        return (
          <XmlBuildDropdownEditor
            key={this.props.xmlNode.$iid}
            xmlNode={this.props.xmlNode}
            isReadOnly={readOnly}
            subscribeToFocusManager={(firstCheckInput) =>
              this.focusManager.subscribe(firstCheckInput, this.props.property?.id, this.props.property?.tabIndex)
            }
            tagEditor={
              <TagInputEditor
                value={this.props.value}
                isReadOnly={readOnly}
                isInvalid={isInvalid}
                invalidMessage={invalidMessage}
                isFocused={false}
                backgroundColor={backgroundColor}
                foregroundColor={foregroundColor}
                customStyle={this.props.property?.style}
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
            isInvalid={isInvalid}
            invalidMessage={invalidMessage}
            subscribeToFocusManager={(firstCheckInput) =>
              this.focusManager.subscribe(firstCheckInput, this.props.property?.id, this.props.property?.tabIndex)
            }
            onKeyDown={this.MakeOnKeyDownCallBack()}
          />
        );
      case "Image":
        return <ImageEditor value={this.props.value} />;
      case "Blob":
        return (
          <BlobEditor
            value={this.props.value}
            isInvalid={isInvalid}
            invalidMessage={invalidMessage}
            onKeyDown={this.MakeOnKeyDownCallBack()}
            subscribeToFocusManager={(inputEditor) =>
              this.focusManager.subscribe(inputEditor, this.props.property?.id, this.props.property?.tabIndex)
            }
          />
        );
      default:
        return "Unknown field";
    }
  }

  private MakeOnKeyDownCallBack() {
    const dataView = getDataView(this.props.property);

    return (event: any) => {
      console.log("event.key: "+event.key)
      if (event.key === "Tab") {
        DomEvent.preventDefault(event);
        if(event.shiftKey){
          this.focusManager.focusPrevious(document.activeElement);
        }
        else{
          this.focusManager.focusNext(document.activeElement);
        }
        return;
      }

      if (this.props.property!.multiline) {
        return;
      }
      if (!dataView.firstEnabledDefaultAction) {
        return;
      }
      if (event.key === "Enter") {
          uiActions.actions.onActionClick(dataView.firstEnabledDefaultAction)(event, dataView.firstEnabledDefaultAction);
      }
    };
  }

  render() {
    return this.getEditor();
  }
}
