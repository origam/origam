/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { BlobEditor } from "gui/Components/ScreenElements/Editors/BlobEditor";
import { CheckList } from "gui/Components/ScreenElements/Editors/CheckList";
import { ImageEditor } from "gui/Components/ScreenElements/Editors/ImageEditor";
import { NumberEditor } from "gui/Components/ScreenElements/Editors/NumberEditor";
import { TagInputEditor } from "gui/Components/ScreenElements/Editors/TagInputEditor";
import { TextEditor } from "gui/Components/ScreenElements/Editors/TextEditor";
import { inject, observer } from "mobx-react";
import { onFieldBlur } from "model/actions-ui/DataView/TableView/onFieldBlur";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import React from "react";
import uiActions from "model/actions-ui-tree";
import { IDockType, IProperty } from "model/entities/types/IProperty";
import { getDataView } from "model/selectors/DataView/getDataView";
import { isReadOnly } from "model/selectors/RowState/isReadOnly";
import { XmlBuildDropdownEditor } from "modules/Editors/DropdownEditor/DropdownEditor";
import { BoolEditor } from "gui/Components/ScreenElements/Editors/BoolEditor";
import { DateTimeEditor } from "gui/Components/ScreenElements/Editors/DateTimeEditor/DateTimeEditor";
import { FormFocusManager } from "model/entities/FormFocusManager";
import { DomEvent } from "leaflet";
import { onDropdownEditorClick } from "model/actions/DropdownEditor/onDropdownEditorClick";
import { shadeHexColor } from "utils/colorUtils";
import { getIsFormScreenDirty } from "model/selectors/FormScreen/getisFormScreenDirty";
import { runGeneratorInFlowWithHandler, runInFlowWithHandler } from "utils/runInFlowWithHandler";
import ColorEditor from "gui/Components/ScreenElements/Editors/ColorEditor";
import { CellAlignment } from "gui/Components/ScreenElements/Table/TableRendering/cells/cellAlignment";
import {
  isAddRecordShortcut,
  isDeleteRecordShortcut,
  isDuplicateRecordShortcut, 
  isFilterRecordShortcut,
  isRefreshShortcut,
  isSaveShortcut
} from "utils/keyShortcuts";
import { onCreateRowClick } from "model/actions-ui/DataView/onCreateRowClick";
import { onEscapePressed } from "model/actions-ui/DataView/onEscapePressed";
import { flushCurrentRowData } from "model/actions/DataView/TableView/flushCurrentRowData";
import { onDeleteRowClick } from "model/actions-ui/DataView/onDeleteRowClick";
import { onCopyRowClick } from "model/actions-ui/DataView/onCopyRowClick";
import { onFilterButtonClick } from "model/actions-ui/DataView/onFilterButtonClick";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getIsAddButtonVisible } from "model/selectors/DataView/getIsAddButtonVisible";
import { getIsDelButtonVisible } from "model/selectors/DataView/getIsDelButtonVisible";
import { getIsCopyButtonVisible } from "model/selectors/DataView/getIsCopyButtonVisible";
import { getScreenActionButtonsState } from "model/actions-ui/ScreenToolbar/saveButtonVisible";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";
import { makeOnAddNewRecordClick, onSaveClick } from "gui/connections/NewRecordScreen";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { flashColor2htmlColor, htmlColor2FlashColor } from "utils/flashColorFormat";


@inject(({property, formPanelView}) => {
  return {
    property,
    onEditorBlur: () => onFieldBlur(formPanelView)(),
    onChange: async (event: any, value: any) => {
      const row = getSelectedRow(property);
      if(row === undefined){
        return;
      }
      await onFieldChange(formPanelView)({
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
  onChange?: (event: any, value: any) => Promise<void>;
  onEditorBlur?: () => Promise<any>;
  backgroundColor?: string;
  onTextOverflowChanged?: (tooltip: string | null | undefined) => void;
  dock?: IDockType;
}> {
  focusManager: FormFocusManager;

  constructor(props: any) {
    super(props);
    this.focusManager = getDataView(this.props.property).formFocusManager;
  }

  getEditor() {
    const property = this.props.property!;
    const rowId = getSelectedRowId(property);
    const row = getSelectedRow(property);
    const foregroundColor = getRowStateForegroundColor(property, rowId || "");
    const dataView = getDataView(property);
    const readOnly =
      !row ||
      isReadOnly(property, rowId) ||
      (dataView.orderProperty !== undefined && property?.name === dataView.orderProperty.name);
    const backgroundColor = readOnly
      ? shadeHexColor(this.props.backgroundColor, -0.1)
      : this.props.backgroundColor;

    switch (property.column) {
      case "Number":
        return (
          <NumberEditor
            id={"editor_" + property.modelInstanceId}
            value={this.props.value}
            isReadOnly={readOnly}
            isPassword={property.isPassword}
            property={property}
            maxLength={property.maxLength}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            customNumberFormat={property.customNumericFormat}
            customStyle={resolveNumericCellAlignment(property.style)}
            onChange={this.props.onChange}
            onKeyDown={this.makeOnKeyDownCallBack()}
            onEditorBlur={this.props.onEditorBlur}
            onTextOverflowChanged={this.props.onTextOverflowChanged}
            subscribeToFocusManager={(textEditor, onBlur) =>{
              this.focusManager.subscribe(
                textEditor,
                property.id,
                property.tabIndex,
                onBlur);
              }
            }
          />
        );
      case "Text":
        return (
          <TextEditor
            id={"editor_" + property.modelInstanceId}
            value={this.props.value}
            isReadOnly={readOnly}
            isMultiline={property.multiline}
            isAllowTab={property.isAllowTab}
            isPassword={property.isPassword}
            customStyle={property.style}
            maxLength={property.maxLength}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            onChange={this.props.onChange}
            onKeyDown={this.makeOnKeyDownCallBack()}
            wrapText={true}
            onEditorBlur={this.props.onEditorBlur}
            isRichText={this.props.isRichText}
            onTextOverflowChanged={this.props.onTextOverflowChanged}
            subscribeToFocusManager={(textEditor) =>
              this.focusManager.subscribe(
                textEditor,
                property.id,
                property.tabIndex,
                this.props.onEditorBlur
              )
            }
            dock={this.props.dock}
          />
        );
      case "Date":
        return (
          <DateTimeEditor
            value={this.props.value}
            outputFormat={property.formatterPattern}
            outputFormatToShow={property.modelFormatterPattern}
            isReadOnly={readOnly}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            onChange={this.props.onChange}
            onEditorBlur={this.props.onEditorBlur}
            subscribeToFocusManager={(textEditor, onBlur) =>{
              this.focusManager.subscribe(
                textEditor,
                property.id,
                property.tabIndex,
                onBlur);
            }
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
            onClick={event => this.focusManager.stopAutoFocus()}
            onKeyDown={undefined}
            subscribeToFocusManager={(textEditor) =>
              this.focusManager.subscribe(
                textEditor,
                property.id,
                property.tabIndex
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
                property.id,
                property.tabIndex
              )
            }
            autoSort={property.autoSort}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            customStyle={property.style}
            newRecordScreen={this.props.property?.lookup?.newRecordScreen}
            onAddNewRecordClick={makeOnAddNewRecordClick(this.props.property!)}
            isLink={property.isLink}
            onClick={(event) => {
              onDropdownEditorClick(property)(event, property, row);
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
                property.id,
                property.tabIndex
              )
            }
            autoSort={property.autoSort}
            tagEditor={
              <TagInputEditor
                value={this.props.value}
                isReadOnly={readOnly}
                backgroundColor={backgroundColor}
                foregroundColor={foregroundColor}
                customStyle={property.style}
                onChange={this.props.onChange}
                onKeyDown={this.makeOnKeyDownCallBack()}
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
            isReadonly={readOnly}
            subscribeToFocusManager={(firstCheckInput) =>
              this.focusManager.subscribe(
                firstCheckInput,
                property.id,
                property.tabIndex
              )
            }
            onKeyDown={this.makeOnKeyDownCallBack()}
            onClick={() => this.focusManager.stopAutoFocus()}
          />
        );
      case "Color":
        return (
          <ColorEditor
            value={flashColor2htmlColor(this.props.value) || null}
            onChange={(value) => this.props.onChange?.(undefined, htmlColor2FlashColor(value))}
            onBlur={() => this.props.onEditorBlur?.()}
            onKeyDown={this.makeOnKeyDownCallBack()}
            isReadOnly={readOnly}
            subscribeToFocusManager={(textEditor) =>
              this.focusManager.subscribe(
                textEditor,
                property.id,
                property.tabIndex
              )
            }
          />
        );
      case "Image":
        return <ImageEditor value={this.props.value}/>;
      case "Blob":
        const isDirty = getIsFormScreenDirty(property);
        return (
          <BlobEditor
            isReadOnly={readOnly}
            value={this.props.value}
            onKeyDown={this.makeOnKeyDownCallBack()}
            canUpload={!isDirty}
            subscribeToFocusManager={(inputEditor) =>
              this.focusManager.subscribe(
                inputEditor,
                property.id,
                property.tabIndex
              )
            }
            onChange={this.props.onChange}
            onEditorBlur={this.props.onEditorBlur}
          />
        );
      case "Polymorph":
        console.warn(`Type of polymorphic column was not determined, no editor was rendered`); // eslint-disable-line no-console
        return "";
      default:
        console.warn(`Unknown column type "${property.column}", no editor was rendered` // eslint-disable-line no-console
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
          dataView.formFocusManager.stopAutoFocus();
          const openedScreen = getOpenedScreen(this.props.property);
          if (event.key === "Tab") {
            DomEvent.preventDefault(event);
            if (event.shiftKey) {
              this.focusManager.focusPrevious(document.activeElement);
            } else {
              this.focusManager.focusNext(document.activeElement);
            }
            return;
          }
          if (isSaveShortcut(event)) {
            await runGeneratorInFlowWithHandler({
              ctx: dataView,
              generator: function*() {
                yield*flushCurrentRowData(dataView)();
                if (getScreenActionButtonsState(dataView)?.isSaveButtonVisible) {
                  const formScreenLifecycle = getFormScreenLifecycle(dataView);
                  yield*formScreenLifecycle.onSaveSession();
                }
              }()
            });
            return;
          }
          if (isRefreshShortcut(event) && getScreenActionButtonsState(dataView)?.isRefreshButtonVisible) {
            await runGeneratorInFlowWithHandler({
              ctx: dataView,
              generator: function*() {
                yield*flushCurrentRowData(dataView)();
                const formScreenLifecycle = getFormScreenLifecycle(dataView);
                yield*formScreenLifecycle.onRequestScreenReload();
              }()
            });
            return;
          }
          if (isAddRecordShortcut(event) && getIsAddButtonVisible(dataView)) {
            await onCreateRowClick(dataView)(event);
            return;
          }
          if (isDeleteRecordShortcut(event) && getIsDelButtonVisible(dataView)) {
            await onDeleteRowClick(dataView)(event);
            return;
          }
          if (isDuplicateRecordShortcut(event) && getIsCopyButtonVisible(dataView)) {
            await onCopyRowClick(dataView)(event);
            return;
          }
          if (isFilterRecordShortcut(event) && !dataView.isHeadless) {
            await onFilterButtonClick(dataView)(event);
            return;
          }
          if (event.key === "Escape") {
            if (openedScreen.isNewRecordScreen){
              onScreenTabCloseClick(openedScreen)(event, true);
              return;
            }
            await onEscapePressed(dataView, event);
            return;
          }
          if (this.props.property!.multiline) {
            return;
          }
          if (event.key === "Enter") {
            const updateExecuted = await this.props.onEditorBlur?.();
            if (openedScreen.isNewRecordScreen){
              await onSaveClick(openedScreen);
              return;
            }
            if (!updateExecuted && dataView.firstEnabledDefaultAction) {
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

export function resolveNumericCellAlignment(customStyle: { [p: string]: string } | undefined) {
  let cellAlignment = new CellAlignment(false, "Number", customStyle);
  const style = customStyle ? Object.assign({}, customStyle) : {};
  style["paddingLeft"] = cellAlignment.paddingLeft + "px";
  style["textAlign"] = cellAlignment.alignment;
  return style;
}
