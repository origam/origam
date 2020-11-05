import { DateTimeEditor } from "gui/Components/ScreenElements/Editors/DateTimeEditor";
import { action, observable, runInAction } from "mobx";
import { observer } from "mobx-react";
import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem,
} from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettingsComboBox";
import { FilterSetting } from "./FilterSetting";
import { T } from "utils/translation";
import { Operator } from "./Operatots";
import {LookupFilterSetting} from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSettingsLookup";

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
}> = (props) => {
  return (
    <FilterSettingsComboBox
      trigger={<>{(OPERATORS.find((item) => item.type === props.setting.type) || {}).human}</>}
    >
      {OPERATORS.map((op) => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() => {
            props.setting.type = op.type;
            props.onChange(props.setting);}
          }
        >
          {op.human}
        </FilterSettingsComboBoxItem>
      ))}
    </FilterSettingsComboBox>
  );
};

const OpEditors: React.FC<{
  setting: any;
  onChange?: (newSetting: any) => void;
  onBlur?: (event: any) => void;
  onKeyDown?: (event: any) => void;
}> = observer((props) => {
  const { setting } = props;
  switch (setting.type) {
    case "eq":
    case "neq":
    case "lt":
    case "gt":
    case "lte":
    case "gte":
      return (
        <DateTimeEditor
          value={setting.val1 ?? ""}
          outputFormat="D.M.YYYY"
          onChange={(event, isoDay) => {
            runInAction(()=> {
              setting.val1 = isoDay === null ? undefined : removeTimeZone(isoDay);
              props.onChange && props.onChange(setting)
            })
          }}
          onEditorBlur={props.onBlur}
          onKeyDown={props.onKeyDown}
        />
      );

    case "between":
    case "nbetween":
      return (
        <>
          <DateTimeEditor
            value={setting.val1}
            outputFormat="D.M.YYYY"
            onChange={(event, isoDay) => {
              runInAction(()=> {
                setting.val1 = isoDay === null ? undefined : removeTimeZone(isoDay);
                props.onChange &&
                props.onChange(setting)
              })
            }}
            onEditorBlur={props.onBlur}
            onKeyDown={props.onKeyDown}
          />
          <DateTimeEditor
            value={setting.val2}
            outputFormat="D.M.YYYY"
            onChange={(event, isoDay) => {
              runInAction(()=> {
                setting.val2 = isoDay === null ? undefined : removeTimeZone(isoDay);
                props.onChange &&
                props.onChange(setting)
              })
            }}
            onEditorBlur={props.onBlur}
            onKeyDown={props.onKeyDown}
          />
        </>
      );
    case "null":
    case "nnull":
    default:
      return null;
  }
});

@observer
export class FilterSettingsDate extends React.Component<{
  setting?: any;
}> {

  static get defaultSettings(){
    return new FilterSetting(OPERATORS[0].type)
  }

  @action.bound
  handleChange(newSetting: any) {
    this.handleSettingChange();
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
        <OpCombo setting={this.props.setting} onChange={this.handleChange} />
        <OpEditors setting={this.props.setting} onChange={this.handleChange} />
      </>
    );
  }
}

function removeTimeZone(isoDateString: string | null | undefined) {
  if (!isoDateString) return isoDateString;
  return isoDateString.substring(0, 23);
}
