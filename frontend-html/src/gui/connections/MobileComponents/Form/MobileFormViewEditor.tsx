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

import React, { useContext } from "react";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { isReadOnly } from "model/selectors/RowState/isReadOnly";
import { shadeHexColor } from "utils/colorUtils";
import { ComboBox, XmlBuildDropdownEditor } from "gui/connections/MobileComponents/Form/ComboBox/ComboBox";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IProperty } from "model/entities/types/IProperty";
import { FormViewEditor } from "gui/Workbench/ScreenArea/FormView/FormViewEditor";
import { MobileTagInputEditor } from "gui/connections/MobileComponents/Form/ComboBox/MobileTagInputEditor";
import { MobXProviderContext } from "mobx-react";
import { MobileState } from "model/entities/MobileState/MobileState";
import { EditLayoutState } from "model/entities/MobileState/MobileLayoutState";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";
import { MobileDateTimeEditor } from "gui/connections/MobileComponents/Form/MobileDateTimeEditor";
import { onFieldBlur } from "model/actions-ui/DataView/TableView/onFieldBlur";

export const MobileFormViewEditor: React.FC<{
  value?: any;
  textualValue?: any;
  isRichText: boolean;
  property: IProperty;
  xmlNode: any;
  onTextOverflowChanged?: (tooltip: string | null | undefined) => void;
  backgroundColor?: string;
}> = (props) => {

  const mobileState = useContext(MobXProviderContext).application.mobileState as MobileState;
  const rowId = getSelectedRowId(props.property);
  const row = getSelectedRow(props.property);
  const foregroundColor = getRowStateForegroundColor(props.property, rowId || "");
  const readOnly = !row || isReadOnly(props.property!, rowId);
  const backgroundColor = readOnly
    ? shadeHexColor(props.backgroundColor, -0.1)
    : props.backgroundColor;

  async function onChange(event: any, newValue: any) {
    const row = getSelectedRow(props.property);
    if(!row){
      return;
    }
    onFieldChange(props.property)({
      event: event,
      row: row!,
      property: props.property,
      value: newValue,
    });
  }

  const onEditorBlur = (event: any) => onFieldBlur(props.property)();

  if (props.property!.column === "ComboBox") {
    return (
      <ComboBox
        key={props.xmlNode.$iid}
        onTextOverflowChanged={props.onTextOverflowChanged}
        xmlNode={props.xmlNode}
        isReadOnly={readOnly}
        autoSort={props.property?.autoSort}
        backgroundColor={backgroundColor}
        foregroundColor={foregroundColor}
        customStyle={props.property?.style}
        isLink={props.property?.isLink}
        dataView={getDataView(props.property)}
        property={props.property!}
      />);
  }
  if (props.property!.column === "TagInput") {
    return (
      <MobileTagInputEditor
        key={props.xmlNode.$iid}
        isReadOnly={readOnly}
        backgroundColor={backgroundColor}
        foregroundColor={foregroundColor}
        customStyle={props.property.style}
        property={props.property}
        onChange={onChange}
        onPlusButtonClick={() => {
          const previousState = mobileState.layoutState;
          mobileState.layoutState = new EditLayoutState(
            <XmlBuildDropdownEditor
              {...props}
              isReadOnly={readOnly}
              dataView={getDataView(props.property)}
              property={props.property}
              editingTags={true}
              onValueSelected={() => mobileState.layoutState = previousState}
            />,
            props.property.name
          )
        }}
      />);
  }
  if (props.property!.column === "Date") {
    return (
      <MobileDateTimeEditor
        value={props.value}
        property={props.property}
        outputFormat={props.property!.formatterPattern}
        outputFormatToShow={props.property!.modelFormatterPattern}
        isReadOnly={readOnly}
        backgroundColor={backgroundColor}
        foregroundColor={foregroundColor}
        onChange={onChange}
        onEditorBlur={onEditorBlur}
      />
    );
  }
  return (
    <FormViewEditor
      value={props.value}
      isRichText={props.isRichText}
      textualValue={props.textualValue}
      xmlNode={props.xmlNode}
      backgroundColor={props.backgroundColor}
      onTextOverflowChanged={props.onTextOverflowChanged}
    />);
}
