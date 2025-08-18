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

import { DateTimeEditor } from "gui/Components/ScreenElements/Editors/DateTimeEditor/DateTimeEditor";
import { action, runInAction } from "mobx";
import { observer } from "mobx-react";
import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem,
} from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettingsComboBox";
import { FilterSetting } from "./FilterSetting";
import { Operator } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";
import { getDefaultCsDateFormatDataFromCookie } from "utils/cookies";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { IProperty } from "model/entities/types/IProperty";
import { MobileDateTimeEditor } from "gui/connections/MobileComponents/Form/MobileDateTimeEditor";
import { IFilterSetting } from "model/entities/types/IFilterSetting";
import { IEditorState } from "gui/Components/ScreenElements/Editors/DateTimeEditor/DateEditorModel";
import S from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSettingsCommon.module.scss";
import { csToMomentFormat } from "utils/dateConversion";

const OPERATORS = [
  Operator.equals,
  Operator.notEquals,
  Operator.lessThanOrEquals,
  Operator.greaterThanOrEquals,
  Operator.lessThan,
  Operator.greaterThan,
  Operator.between,
  Operator.notBetween,
  Operator.isNull,
  Operator.isNotNull
];

const OpCombo: React.FC<{
  setting: any;
  onChange: (newSetting: any) => void;
  id: string;
}> = observer((props) => {
  return (
    <FilterSettingsComboBox
      id={props.id}
      trigger={<>{(OPERATORS.find((item) => item.type === props.setting.type) || {}).caption}</>}
    >
      {OPERATORS.map((op) => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() => {
            props.setting.type = op.type;
            props.onChange(props.setting);
          }
          }
        >
          {op.caption}
        </FilterSettingsComboBoxItem>
      ))}
    </FilterSettingsComboBox>
  );
});

const OpEditors: React.FC<{
  setting: IFilterSetting;
  onChange?: (newSetting: any) => void;
  onBlur?: (event: any) => Promise<void>;
  onKeyDown?: (event: any) => void;
  autoFocus: boolean;
  id: string;
  property: IProperty;
}> = observer((props) => {
  const {setting} = props;
  const dateFormatCs = getDefaultCsDateFormatDataFromCookie().defaultLongDateFormat;
  const dateFormatMoment = csToMomentFormat(dateFormatCs)!;
  switch (setting.type) {
    case "eq":
    case "neq":
    case "lt":
    case "gt":
    case "lte":
    case "gte":
      if (isMobileLayoutActive(props.property)) {
        return (
          <MobileDateTimeEditor
            value={setting.val1 ?? ""}
            property={props.property}
            outputFormat={dateFormatMoment}
            outputFormatToShow={dateFormatCs}
            onChange={async (event, isoDay) => {
              runInAction(() => {
                setting.val1 = isoDay === null ? undefined : removeTimeZone(isoDay);
                props.onChange && props.onChange(setting)
              })
            }}
            autoFocus={props.autoFocus}
            onKeyDown={props.onKeyDown}
            editorState={new MobileFilterEditorStateVal1(setting)}
            showClearButton={true}
            inputClass={S.dateTimeInput}
          />);
      } else {
        return (
          <DateTimeEditor
            id={props.id}
            value={setting.val1 ?? ""}
            outputFormat={dateFormatMoment}
            outputFormatToShow={dateFormatCs}
            onChange={async (event, isoDay) => {
              runInAction(() => {
                setting.val1 = isoDay === null ? undefined : removeTimeZone(isoDay);
                props.onChange && props.onChange(setting)
              })
            }}
            autoFocus={props.autoFocus}
            onEditorBlur={props.onBlur}
            onKeyDown={props.onKeyDown}
            className={S.dateTimeInput}
          />
        );
      }
    case "between":
    case "nbetween":
      if (isMobileLayoutActive(props.property)) {
        return (
          <>
            <MobileDateTimeEditor
              value={setting.val1}
              property={props.property}
              outputFormat={dateFormatMoment}
              outputFormatToShow={dateFormatCs}
              onChange={async (event, isoDay) => {
                await runInAction(() => {
                  setting.val1 = isoDay === null ? undefined : removeTimeZone(isoDay);
                  props.onChange && props.onChange(setting)
                })
              }}
              autoFocus={props.autoFocus}
              onKeyDown={props.onKeyDown}
              editorState={new MobileFilterEditorStateVal1(setting)}
              showClearButton={true}
              inputClass={S.dateTimeInput}
            />
            <MobileDateTimeEditor
              value={setting.val2}
              property={props.property}
              outputFormat={dateFormatMoment}
              outputFormatToShow={dateFormatCs}
              onChange={async (event, isoDay) => {
                await runInAction(() => {
                  setting.val2 = isoDay === null ? undefined : removeTimeZone(isoDay);
                  props.onChange && props.onChange(setting)
                })
              }}
              autoFocus={props.autoFocus}
              onKeyDown={props.onKeyDown}
              editorState={new MobileFilterEditorStateVal2(setting)}
              showClearButton={true}
              inputClass={S.dateTimeInput}
            />
          </>
        );
      } else {
        return (
          <>
            <DateTimeEditor
              id={"from_" + props.id}
              value={setting.val1}
              outputFormat={dateFormatMoment}
              outputFormatToShow={dateFormatCs}
              onChange={async (event, isoDay) => {
                await runInAction(() => {
                  setting.val1 = isoDay === null ? undefined : removeTimeZone(isoDay);
                  props.onChange &&
                  props.onChange(setting)
                })
              }}
              onEditorBlur={props.onBlur}
              onKeyDown={props.onKeyDown}
              className={S.dateTimeInput}
            />
            <DateTimeEditor
              id={"to_" + props.id}
              value={setting.val2}
              outputFormat={dateFormatMoment}
              outputFormatToShow={dateFormatCs}
              onChange={async (event, isoDay) => {
                await runInAction(() => {
                  setting.val2 = isoDay === null ? undefined : removeTimeZone(isoDay);
                  props.onChange &&
                  props.onChange(setting)
                })
              }}
              onEditorBlur={props.onBlur}
              onKeyDown={props.onKeyDown}
              className={S.dateTimeInput}
            />
          </>
        );
      }
    case "null":
    case "nnull":
    default:
      return null;
  }
});

@observer
export class FilterSettingsDate extends React.Component<{
  setting: IFilterSetting;
  autoFocus: boolean
  id: string;
  property: IProperty;
}> {

  static get defaultSettings() {
    return new FilterSetting(OPERATORS[0].type)
  }

  @action.bound
  handleChange(newSetting: any) {
    this.handleSettingChange();
    return Promise.resolve();
  }

  handleSettingChange() {
    const setting = this.props.setting;
    switch (setting.type) {
      case "eq":
      case "neq":
      case "lt":
      case "gt":
      case "lte":
      case "gte":
        setting.isComplete = setting.val1 !== undefined;
        setting.val2 = undefined;
        break;
      case "between":
      case "nbetween":
        setting.isComplete = setting.val1 !== undefined && setting.val2 !== undefined;
        break;
      default:
        setting.val1 = undefined;
        setting.val2 = undefined;
        setting.isComplete = setting.type === "null" || setting.type === "nnull";
    }
  }

  render() {
    return (
      <>
        <OpCombo
          setting={this.props.setting}
          onChange={this.handleChange}
          id={"combo_" + this.props.id}
        />
        <OpEditors
          id={"input_" + this.props.id}
          setting={this.props.setting}
          onChange={this.handleChange}
          autoFocus={this.props.autoFocus}
          property={this.props.property}
        />
      </>
    );
  }
}

function removeTimeZone(isoDateString: string | null | undefined) {
  if (!isoDateString) return isoDateString;
  return isoDateString.substring(0, 23);
}

class MobileFilterEditorStateVal1 implements IEditorState {

  constructor(private setting: IFilterSetting) {
  }

  get initialValue() {
    return this.setting.val1;
  }
}

class MobileFilterEditorStateVal2 implements IEditorState {

  constructor(private setting: IFilterSetting) {
  }

  get initialValue() {
    return this.setting.val2;
  }
}

