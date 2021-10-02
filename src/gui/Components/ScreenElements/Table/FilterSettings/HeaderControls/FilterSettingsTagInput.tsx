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

import { action, computed, observable, runInAction } from "mobx";
import { MobXProviderContext, observer } from "mobx-react";
import { CancellablePromise } from "mobx/lib/api/flow";
import React, { useContext, useState } from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem,
} from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettingsComboBox";
import S from "./FilterSettingsLookup.module.scss";
import { IFilterSetting } from "model/entities/types/IFilterSetting";
import {
  CtxDropdownEditor,
  DropdownEditor,
  DropdownEditorSetup,
  IDropdownEditorContext,
} from "modules/Editors/DropdownEditor/DropdownEditor";
import { TagInputEditor } from "gui/Components/ScreenElements/Editors/TagInputEditor";
import { IDropdownEditorApi } from "modules/Editors/DropdownEditor/DropdownEditorApi";
import { IDropdownEditorData } from "modules/Editors/DropdownEditor/DropdownEditorData";
import {
  DropdownColumnDrivers,
  DropdownDataTable,
} from "modules/Editors/DropdownEditor/DropdownTableModel";
import { DropdownEditorLookupListCache } from "modules/Editors/DropdownEditor/DropdownEditorLookupListCache";
import { DropdownEditorBehavior } from "modules/Editors/DropdownEditor/DropdownEditorBehavior";
import { TextCellDriver } from "modules/Editors/DropdownEditor/Cells/TextCellDriver";
import { DefaultHeaderCellDriver } from "modules/Editors/DropdownEditor/Cells/HeaderCell";
import { ILookup } from "model/entities/types/ILookup";
import { IProperty } from "model/entities/types/IProperty";
import { Operator } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";
import { prepareForFilter } from "../../../../../../model/selectors/PortalSettings/getStringFilterConfig";

const OPERATORS = [
    Operator.in,
    Operator.notIn,
    Operator.isNull,
    Operator.isNotNull
  ];

const OpCombo: React.FC<{
  setting: IFilterSetting
}> = observer((props) => {
  return (
    <FilterSettingsComboBox
      trigger={<>{(OPERATORS.find((op) => op.type === props.setting.type) || {}).caption}</>}
    >
      {OPERATORS.map((op) => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() => {
            runInAction(() => {
              props.setting.type = op.type;
              props.setting.isComplete = op.type === "null" || op.type === "nnull" || props.setting.val1 !== undefined;
              if(op.type === "null" || op.type === "nnull"){
                props.setting.val1 = undefined;
                props.setting.val2 = undefined;
              }
            });
          }}
        >
          {op.caption}
        </FilterSettingsComboBoxItem>
      ))}
    </FilterSettingsComboBox>
  );
});

export interface ITagEditorItem {
  text: string;
  value: string;
}

@observer
class OpEditors extends React.Component<{
  setting: IFilterSetting;
  getOptions: (searchTerm: string) => CancellablePromise<Array<any>>;
  lookup: ILookup;
  property: IProperty;
  autoFocus: boolean;
}> {

  render() {
    const { setting } = this.props;
    switch (setting?.type) {
      case "in":
      case "nin":
        return (
          <FilterBuildDropdownEditor
            lookup={this.props.lookup}
            property={this.props.property}
            getOptions={this.props.getOptions}
            values={setting.val1 ?? []}
            setting={setting}
            autoFocus={this.props.autoFocus}
          />
        );
      case "null":
      case "nnull":
      default:
        return null;
    }
  }
}

@observer
export class FilterSettingsTagInput extends React.Component<{
  getOptions: (searchTerm: string) => CancellablePromise<Array<any>>;
  lookup: ILookup;
  property: IProperty;
  setting: IFilterSetting;
  autoFocus: boolean;
}> {
  static get defaultSettings(){
    return new TagInputFilterSetting(OPERATORS[0].type)
  }

  render() {
    const setting = this.props.setting;
    return (
      <>
        <OpCombo setting={setting}/>
        <OpEditors
          setting={setting}
          getOptions={this.props.getOptions}
          lookup={this.props.lookup}
          property={this.props.property}
          autoFocus={this.props.autoFocus}
        />
      </>
    );
  }
}

export class TagInputFilterSetting implements IFilterSetting {
  @observable type: string;
  @observable val1?: any; // used for "in" operator ... string[]
  @observable val2?: any; // used for "contains" operator ... string
  isComplete: boolean;
  lookupId: string | undefined;

  get filterValue1() {
    if (!this.val1) {
      return this.val1;
    }
    switch (this.type) {
      case "in":
      case "nin":
        return this.val1;
      default:
        return undefined;
    }
  }

  get filterValue2() {    
    return this.type === "between" || this.type === "nbetween" 
      ? this.val2 
      : undefined;
  }


  get val1ServerForm(){
    return this.val1 ? this.val1.join(",") : this.val1;
  }

  get val2ServerForm(){
    return this.val2;
  }

  constructor(type: string, isComplete=false, val1?:string, val2?: any) {
    this.type = type;
    this.isComplete = isComplete;
    if(val1 !== undefined && val1 !== null){
      this.val1 = [...new Set(val1.split(","))];
    }
    this.val2 = val2 ?? undefined;
  }
}

export function FilterBuildDropdownEditor(props: {
  lookup: ILookup;
  property: IProperty;
  getOptions: (searchTerm: string) => CancellablePromise<Array<any>>;
  values: Array<any>;
  setting: IFilterSetting;
  autoFocus: boolean;
}) {
  const mobxContext = useContext(MobXProviderContext);
  const workbench = mobxContext.workbench;
  const { lookupListCache } = workbench;

  const [dropdownEditorInfrastructure] = useState<IDropdownEditorContext>(() => {
    const dropdownEditorApi: IDropdownEditorApi = new DropDownApi(props.getOptions);
    const dropdownEditorData: IDropdownEditorData = new FilterEditorData(props.setting); 

    const dropdownEditorDataTable = new DropdownDataTable(
      () => dropdownEditorSetup,
      dropdownEditorData
    );
    const dropdownEditorLookupListCache = new DropdownEditorLookupListCache(
      () => dropdownEditorSetup,
      lookupListCache
    );
    const dropdownEditorBehavior = new DropdownEditorBehavior(
      dropdownEditorApi,
      dropdownEditorData,
      dropdownEditorDataTable,
      () => dropdownEditorSetup,
      dropdownEditorLookupListCache,
      false,
      text => prepareForFilter(props.property, text)!
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
      props.property.lookup?.cached!,
      !props.property.lookup?.searchByFirstColumnOnly
    );

    return {
      behavior: dropdownEditorBehavior,
      editorData: dropdownEditorData,
      columnDrivers: drivers,
      editorDataTable: dropdownEditorDataTable,
      setup: dropdownEditorSetup
    };
  });

  (dropdownEditorInfrastructure.editorData as FilterEditorData).setting = props.setting;

  const value = props.values;
  return (
    <CtxDropdownEditor.Provider value={dropdownEditorInfrastructure}>
      <DropdownEditor
        editor={
          <TagInputEditor
            customInputClass={S.tagInput}
            value={value}
            isReadOnly={false}
            isInvalid={false}
            autoFocus={props.autoFocus}
            onClick={undefined}
          />
        }
      />
    </CtxDropdownEditor.Provider>
  );
}

export class FilterEditorData implements IDropdownEditorData {
  constructor( public setting: IFilterSetting) {
    }

  setValue(value: string[]){}

  @computed get value(): string | string[] | null {
    return this.setting.val1;
  }

  @computed get text(): string {
    return "";
  }

  get isResolving() {
    return false;
  }

  onChange(){
    this.setting.val2 = undefined;
    this.setting.isComplete = this.setting.val1 !== undefined && this.setting.val1.length > 0;
  }

  @action.bound chooseNewValue(value: any) {
    if (value !== null && !this.setting.val1?.includes(value)) {
      if(this.setting.val1 === undefined){
        this.setting.val1 = [value];
      }else{
        this.setting.val1 = [...this.setting.val1, value];
      }
      this.onChange();
    }
  }

  get idsInEditor() {
    return this.setting.val1 ?? [] as string[];
  }

  remove(valueToRemove: any): void {
    const index = this.setting.val1.indexOf(valueToRemove)
    if(index > -1){
      this.setting.val1.splice(index, 1);
    }
    if(this.setting.val1?.length === 0){
      this.setting.val1 = undefined;
    }else{
      this.setting.val1 = [...this.setting.val1];
    }
    this.onChange();
  }
}

class DropDownApi implements IDropdownEditorApi {
  constructor(private getOptions: (searchTerm: string) => CancellablePromise<Array<any>>) {}

  *getLookupList(searchTerm: string): Generator {
    return yield this.getOptions("");
  }
}
