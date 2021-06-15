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
import { onFieldBlur } from "../../../../model/actions-ui/DataView/TableView/onFieldBlur";
import { IProperty } from "../../../../model/entities/types/IProperty";
import { getSelectedRow } from "../../../../model/selectors/DataView/getSelectedRow";
import { getCellValue } from "../../../../model/selectors/TablePanelView/getCellValue";
import { getSelectedProperty } from "../../../../model/selectors/TablePanelView/getSelectedProperty";
import { BoolEditor } from "../../../Components/ScreenElements/Editors/BoolEditor";
import { DateTimeEditor } from "../../../Components/ScreenElements/Editors/DateTimeEditor";
import { NumberEditor } from "gui/Components/ScreenElements/Editors/NumberEditor";
import { BlobEditor } from "gui/Components/ScreenElements/Editors/BlobEditor";
import { XmlBuildDropdownEditor } from "../../../../modules/Editors/DropdownEditor/DropdownEditor";
import { getDataView } from "../../../../model/selectors/DataView/getDataView";
import uiActions from "../../../../model/actions-ui-tree";
import { isReadOnly } from "../../../../model/selectors/RowState/isReadOnly";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import {
  cellPaddingRightFirstCell,
  rowHeight,
} from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";
import { shadeHexColor } from "utils/colorUtils";
import { getRowStateRowBgColor } from "model/selectors/RowState/getRowStateRowBgColor";
import ColorEditor from "gui/Components/ScreenElements/Editors/ColorEditor";
import { flashColor2htmlColor, htmlColor2FlashColor } from "utils/flashColorFormat";
import {prepareForSortAndFilter} from "../../../../model/selectors/PortalSettings/getSortingConfig";

@inject(({ tablePanelView }) => {
  const row = getSelectedRow(tablePanelView)!;
  const property = getSelectedProperty(tablePanelView)!;
  const actualProperty =
    property.column === "Polymorph" ? property.getPolymophicProperty(row) : property;
  return {
    property: actualProperty,
    getCellValue: () => getCellValue(tablePanelView, row, actualProperty),
    onChange: (event: any, value: any) =>
      onFieldChange(tablePanelView)({
        event: event,
        row: row,
        property: actualProperty,
        value: value,
      }),
    onEditorBlur: (event: any) => onFieldBlur(tablePanelView)(event),
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
  onChange?: (event: any, value: any) => void;
  onEditorBlur?: (event: any) => void;
  onEditorKeyDown?: (event: any) => void;
}> {
  getEditor() {
    const rowId = getSelectedRowId(this.props.property);
    const foregroundColor = getRowStateForegroundColor(this.props.property, rowId || "");
    const dataView = getDataView(this.props.property);
    const readOnly =
      isReadOnly(this.props.property!, rowId) ||
      (dataView.orderProperty !== undefined &&
        this.props.property?.name === dataView.orderProperty.name);

    const customBackgroundColor = getRowStateRowBgColor(dataView, rowId);
    const backgroundColor = readOnly
      ? shadeHexColor(customBackgroundColor, -0.1)
      : customBackgroundColor;

    const isFirsColumn = getTablePanelView(dataView).firstColumn === this.props.property;

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
            reFocuser={undefined}
            onChange={this.props.onChange}
            onKeyDown={this.props.onEditorKeyDown}
            onClick={undefined}
            onDoubleClick={(event) => this.onDoubleClick(event)}
            onEditorBlur={this.props.onEditorBlur}
            customStyle={isFirsColumn ? { paddingRight: cellPaddingRightFirstCell - 1 + "px" } : {}}
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
            onDoubleClick={(event) => this.onDoubleClick(event)}
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
            outputFormatToShow={this.props.property!.modelFormatterPattern}
            isReadOnly={readOnly}
            isInvalid={false}
            isFocused={true}
            backgroundColor={backgroundColor}
            foregroundColor={foregroundColor}
            refocuser={undefined}
            onChange={this.props.onChange}
            onClick={undefined}
            onDoubleClick={(event) => this.onDoubleClick(event)}
            onEditorBlur={this.props.onEditorBlur}
            onKeyDown={this.props.onEditorKeyDown}
          />
        );
      case "CheckBox":
        return (
          <BoolEditor
            value={this.props.getCellValue!()}
            isReadOnly={readOnly}
            readOnlyNoGrey={true}
            isInvalid={false}
            onChange={this.props.onChange}
            onClick={undefined}
            onKeyDown={this.props.onEditorKeyDown}
            forceTakeFocus={true}
          />
        );
      case "ComboBox":
        return (
          <XmlBuildDropdownEditor
            key={this.props.property!.xmlNode.$iid}
            xmlNode={this.props.property!.xmlNode}
            onDoubleClick={(event) => this.onDoubleClick(event)}
            isReadOnly={readOnly}
            foregroundColor={foregroundColor}
            backgroundColor={backgroundColor}
            autoSort={this.props.property!.autoSort}
            onKeyDown={this.props.onEditorKeyDown}
            prepareForSortAndFilter={text => prepareForSortAndFilter(this.props.property, text)}
            subscribeToFocusManager={(input) => input.focus()} // will cause the editor to take focus after opening
          />
        );
      case "Checklist":
        return "";
      case "Color":
        return (
          <ColorEditor
            value={flashColor2htmlColor(this.props.getCellValue!()) || null}
            onChange={(value) => this.props.onChange?.(undefined, htmlColor2FlashColor(value))}
            onBlur={() => this.props.onEditorBlur?.(undefined)}
            onKeyDown={this.props.onEditorKeyDown}
            isReadOnly={readOnly}
            subscribeToFocusManager={(input) => input.focus()}
          />
        );
      case "TagInput":
        return (
          <div style={{ height: rowHeight * 5 + "px", backgroundColor: "white" }}>
            <XmlBuildDropdownEditor
              key={this.props.property!.xmlNode.$iid}
              xmlNode={this.props.property!.xmlNode}
              isReadOnly={readOnly}
              autoSort={this.props.property!.autoSort}
              prepareForSortAndFilter={text => prepareForSortAndFilter(this.props.property, text)}
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
            isInvalid={false}
            canUpload={true}
            onChange={this.props.onChange}
            onEditorBlur={this.props.onEditorBlur}
          />
        );
      case "Polymorph":
        console.warn(`Type of polymorphic column was not determined, no editor was rendered`); // eslint-disable-line no-console
        return "";
      default:
        console.warn(`Unknown column type "${this.props.property!.column}", no editor was rendered`) // eslint-disable-line no-console
        return "";
    }
  }

  onDoubleClick(event: any) {
    getTablePanelView(this.props.property).setEditing(false);
    const dataView = getDataView(this.props.property);
    if (!dataView.firstEnabledDefaultAction) {
      return;
    }
    uiActions.actions.onActionClick(dataView.firstEnabledDefaultAction)(
      event,
      dataView.firstEnabledDefaultAction
    );
  }

  render() {
    return <Provider property={this.props.property}>{this.getEditor()}</Provider>;
  }
}
