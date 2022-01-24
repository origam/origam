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

import React from "react";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { isReadOnly } from "model/selectors/RowState/isReadOnly";
import { shadeHexColor } from "utils/colorUtils";
import { ComboBox } from "gui/connections/MobileComponents/Form/ComboBox/ComboBox";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IProperty } from "model/entities/types/IProperty";
import { FormViewEditor } from "gui/Workbench/ScreenArea/FormView/FormViewEditor";
import { TagInputEditor } from "gui/connections/MobileComponents/Form/ComboBox/TagInputEditor";

export const MobileFormViewEditor: React.FC<{
  value?: any;
  textualValue?: any;
  isRichText: boolean;
  property: IProperty;
  xmlNode: any;
  onTextOverflowChanged?: (toolTip: string | null | undefined) => void;
  backgroundColor?: string;
}> = (props) => {

  const rowId = getSelectedRowId(props.property);
  const row = getSelectedRow(props.property);
  const foregroundColor = getRowStateForegroundColor(props.property, rowId || "");
  const readOnly = !row || isReadOnly(props.property!, rowId);
  const backgroundColor = readOnly
    ? shadeHexColor(props.backgroundColor, -0.1)
    : props.backgroundColor;

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
      <TagInputEditor
        key={props.xmlNode.$iid}
        xmlNode={props.xmlNode}
        isReadOnly={readOnly}
        backgroundColor={backgroundColor}
        foregroundColor={foregroundColor}
        customStyle={props.property?.style}
        dataView={getDataView(props.property)}
        property={props.property!}
      />);
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
