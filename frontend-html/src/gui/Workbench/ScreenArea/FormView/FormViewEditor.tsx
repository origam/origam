import {BlobEditor} from "gui/Components/ScreenElements/Editors/BlobEditor";
import {CheckList} from "gui/Components/ScreenElements/Editors/CheckList";
import {ImageEditor} from "gui/Components/ScreenElements/Editors/ImageEditor";
import {NumberEditor} from "gui/Components/ScreenElements/Editors/NumberEditor";
import {TagInputEditor} from "gui/Components/ScreenElements/Editors/TagInputEditor";
import {TextEditor} from "gui/Components/ScreenElements/Editors/TextEditor";
import {inject, observer} from "mobx-react";
import {onFieldBlur} from "model/actions-ui/DataView/TableView/onFieldBlur";
import {onFieldChange} from "model/actions-ui/DataView/TableView/onFieldChange";
import {getFieldErrorMessage} from "model/selectors/DataView/getFieldErrorMessage";
import {getSelectedRow} from "model/selectors/DataView/getSelectedRow";
import {getRowStateForegroundColor} from "model/selectors/RowState/getRowStateForegroundColor";
import {getSelectedRowId} from "model/selectors/TablePanelView/getSelectedRowId";
import React from "react";
import uiActions from "model/actions-ui-tree";
import {IProperty} from "model/entities/types/IProperty";
import {getDataView} from "model/selectors/DataView/getDataView";
import {isReadOnly} from "model/selectors/RowState/isReadOnly";
import {XmlBuildDropdownEditor} from "modules/Editors/DropdownEditor/DropdownEditor";
import {BoolEditor} from "gui/Components/ScreenElements/Editors/BoolEditor";
import {DateTimeEditor} from "gui/Components/ScreenElements/Editors/DateTimeEditor";
import {FocusManager} from "model/entities/FocusManager";
import {DomEvent} from "leaflet";
import {onDropdownEditorClick} from "model/actions/DropdownEditor/onDropdownEditorClick";
import {shadeHexColor} from "utils/colorUtils";
import {getIsFormScreenDirty} from "model/selectors/FormScreen/getisFormScreenDirty";
import {runInFlowWithHandler} from "utils/runInFlowWithHandler";
import ColorEditor from "gui/Components/ScreenElements/Editors/ColorEditor";
import {flashColor2htmlColor, htmlColor2FlashColor} from "utils/flashColorFormat";


@inject(({ property, formPanelView }) => {
  const row = getSelectedRow(formPanelView)!;
  return {
    property,
    onEditorBlur: (event: any) => onFieldBlur(formPanelView)(event),
    onChange: (event: any, value: any) => {
      onFieldChange(formPanelView)({
        event: event,
        row: row,
        property: property,
        value: value,
      });
    },
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
  onEditorBlur?: (event: any) => Promise<any>;
  backgroundColor?: string;
  onTextOverflowChanged?: (toolTip: string | null | undefined) => void;
}> {
  focusManager: FocusManager;

  constructor(props: any) {
    super(props);
    this.focusManager = getDataView(this.props.property).focusManager;
  }

  getEditor() {
    const rowId = getSelectedRowId(this.props.property);
    const row = getSelectedRow(this.props.property);
    const foregroundColor = getRowStateForegroundColor(this.props.property, rowId || "");
    const readOnly = !row || isReadOnly(this.props.property!, rowId);
    const backgroundColor = readOnly
      ? shadeHexColor(this.props.backgroundColor, -0.1)
      : this.props.backgroundColor;
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
            isPassword={this.props.property!.isPassword}
            invalidMessage={invalidMessage}
            property={this.props.property}
            isFocused={false}
            maxLength={this.props.property?.maxLength}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            customNumberFormat={this.props.property!.customNumericFormat}
            customStyle={this.props.property?.style}
            reFocuser={undefined}
            onChange={this.props.onChange}
            onKeyDown={this.makeOnKeyDownCallBack()}
            onClick={undefined}
            onEditorBlur={this.props.onEditorBlur}
            onTextOverflowChanged={this.props.onTextOverflowChanged}
            subscribeToFocusManager={(textEditor) =>
              this.focusManager.subscribe(
                textEditor,
                this.props.property?.id,
                this.props.property?.tabIndex
              )
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
            onKeyDown={this.makeOnKeyDownCallBack()}
            onClick={undefined}
            wrapText={true}
            onEditorBlur={this.props.onEditorBlur}
            isRichText={this.props.isRichText}
            onTextOverflowChanged={this.props.onTextOverflowChanged}
            subscribeToFocusManager={(textEditor) =>
              this.focusManager.subscribe(
                textEditor,
                this.props.property?.id,
                this.props.property?.tabIndex
              )
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
              this.focusManager.subscribe(
                textEditor,
                this.props.property?.id,
                this.props.property?.tabIndex
              )
            }
            onKeyDown={this.makeOnKeyDownCallBack()}
          />
        );
      case "CheckBox":
        return (
          <BoolEditor
            value={this.props.value}
            isReadOnly={readOnly}
            onChange={this.props.onChange}
            onClick={() => getDataView(this.props.property).focusManager.stopAutoFocus()}
            isInvalid={isInvalid}
            invalidMessage={invalidMessage}
            onKeyDown={undefined}
            subscribeToFocusManager={(textEditor) =>
              this.focusManager.subscribe(
                textEditor,
                this.props.property?.id,
                this.props.property?.tabIndex
              )
            }
          />
        );
      case "ComboBox":
        return (
          <XmlBuildDropdownEditor
            key={this.props.xmlNode.$iid}
            onTextOverflowChanged={this.props.onTextOverflowChanged}
            xmlNode={this.props.xmlNode}
            isReadOnly={readOnly}
            subscribeToFocusManager={(textEditor) =>
              this.focusManager.subscribe(
                textEditor,
                this.props.property?.id,
                this.props.property?.tabIndex
              )
            }
            autoSort={this.props.property?.autoSort}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            customStyle={this.props.property?.style}
            isInvalid={isInvalid}
            invalidMessage={invalidMessage}
            isLink={this.props.property?.isLink}
            onClick={(event) => {
              onDropdownEditorClick(this.props.property)(event, this.props.property, row);
            }}
            onKeyDown={this.makeOnKeyDownCallBack()}
          />
        );
      case "TagInput":
        return (
          <XmlBuildDropdownEditor
            key={this.props.xmlNode.$iid}
            xmlNode={this.props.xmlNode}
            isReadOnly={readOnly}
            subscribeToFocusManager={(firstCheckInput) =>
              this.focusManager.subscribe(
                firstCheckInput,
                this.props.property?.id,
                this.props.property?.tabIndex
              )
            }
            autoSort={this.props.property?.autoSort}
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
                onKeyDown={this.makeOnKeyDownCallBack()}
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
            isReadonly={readOnly}
            invalidMessage={invalidMessage}
            subscribeToFocusManager={(firstCheckInput) =>
              this.focusManager.subscribe(
                firstCheckInput,
                this.props.property?.id,
                this.props.property?.tabIndex
              )
            }
            onKeyDown={this.makeOnKeyDownCallBack()}
            onClick={() => getDataView(this.props.property).focusManager.stopAutoFocus()}
          />
        );
      case "Color":
        return (
          <ColorEditor
            value={flashColor2htmlColor(this.props.value) || null}
            onChange={(value) => this.props.onChange?.(undefined, htmlColor2FlashColor(value))}
            onBlur={() => this.props.onEditorBlur?.(undefined)}
            onKeyDown={this.makeOnKeyDownCallBack()}
            isReadOnly={readOnly}
            subscribeToFocusManager={(textEditor) =>
              this.focusManager.subscribe(
                textEditor,
                this.props.property?.id,
                this.props.property?.tabIndex
              )
            }
          />
        );
      case "Image":
        return <ImageEditor value={this.props.value} />;
      case "Blob":
        const isDirty = getIsFormScreenDirty(this.props.property);
        return (
          <BlobEditor
            isReadOnly={readOnly}
            value={this.props.value}
            isInvalid={isInvalid}
            invalidMessage={invalidMessage}
            onKeyDown={this.makeOnKeyDownCallBack()}
            canUpload={!isDirty}
            subscribeToFocusManager={(inputEditor) =>
              this.focusManager.subscribe(
                inputEditor,
                this.props.property?.id,
                this.props.property?.tabIndex
              )
            }
            onChange={this.props.onChange}
            onEditorBlur={this.props.onEditorBlur}
          />
        );
      case "Polymorph":
        console.warn(`Type of polymorphic column was not determined, no editor was rendered`);
        return "";
      default:
        console.warn(
          `Unknown column type "${this.props.property!.column}", no editor was rendered`
        );
        return "";
    }
  }

  private makeOnKeyDownCallBack() {
    const dataView = getDataView(this.props.property);

    return (event: any) => {
      runInFlowWithHandler({
        ctx: this.props.property,
        action: async () => {
          dataView.focusManager.stopAutoFocus();
          if (event.key === "Tab") {
            DomEvent.preventDefault(event);
            if (event.shiftKey) {
              this.focusManager.focusPrevious(document.activeElement);
            } else {
              this.focusManager.focusNext(document.activeElement);
            }
            return;
          }
          if (this.props.property!.multiline) {
            return;
          }
          if (event.key === "Enter") {
            await this.props.onEditorBlur?.(null);
            if (dataView.firstEnabledDefaultAction) {
              uiActions.actions.onActionClick(dataView.firstEnabledDefaultAction)(
                event,
                dataView.firstEnabledDefaultAction
              );
            }
          }
        },
      });
    };
  }

  render() {
    return this.getEditor();
  }
}
