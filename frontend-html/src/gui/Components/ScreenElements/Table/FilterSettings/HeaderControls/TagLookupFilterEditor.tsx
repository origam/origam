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


import { ILookup } from "model/entities/types/ILookup";
import { IProperty } from "model/entities/types/IProperty";
import { CancellablePromise } from "mobx/lib/api/flow";
import React, { useContext, useState } from "react";
import { MobXProviderContext } from "mobx-react";
import {
  CtxDropdownEditor,
  DropdownEditor,
  IDropdownEditorContext
} from "modules/Editors/DropdownEditor/DropdownEditor";
import { IDropdownEditorApi } from "modules/Editors/DropdownEditor/DropdownEditorApi";
import { IDropdownEditorData } from "modules/Editors/DropdownEditor/DropdownEditorData";
import { DropdownColumnDrivers, DropdownDataTable } from "modules/Editors/DropdownEditor/DropdownTableModel";
import { DropdownEditorLookupListCache } from "modules/Editors/DropdownEditor/DropdownEditorLookupListCache";
import { DropdownEditorBehavior } from "modules/Editors/DropdownEditor/DropdownEditorBehavior";
import { DefaultHeaderCellDriver } from "modules/Editors/DropdownEditor/Cells/HeaderCell";
import { TextCellDriver } from "modules/Editors/DropdownEditor/Cells/TextCellDriver";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { DropdownEditorSetup } from "modules/Editors/DropdownEditor/DropdownEditorSetup";
import { TagInputEditor } from "gui/Components/ScreenElements/Editors/TagInputEditor";
import S from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSettingsLookup.module.scss";
import { action, computed, observable } from "mobx";

export function TagLookupFilterEditor(props: {
  lookup: ILookup;
  property: IProperty;
  getOptions: (searchTerm: string) => CancellablePromise<Array<any>>;
  onChange(selectedItems: Array<any>): void;
  values: Array<any>;
  autoFocus: boolean;
  id: string;
}) {
  const mobxContext = useContext(MobXProviderContext);


  const workbench = mobxContext.workbench;
  const {lookupListCache} = workbench;

  const [dropdownEditorInfrastructure] = useState<IDropdownEditorContext>(() => {
    const dropdownEditorApi: IDropdownEditorApi = new FilterDropDownApi(props.getOptions);
    const dropdownEditorData: IDropdownEditorData = new FilterEditorData(props.onChange);

    const dropdownEditorDataTable = new DropdownDataTable(
      () => dropdownEditorSetup,
      dropdownEditorData
    );
    const dropdownEditorLookupListCache = new DropdownEditorLookupListCache(
      () => dropdownEditorSetup,
      lookupListCache
    );
    const dropdownEditorBehavior = new DropdownEditorBehavior(
      {
        api: dropdownEditorApi,
        data: dropdownEditorData,
        dataTable: dropdownEditorDataTable,
        setup: () => dropdownEditorSetup,
        cache: dropdownEditorLookupListCache,
        isReadOnly: false,
      }
    );

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
      false,
      !props.property.lookup?.searchByFirstColumnOnly,
      drivers
    );

    return {
      behavior: dropdownEditorBehavior,
      editorData: dropdownEditorData,
      editorDataTable: dropdownEditorDataTable,
      columnDrivers: undefined,
      setup: dropdownEditorSetup
    };
  });

  function onItemRemoved(event: any, value: string[]) {
    props.onChange(value);
  }

  const value = props.values;
  return (
    <CtxDropdownEditor.Provider value={dropdownEditorInfrastructure}>
      <DropdownEditor
        id={props.id}
        editor={
          <TagInputEditor
            id={props.id}
            customInputClass={S.tagInput}
            value={value}
            isReadOnly={false}
            onChange={onItemRemoved}
            onClick={undefined}
            autoFocus={props.autoFocus}
          />
        }
      />
    </CtxDropdownEditor.Provider>
  );
}


export class FilterDropDownApi implements IDropdownEditorApi {
  constructor(private getOptions: (searchTerm: string) => CancellablePromise<Array<any>>) {
  }

  *getLookupList(searchTerm: string): Generator {
    return yield this.getOptions(searchTerm);
  }
}


export class FilterEditorData implements IDropdownEditorData {
  constructor(private onChange: (selectedItems: Array<any>) => void) {
  }

  setValue(value: string | string[] | null) {
    if (value) {
      this._value = Array.isArray(value) ? value : [value];
    }
  }

  @computed get value(): string | string[] | null {
    return this._value;
  }

  @observable
  _value: any[] = [];

  @computed get text(): string {
    return "";
  }

  get isResolving() {
    return false;
  }

  @action.bound async chooseNewValue(value: any) {
    if (value !== null && !this._value.includes(value)) {
      this._value = [...this._value, value];
      this.onChange(this._value);
    }
  }

  get idsInEditor() {
    return this._value as string[];
  }

  remove(valueToRemove: any): void {
    const index = this._value.indexOf(valueToRemove)
    if (index > -1) {
      this._value.splice(index, 1);
    }
  }
}
