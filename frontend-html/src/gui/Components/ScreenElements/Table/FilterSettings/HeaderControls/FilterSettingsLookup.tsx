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

import { action, observable, runInAction } from "mobx";
import { observer } from "mobx-react";
import { CancellablePromise } from "mobx/lib/api/flow";
import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem,
} from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettingsComboBox";
import CS from "./FilterSettingsCommon.module.scss";
import { IFilterSetting } from "model/entities/types/IFilterSetting";
import { ILookup } from "model/entities/types/ILookup";
import { Operator } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";
import { IProperty } from "model/entities/types/IProperty";
import { MobileTagLookupFilterEditor } from "gui/connections/MobileComponents/Grid/MobileTagLookupFilterEditor";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import {
  TagLookupFilterEditor
} from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/TagLookupFilterEditor";

const OPERATORS = [
  Operator.in,
  Operator.notIn,
  Operator.startsWith,
  Operator.notStartsWith,
  Operator.contains,
  Operator.notContains,
  Operator.isNull,
  Operator.isNotNull
];

const LOOKUP_TYPE_OPERATORS = [
  Operator.startsWith,
  Operator.notStartsWith,
  Operator.contains,
  Operator.notContains
];

function operatorGroupChanges(oldOperator: string, newOperator: string) {
  let lookupOperatorTypes = LOOKUP_TYPE_OPERATORS.map(operator => operator.type);
  return newOperator === "null" || newOperator === "nnull" ||
    lookupOperatorTypes.includes(newOperator) && !lookupOperatorTypes.includes(oldOperator) || 
    !lookupOperatorTypes.includes(newOperator) && lookupOperatorTypes.includes(oldOperator)
}

const OpCombo: React.FC<{
  setting: any;
  enableLookupTypeFilters: boolean
  id: string;
}> = observer((props) => {
  return (
    <FilterSettingsComboBox
      id={props.id}
      trigger={<>{(OPERATORS.find((op) => op.type === props.setting.type) || {}).caption}</>}
    >
      {OPERATORS
        .filter(operator => props.enableLookupTypeFilters || !LOOKUP_TYPE_OPERATORS.includes(operator))
        .map((op) => (
          <FilterSettingsComboBoxItem
            key={op.type}
            onClick={() => {
              runInAction(() => {
                if (operatorGroupChanges(props.setting.type, op.type)) {
                  props.setting.val1 = undefined;
                  props.setting.val2 = undefined;
                }
                props.setting.type = op.type;
                props.setting.isComplete = op.type === "null" || op.type === "nnull" ||
                  props.setting.val1 !== undefined || props.setting.val2 !== undefined;
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
  id: string;
}> {

  @action.bound handleSelectedItemsChange(items: Array<any>) {
    this.props.setting.val1 = [...items];
    this.props.setting.val2 = undefined;
    this.props.setting.isComplete = this.props.setting.val1 !== undefined && this.props.setting.val1.length > 0;
  }

  @action.bound handleTermChange(event: any) {
    this.props.setting.val1 = undefined;
    this.props.setting.val2 = event.target.value;
    this.props.setting.isComplete = !!this.props.setting.val2;
  }

  render() {
    const {setting} = this.props;
    switch (setting?.type) {
      case "in":
      case "nin":
        if (isMobileLayoutActive(this.props.property)) {
          return (
            <MobileTagLookupFilterEditor
              id={this.props.id}
              lookup={this.props.lookup}
              property={this.props.property}
              getOptions={this.props.getOptions}
              onChange={this.handleSelectedItemsChange}
              values={this.props.setting.val1 ?? []}
              autoFocus={this.props.autoFocus}
              setting={this.props.setting}
            />
          );
        } else {
          return (
            <TagLookupFilterEditor
              id={this.props.id}
              lookup={this.props.lookup}
              property={this.props.property}
              getOptions={this.props.getOptions}
              onChange={this.handleSelectedItemsChange}
              values={this.props.setting.val1 ?? []}
              autoFocus={this.props.autoFocus}
            />
          );
        }
      case "starts":
      case "nstarts":
      case "contains":
      case "ncontains":
        return (
          <input
            id={this.props.id}
            value={this.props.setting.val2 ?? ""}
            className={CS.input}
            autoComplete={"off"}
            onChange={this.handleTermChange}
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
export class FilterSettingsLookup extends React.Component<{
  getOptions: (searchTerm: string) => CancellablePromise<Array<any>>;
  lookup: ILookup;
  property: IProperty;
  setting: IFilterSetting;
  autoFocus: boolean;
  id: string;
}> {
  static get defaultSettings() {
    return new LookupFilterSetting(OPERATORS[0].type)
  }

  render() {
    const setting = this.props.setting;
    return (
      <>
        <OpCombo
          id={"combo_" + this.props.id}
          setting={setting}
          enableLookupTypeFilters={this.props.property.supportsServerSideSorting}/>
        <OpEditors
          id={"input_" + this.props.id}
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

export class LookupFilterSetting implements IFilterSetting {
  @observable type: string;
  @observable val1?: any; // used for "in" operator ... string[]
  @observable val2?: any; // used for "contains" operator ... string
  isComplete: boolean;

  private _lookupId: string | undefined;

  public get lookupId(): string | undefined {
    if (LOOKUP_TYPE_OPERATORS.map(operator => operator.type).includes(this.type)) {
      return this._lookupId;
    }
    return undefined;
  }

  public set lookupId(value: string | undefined) {
    this._lookupId = value;
  }

  get filterValue1() {
    if (!this.val1) {
      return this.val1;
    }
    switch (this.type) {
      case "contain":
      case "ncontain":
        return this.val2;
      case "in":
      case "nin":
        return this.val1;
      default:
        return undefined;
    }
  }

  get filterValue2() {
    return this.val2;
  }


  get val1ServerForm() {
    return this.val1 ? this.val1.join(",") : this.val1;
  }

  get val2ServerForm() {
    return this.val2;
  }

  constructor(type: string, isComplete = false, val1?: string, val2?: any) {
    this.type = type;
    this.isComplete = isComplete;
    if (Array.isArray(val1)) {
      this.val1 = [...new Set(val1)];
    } else if (val1 !== undefined && val1 !== null) {
      this.val1 = [...new Set(val1.split(","))];
    }
    this.val2 = val2 ?? undefined;
  }
}


