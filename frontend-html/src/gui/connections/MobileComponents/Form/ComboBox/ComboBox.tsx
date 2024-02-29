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

import React, { useContext, useState } from "react";
import S from "gui/connections/MobileComponents/Form/ComboBox/ComboBox.module.scss";
import cx from "classnames";
import CS from "gui/Components/Dropdown/Dropdown.module.scss";
import { MobXProviderContext, observer } from "mobx-react";
import { ComboFullScreenEditor } from "gui/connections/MobileComponents/Form/ComboBox/ComboFullScreenEditor";
import { IDataView } from "model/entities/types/IDataView";
import { IProperty } from "model/entities/types/IProperty";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { MobileDropdownBehavior } from "gui/connections/MobileComponents/Form/ComboBox/MobileDropdownBehavior";
import { DropdownEditorApi } from "modules/Editors/DropdownEditor/DropdownEditorApi";
import { DropdownEditorData, IDropdownEditorData } from "modules/Editors/DropdownEditor/DropdownEditorData";
import { TagInputEditorData } from "modules/Editors/DropdownEditor/TagInputEditorData";
import { DropdownDataTable } from "modules/Editors/DropdownEditor/DropdownTableModel";
import { DropdownEditorLookupListCache } from "modules/Editors/DropdownEditor/DropdownEditorLookupListCache";
import { DropdownEditorSetup, DropdownEditorSetupFromXml } from "modules/Editors/DropdownEditor/DropdownEditorSetup";
import { onMobileLinkClick } from "model/actions/DropdownEditor/onMobileLinkClick";
import { showDialog } from "model/selectors/getDialogStack";
import { Dialog } from "gui/connections/MobileComponents/Dialog";


export interface IComboBoxProps {
  xmlNode: any;
  isReadOnly: boolean;
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
  tagEditor?: JSX.Element;
  isLink?: boolean;
  autoSort?: boolean;
  onTextOverflowChanged?: (tooltip: string | null | undefined) => void;
  dataView: IDataView;
  property: IProperty;
  onKeyDown?(event: any): void;
}

export const ComboBox: React.FC<IComboBoxProps> = observer((props) => {

  const application = useContext(MobXProviderContext).application;
  const row = getSelectedRow(props.property);
  const currentValue = row && props.dataView.dataTable.getCellText(row, props.property);

  function onTextClick() {
    if (props.isLink) {
      onMobileLinkClick(props.property, row)
    }
  }

  function onButtonClick() {
    if (props.isReadOnly) {
      return;
    }
    let closeDialog = showDialog(application, "editor",
      <Dialog heading={props.property.name}>
        <XmlBuildDropdownEditor
          {...props}
          onValueSelected={() => closeDialog()}
        />
      </Dialog>
    );
  }

  return (
    <div className={cx(CS.control, S.mobileInput, props.isReadOnly ? S.readOnly : "")}>
      <div
        className={cx("input", S.input, props.isLink ? S.link : "")}
        onClick={onTextClick}
      >
        {currentValue}
      </div>
      <div
        className={cx(S.button, "inputBtn", "lastOne")}
        onClick={onButtonClick}
      >
        <i className="fas fa-caret-down"/>
      </div>
    </div>
  );
});

export interface IMobileDropdownContext {
  behavior: MobileDropdownBehavior;
  editorData: IDropdownEditorData;
  editorDataTable: DropdownDataTable;
  setup: DropdownEditorSetup;
}

export function XmlBuildDropdownEditor(props: {
  xmlNode: any;
  isReadOnly: boolean;
  editingTags?: boolean;
  isLink?: boolean;
  autoSort?: boolean;
  onTextOverflowChanged?: (tooltip: string | null | undefined) => void;
  onValueSelected: () => void;
  onKeyDown?(event: any): void;
  dataView: IDataView,
  property: IProperty;
}) {
  const mobxContext = useContext(MobXProviderContext);
  const {dataViewRowCursor, dataViewApi, dataViewData} = props.dataView;
  const workbench = mobxContext.workbench;
  const {lookupListCache} = workbench;

  const [dropdownEditorInfrastructure] = useState<IMobileDropdownContext>(() => {
    const dropdownEditorApi: DropdownEditorApi = new DropdownEditorApi(
      () => dropdownEditorSetup,
      dataViewRowCursor,
      dataViewApi,
      () => dropdownEditorBehavior
    );
    const dropdownEditorData: IDropdownEditorData = props.editingTags
      ? new TagInputEditorData(dataViewData, dataViewRowCursor, () => dropdownEditorSetup)
      : new DropdownEditorData(dataViewData, dataViewRowCursor, () => dropdownEditorSetup);
    const dropdownEditorDataTable = new DropdownDataTable(
      () => dropdownEditorSetup,
      dropdownEditorData
    );

    const dropdownEditorLookupListCache = new DropdownEditorLookupListCache(
      () => dropdownEditorSetup,
      lookupListCache
    );

    const dropdownEditorBehavior = new MobileDropdownBehavior({
      api: dropdownEditorApi,
      data: dropdownEditorData,
      dataTable: dropdownEditorDataTable,
      setup: () => dropdownEditorSetup,
      cache: dropdownEditorLookupListCache,
      autoSort: props.autoSort,
      onTextOverflowChanged: props.onTextOverflowChanged,
      onValueSelected: props.onValueSelected
    });

    const dropdownEditorSetup = DropdownEditorSetupFromXml(
      props.xmlNode, dropdownEditorDataTable, dropdownEditorBehavior, undefined, props.isLink);

    return {
      behavior: dropdownEditorBehavior,
      editorData: dropdownEditorData,
      columnDrivers: dropdownEditorSetup.columnDrivers,
      editorDataTable: dropdownEditorDataTable,
      setup: dropdownEditorSetup,
    };
  });

  return (
    <ComboFullScreenEditor
      {...props}
      behavior={dropdownEditorInfrastructure.behavior}
      dataTable={dropdownEditorInfrastructure.editorDataTable}
      columnDrivers={dropdownEditorInfrastructure.setup.columnDrivers}
      editorData={dropdownEditorInfrastructure.editorData}
    />
  );
}




