import React, { useImperativeHandle } from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.css";
import { observable, computed, action } from "mobx";
import { observer, PropTypes } from "mobx-react";
import {
  TagInput,
  TagInputItem,
  TagInputAdd,
  TagInputItemDelete
} from "gui/Components/TagInput/TagInput";
import { useObservable } from "mobx-react-lite";
import { Dropdowner } from "../../../../Dropdowner/Dropdowner";

export interface IStringFilterOp {}

interface IOp1 {
  type: "eq" | "neq" | "contains" | "ncontains";
  human: React.ReactNode;
  val1: string;
}

interface IOp0 {
  type: "null" | "nnull";
  human: React.ReactNode;
}

type ISetting = IOp1 | IOp0;

const OPERATORS: ISetting[] = [
  { human: <>=</>, type: "eq", val1: "" },
  { human: <>&ne;</>, type: "neq", val1: "" },
  { human: <>contain</>, type: "contains", val1: "" },
  { human: <>not contain</>, type: "ncontains", val1: "" },
  { human: <>is null</>, type: "null" },
  { human: <>is not null</>, type: "nnull" }
];

const OpCombo: React.FC<{
  setting: ISetting;
  onChange: (newSetting: ISetting) => void;
}> = props => {
  return (
    <FilterSettingsComboBox trigger={<>{props.setting.human}</>}>
      {OPERATORS.map(op => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() => props.onChange(op)}
        >
          {op.human}
        </FilterSettingsComboBoxItem>
      ))}
    </FilterSettingsComboBox>
  );
};

export interface ITagEditorItem {
  text: string;
  value: string;
}

const TagEditor: React.FC<{
  items: ITagEditorItem[];
  availableItems: ITagEditorItem[];
  onAdd?: (item: ITagEditorItem) => void;
  onDeleteClick?: (event: any, item: ITagEditorItem) => void;
}> = observer(props => {
  return (
    <TagInput>
      <Dropdowner
        trigger={({ refTrigger, setDropped }) => (
          <TagInputAdd domRef={refTrigger} onClick={() => setDropped(true)} />
        )}
        content={({ setDropped }) => (
          <div className={CS.tagEditorDropdown}>
            {props.availableItems.map(item => (
              <div
                key={item.value}
                className={CS.tagEditorDropdownItem}
                onClick={() => {
                  props.onAdd && props.onAdd(item);
                  setDropped(false);
                }}
              >
                {item.text}
              </div>
            ))}
          </div>
        )}
      />

      {props.items.map(item => (
        <TagInputItem key={item.value}>
          <TagInputItemDelete
            onClick={event =>
              props.onDeleteClick && props.onDeleteClick(event, item)
            }
          />
          {item.text}
        </TagInputItem>
      ))}
    </TagInput>
  );
});

const StatefulTagEditor: React.FC<{}> = observer(props => {
  const items = useObservable<ITagEditorItem[]>([]);
  return (
    <TagEditor
      items={items}
      availableItems={[
        { text: "Apple", value: "1" },
        { text: "Banana", value: "2" }
      ]}
      onAdd={item => items.push(item)}
      onDeleteClick={(event, item) =>
        items.splice(items.findIndex(ii => ii === item), 1)
      }
    />
  );
});

const OpEditors: React.FC<{
  setting: ISetting;
  onChange: (newSetting: ISetting) => void;
}> = props => {
  const { setting } = props;
  switch (setting.type) {
    case "eq":
    case "neq":
    case "contains":
    case "ncontains":
      return <StatefulTagEditor />;
    case "null":
    case "nnull":
    default:
      return null;
  }
};

@observer
export class FilterSettingsLookup extends React.Component {
  @observable selectedOperator: ISetting = OPERATORS[0];

  render() {
    return (
      <>
        <OpCombo
          setting={this.selectedOperator}
          onChange={newSetting => (this.selectedOperator = newSetting)}
        />
        <OpEditors
          setting={this.selectedOperator}
          onChange={newSetting => (this.selectedOperator = newSetting)}
        />

        {/*<input className={CS.input} />*/}
      </>
    );
  }
}
