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
import { MobileTagInputEditor } from "gui/connections/MobileComponents/Form/ComboBox/MobileTagInputEditor";
import { getMobileState } from "model/selectors/getMobileState";
import { EditLayoutState } from "model/entities/MobileState/MobileLayoutState";
import {
  FilterEditorData,
} from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/TagFilterEditor";
import { IFilterSetting } from "model/entities/types/IFilterSetting";
import { CancellablePromise } from "mobx/lib/api/flow";
import { ILookup } from "model/entities/types/ILookup";
import { IProperty } from "model/entities/types/IProperty";
import { MobXProviderContext } from "mobx-react";
import { IMobileDropdownContext } from "gui/connections/MobileComponents/Form/ComboBox/ComboBox";
import { IDropdownEditorApi } from "modules/Editors/DropdownEditor/DropdownEditorApi";
import { IDropdownEditorData } from "modules/Editors/DropdownEditor/DropdownEditorData";
import { DropdownColumnDrivers, DropdownDataTable } from "modules/Editors/DropdownEditor/DropdownTableModel";
import { DropdownEditorLookupListCache } from "modules/Editors/DropdownEditor/DropdownEditorLookupListCache";
import { MobileDropdownBehavior } from "gui/connections/MobileComponents/Form/ComboBox/MobileDropdownBehavior";
import { DefaultHeaderCellDriver } from "modules/Editors/DropdownEditor/Cells/HeaderCell";
import { TextCellDriver } from "modules/Editors/DropdownEditor/Cells/TextCellDriver";
import { DropdownEditorSetup } from "modules/Editors/DropdownEditor/DropdownEditorSetup";
import { ComboFullScreenEditor } from "gui/connections/MobileComponents/Form/ComboBox/ComboFullScreenEditor";
import {
  FilterDropDownApi
} from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/TagLookupFilterEditor";

export const MobileTagFilterEditor: React.FC<{
  setting: IFilterSetting;
  getOptions: (searchTerm: string) => CancellablePromise<Array<any>>;
  lookup: ILookup;
  property: IProperty;
  autoFocus: boolean;
  id: string;
}> = (props) => {
  return <MobileTagInputEditor
    key={props.id}
    isReadOnly={false}
    property={props.property}
    values={props.setting.val1 ?? []}
    onPlusButtonClick={() => {
      let mobileState = getMobileState(props.property);
      let layoutBeforeEditing = mobileState.layoutState;
      mobileState.layoutState = new EditLayoutState(
        <FullScreenFilterTagEditor
          id={props.id}
          lookup={props.lookup}
          property={props.property}
          getOptions={props.getOptions}
          onChange={() => {
            mobileState.layoutState = layoutBeforeEditing;
          }}
          setting={props.setting}
          values={props.setting.val1 ?? []}
          autoFocus={props.autoFocus}
        />,
        props.property.name,
        layoutBeforeEditing
      )
    }}
  />
}


function FullScreenFilterTagEditor(props: {
  lookup: ILookup;
  property: IProperty;
  getOptions: (searchTerm: string) => CancellablePromise<Array<any>>;
  values: Array<any>;
  onChange: () => void;
  setting: IFilterSetting;
  autoFocus: boolean;
  id: string;
}) {
  const mobxContext = useContext(MobXProviderContext);
  const workbench = mobxContext.workbench;
  const {lookupListCache} = workbench;

  const [dropdownEditorInfrastructure] = useState<IMobileDropdownContext>(() => {
    const dropdownEditorApi: IDropdownEditorApi = new FilterDropDownApi(props.getOptions);
    const dropdownEditorData: IDropdownEditorData = new FilterEditorData(props.setting, props.onChange);

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
    });

    const drivers = new DropdownColumnDrivers();

    let identifierIndex = 0;
    const columnNameToIndex = new Map<string, number>([
      [props.property.identifier!, identifierIndex],
    ]);
    const visibleColumnNames: string[] = [];

    columnNameToIndex.set(props.property.name, 1);
    visibleColumnNames.push(props.property.name);

    drivers.allDrivers.push({
      columnId: props.property.id,
      headerCellDriver: new DefaultHeaderCellDriver(props.property.name),
      bodyCellDriver: new TextCellDriver(1, dropdownEditorDataTable, dropdownEditorBehavior),
    });

    const showUniqueValues = true;

    const dropdownEditorSetup = new DropdownEditorSetup(
      props.property.id,
      props.lookup.lookupId,
      [],
      visibleColumnNames,
      columnNameToIndex,
      showUniqueValues,
      props.property.identifier!,
      identifierIndex,
      props.property.parameters,
      props.property.lookup?.dropDownType!,
      props.property.lookup?.cached!,
      !props.property.lookup?.searchByFirstColumnOnly,
      drivers
    );

    return {
      behavior: dropdownEditorBehavior,
      editorData: dropdownEditorData,
      editorDataTable: dropdownEditorDataTable,
      setup: dropdownEditorSetup
    };
  });

  (dropdownEditorInfrastructure.editorData as FilterEditorData).setting = props.setting;

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

