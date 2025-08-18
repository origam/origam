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

import { TagInputEditor } from "gui/Components/ScreenElements/Editors/TagInputEditor";
import { TextEditor } from "gui/Components/ScreenElements/Editors/TextEditor";
import { inject, observer, Provider } from "mobx-react";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";
import { onFieldKeyDown } from "model/actions-ui/DataView/TableView/onFieldKeyDown";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import React from "react";
import { onFieldBlur } from "model/actions-ui/DataView/TableView/onFieldBlur";
import { IProperty } from "model/entities/types/IProperty";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getCellValue } from "model/selectors/TablePanelView/getCellValue";
import { getSelectedProperty } from "model/selectors/TablePanelView/getSelectedProperty";
import { BoolEditor } from "gui/Components/ScreenElements/Editors/BoolEditor";
import { DateTimeEditor } from "gui/Components/ScreenElements/Editors/DateTimeEditor/DateTimeEditor";
import { NumberEditor } from "gui/Components/ScreenElements/Editors/NumberEditor";
import { BlobEditor } from "gui/Components/ScreenElements/Editors/BlobEditor";
import { XmlBuildDropdownEditor } from "modules/Editors/DropdownEditor/DropdownEditor";
import { getDataView } from "model/selectors/DataView/getDataView";
import uiActions from "../../../../model/actions-ui-tree";
import { isReadOnly } from "model/selectors/RowState/isReadOnly";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { rowHeight } from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";
import { shadeHexColor } from "utils/colorUtils";
import { getRowStateRowBgColor } from "model/selectors/RowState/getRowStateRowBgColor";
import ColorEditor from "gui/Components/ScreenElements/Editors/ColorEditor";
import { getGridFocusManager } from "model/entities/GridFocusManager";
import { resolveCellAlignment } from "gui/Workbench/ScreenArea/TableView/ResolveCellAlignment";
import S from "./TableViewEditor.module.scss";
import { makeOnAddNewRecordClick } from "gui/connections/NewRecordScreen";
import { getKeyBuffer } from "model/selectors/getKeyBuffer";
import { flashColor2htmlColor, htmlColor2FlashColor } from "utils/flashColorFormat";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { onDropdownEditorClick } from "model/actions/DropdownEditor/onDropdownEditorClick";

@inject(({tablePanelView}) => {
  const row = getSelectedRow(tablePanelView)!;
  const property = getSelectedProperty(tablePanelView)!;
  const actualProperty =
    property.column === "Polymorph" ? property.getPolymophicProperty(row) : property;
  return {
    property: actualProperty,
    getCellValue: () => getCellValue(tablePanelView, row, actualProperty),
    onChange: async (event: any, value: any) =>
      await onFieldChange(tablePanelView)({
        event: event,
        row: row,
        property: actualProperty,
        value: value,
      }),
    onEditorBlur: async (event: any) => {
      const gridFocusManager = getGridFocusManager(tablePanelView);
      if(!event?.target || gridFocusManager.activeEditor === event.target){
        gridFocusManager.activeEditor = undefined;
        gridFocusManager.editorBlur = undefined;
      }
      await onFieldBlur(tablePanelView)();
    },
    onEditorKeyDown: (event: any) => {
      event.persist();
      onFieldKeyDown(tablePanelView)(event);
    },
  };
})
@observer
export class TableViewEditor extends React.Component<{
  property?: IProperty;
  getCellValue?: () => any;
  onChange?: (event: any, value: any) => Promise<void>;
  onEditorBlur?: (event: any) => Promise<void>;
  onEditorKeyDown?: (event: any) => void;
  expand?: boolean
}> {

  componentDidMount() {
    const focusManager = getGridFocusManager(this.props.property);
    focusManager.focusEditor();
  }

  getEditor() {
    const property = this.props.property!;
    const rowId = getSelectedRowId(property);
    const row = getSelectedRow(property);
    const foregroundColor = getRowStateForegroundColor(property, rowId || "");
    const dataView = getDataView(property);
    const tablePanelView = getTablePanelView(property);
    const readOnly =
      isReadOnly(property!, rowId) ||
      (dataView.orderProperty !== undefined &&
        property.name === dataView.orderProperty.name);

    const customBackgroundColor = getRowStateRowBgColor(dataView, rowId);
    const backgroundColor = readOnly
      ? shadeHexColor(customBackgroundColor, -0.1)
      : customBackgroundColor;

    const isFirstColumn = getTablePanelView(dataView)?.firstColumn === property;
    const gridFocusManager = getGridFocusManager(property);
    const portalSettings = getWorkbenchLifecycle(property).portalSettings;
    switch (property.column) {
      case "Number":
        return (
          <NumberEditor
            value={this.props.getCellValue!()}
            isReadOnly={readOnly}
            property={property}
            isPassword={property.isPassword}
            maxLength={property.maxLength}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            customNumberFormat={property.customNumericFormat}
            onChange={this.props.onChange}
            onKeyDown={this.props.onEditorKeyDown}
            onClick={undefined}
            onMount={(onChange) => this.onEditorMount(onChange)}
            onDoubleClick={(event) => this.onDoubleClick(event)}
            onEditorBlur={this.props.onEditorBlur}
            customStyle={resolveCellAlignment(property.style, isFirstColumn, "Number")}
            subscribeToFocusManager={(editor, onBlur) =>{
                gridFocusManager.activeEditor = editor
                gridFocusManager.editorBlur = onBlur;
              }
            }
          />
        );
      case "Text":
        return (
          <TextEditor
            id={"editor_" + property.modelInstanceId}
            value={this.props.getCellValue!()}
            isReadOnly={readOnly}
            isPassword={property.isPassword}
            isAllowTab={false}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            maxLength={property.maxLength}
            onChange={this.props.onChange}
            onMount={(onChange) => this.onEditorMount(onChange)}
            onKeyDown={this.props.onEditorKeyDown}
            onClick={undefined}
            wrapText={false}
            customStyle={resolveCellAlignment(property.style, isFirstColumn, "Text")}
            onDoubleClick={(event) => this.onDoubleClick(event)}
            onEditorBlur={this.props.onEditorBlur}
            isRichText={false}
            isMultiline={property.multiline}
            subscribeToFocusManager={(editor) =>
            {
              gridFocusManager.activeEditor = editor
              gridFocusManager.editorBlur = this.props.onEditorBlur;
            }}
          />
        );
      case "Date":
        return (
          <DateTimeEditor
            value={this.props.getCellValue!()}
            outputFormat={property.formatterPattern}
            outputFormatToShow={property.modelFormatterPattern}
            isReadOnly={readOnly}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            onChange={this.props.onChange}
            onClick={undefined}
            onMount={(onChange) => this.onEditorMount(onChange)}
            onDoubleClick={(event) => this.onDoubleClick(event)}
            onEditorBlur={this.props.onEditorBlur}
            onKeyDown={this.props.onEditorKeyDown}
            subscribeToFocusManager={(editor, onBlur) =>
            {
              gridFocusManager.activeEditor = editor;
              gridFocusManager.editorBlur = onBlur;
            }}
          />
        );
      case "CheckBox":
        return (
          <BoolEditor
            value={this.props.getCellValue!()}
            isReadOnly={readOnly}
            readOnlyNoGrey={true}
            onChange={this.props.onChange}
            onClick={undefined}
            onKeyDown={this.props.onEditorKeyDown}
            subscribeToFocusManager={(editor) =>
              gridFocusManager.activeEditor = editor
            }
          />
        );
      case "ComboBox":
        return (
          <XmlBuildDropdownEditor
            key={property.xmlNode.$iid}
            xmlNode={property.xmlNode}
            onDoubleClick={(event) => this.onDoubleClick(event)}
            isReadOnly={readOnly}
            customStyle={resolveCellAlignment(property.style, isFirstColumn, "Text")}
            foregroundColor={foregroundColor}
            backgroundColor={backgroundColor}
            onMount={(onChange) => this.onEditorMount(onChange)}
            onBlur={(target: any)=>{
              const gridFocusManager = getGridFocusManager(dataView);
              if(gridFocusManager.activeEditor === target){
                gridFocusManager.activeEditor = undefined;
                gridFocusManager.editorBlur = undefined;
              }
            }}
            expandAfterMounting={this.props.expand}
            newRecordScreen={property.lookup?.newRecordScreen}
            onAddNewRecordClick={makeOnAddNewRecordClick(property)}
            autoSort={property.autoSort}
            onKeyDown={this.props.onEditorKeyDown}
            subscribeToFocusManager={(editor) =>
              gridFocusManager.activeEditor = editor
            }
            typingDelayMillis={portalSettings?.dropDownTypingDebouncingDelayMilliseconds}
            isLink={property.isLink}
            onClick={(event) => {
              tablePanelView.setEditing(false);
              onDropdownEditorClick(property)(event, property, row);
            }}
          />
        );
      case "Checklist":
        return "";
      case "Color":
        return (
          <ColorEditor
            value={flashColor2htmlColor(this.props.getCellValue!()) || null}
            onChange={(value) => this.props.onChange?.(undefined, htmlColor2FlashColor(value))}
            onBlur={(event: any) => this.props.onEditorBlur?.(event)}
            onKeyDown={this.props.onEditorKeyDown}
            isReadOnly={readOnly}
            subscribeToFocusManager={(editor) =>
              gridFocusManager.activeEditor = editor
            }
          />
        );
      case "TagInput":
        return (
          <div style={{height: rowHeight * 5 + "px", backgroundColor: "white"}}>
            <XmlBuildDropdownEditor
              key={property.xmlNode.$iid}
              xmlNode={property.xmlNode}
              isReadOnly={readOnly}
              autoSort={property.autoSort}
              onMount={(onChange) => this.onEditorMount(onChange)}
              subscribeToFocusManager={(editor) =>
                gridFocusManager.activeEditor = editor
              }
              tagEditor={
                <TagInputEditor
                  value={this.props.getCellValue!()}
                  isReadOnly={readOnly}
                  backgroundColor={backgroundColor}
                  foregroundColor={foregroundColor}
                  onChange={this.props.onChange}
                  onKeyDown={this.props.onEditorKeyDown}
                  onDoubleClick={(event) => this.onDoubleClick(event)}
                  onEditorBlur={this.props.onEditorBlur}
                />
              }
            />
          </div>
        );
      case "Blob":
        return (
          <BlobEditor
            isReadOnly={readOnly}
            value={this.props.getCellValue!()}
            canUpload={true}
            onChange={this.props.onChange}
            onEditorBlur={this.props.onEditorBlur}
            subscribeToFocusManager={(editor) =>
              gridFocusManager.activeEditor = editor
            }
          />
        );
      case "Polymorph":
        console.warn(`Type of polymorphic column was not determined, no editor was rendered`); // eslint-disable-line no-console
        return "";
      default:
        console.warn(`Unknown column type "${property.column}", no editor was rendered`) // eslint-disable-line no-console
        return "";
    }
  }

  async onEditorMount(onChange?: (value: any) => void) {
    const keyBuffer = getKeyBuffer(this.props.property);
    const bufferedText = keyBuffer.getAndClear();
    if (bufferedText !== "" && this.props.onChange) {
      onChange?.(bufferedText);
    }
  }

  onDoubleClick(event: any) {
    const dataView = getDataView(this.props.property);
    if (!dataView.firstEnabledDefaultAction) {
      return;
    }
    getTablePanelView(this.props.property).setEditing(false);
    uiActions.actions.onActionClick(dataView.firstEnabledDefaultAction)(
      event,
      dataView.firstEnabledDefaultAction
    );
  }

  render() {
    const dataView = getDataView(this.props.property);
    return <Provider property={this.props.property}>
      {
        <div
          id={"editor_dataView_" + dataView.modelInstanceId}
          className={S.container}
        >
          {this.getEditor()}
        </div>
      }
    </Provider>;
  }
}
