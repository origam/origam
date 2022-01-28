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

import { observable, runInAction } from "mobx";
import { observer } from "mobx-react";
import { CancellablePromise } from "mobx/lib/api/flow";
import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem,
} from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettingsComboBox";
import { IFilterSetting } from "model/entities/types/IFilterSetting";
import { ILookup } from "model/entities/types/ILookup";
import { IProperty } from "model/entities/types/IProperty";
import { Operator } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { TagFilterEditor } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/TagFilterEditor";
import { MobileTagFilterEditor } from "gui/connections/MobileComponents/Grid/MobileTagFilterEditor";

const OPERATORS = [
  Operator.in,
  Operator.notIn,
  Operator.isNull,
  Operator.isNotNull
];

const OpCombo: React.FC<{
  setting: IFilterSetting
  id: string;
}> = observer((props) => {
  return (
    <FilterSettingsComboBox
      id={props.id}
      trigger={<>{(OPERATORS.find((op) => op.type === props.setting.type) || {}).caption}</>}
    >
      {OPERATORS.map((op) => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() => {
            runInAction(() => {
              props.setting.type = op.type;
              props.setting.isComplete = op.type === "null" || op.type === "nnull" || props.setting.val1 !== undefined;
              if (op.type === "null" || op.type === "nnull") {
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
  id: string;
}> {

  render() {
    const {setting} = this.props;
    switch (setting?.type) {
      case "in":
      case "nin":
        if (isMobileLayoutActive(this.props.property)) {
          return <MobileTagFilterEditor
            id={this.props.id}
            lookup={this.props.lookup}
            property={this.props.property}
            getOptions={this.props.getOptions}
            setting={setting}
            autoFocus={this.props.autoFocus}
          />
        } else {
          return (
            <TagFilterEditor
              id={this.props.id}
              lookup={this.props.lookup}
              property={this.props.property}
              getOptions={this.props.getOptions}
              values={setting.val1 ?? []}
              setting={setting}
              autoFocus={this.props.autoFocus}
            />
          );
        }
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
  id: string;
}> {
  static get defaultSettings() {
    return new TagInputFilterSetting(OPERATORS[0].type)
  }

  render() {
    const setting = this.props.setting;
    return (
      <>
        <OpCombo
          setting={setting}
          id={"combo_" + this.props.id}
        />
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


  get val1ServerForm() {
    return this.val1 ? this.val1.join(",") : this.val1;
  }

  get val2ServerForm() {
    return this.val2;
  }

  constructor(type: string, isComplete = false, val1?: string, val2?: any) {
    this.type = type;
    this.isComplete = isComplete;
    if (val1 !== undefined && val1 !== null) {
      this.val1 = [...new Set(val1.split(","))];
    }
    this.val2 = val2 ?? undefined;
  }
}
