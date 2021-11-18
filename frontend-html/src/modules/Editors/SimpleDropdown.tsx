import {observer} from "mobx-react";
import React, {useContext} from "react";
import {observable} from "mobx";
import {DropdownLayout} from "modules/Editors/DropdownEditor/Dropdown/DropdownLayout";
import {DropdownLayoutBody} from "modules/Editors/DropdownEditor/Dropdown/DropdownLayoutBody";
import {
  CtxDropdownRefBody,
  CtxDropdownRefCtrl
} from "modules/Editors/DropdownEditor/Dropdown/DropdownCommon";
import S from "modules/Editors/SimpleDropdown.module.scss";
import CS from "modules/Editors/DropdownEditor/Dropdown/Dropdown.module.scss";

@observer
export class SimpleDropdown<T> extends React.Component<{
  width: string,
  options: IOption<T>[],
  selectedOption: IOption<T>,
  onOptionClick: (option: IOption<T>) => void
}> {

  @observable
  isDroped = false;

  onOptionClick(option: IOption<T>){
    this.isDroped = false;
    this.props.onOptionClick(option);
  }

  render() {
    return (
      <DropdownLayout
        isDropped={this.isDroped}
        renderCtrl={() => (
          <DropDownControl
            width={this.props.width}
            onClick={() => this.isDroped = !this.isDroped}
            value={this.props.selectedOption.label}
          />
        )}
        renderDropdown={() => (
          <DropdownLayoutBody
            render={() => (
              <DropDownBody
                width={this.props.width}
                options={this.props.options}
                selected={this.props.selectedOption}
                onOptionClick={option => this.onOptionClick(option)}
              />
            )}
          />
        )}
      />
    );
  }
}


export function DropDownControl(props: {
  onClick: ()=> void;
  value: string;
  width: string
}) {
  const ref = useContext(CtxDropdownRefCtrl);

  return (
    <div
      style={{width: props.width}}
      onClick={()=>props.onClick()}
      ref={ref}
      className={CS.control}>
      <input
        className="input"
        value={props.value}
        disabled={true}
      />
      <div
        className="inputBtn lastOne">
        <i className="fas fa-caret-down"></i>
      </div>
    </div>
  )
}


export function DropDownBody<T>(props: {
  width: string,
  options: IOption<T>[],
  selected: IOption<T>,
  onOptionClick: (option: IOption<T>) => void
}) {
  const ref = useContext(CtxDropdownRefBody);

  return (
    <div
      ref={ref}
      className={S.body}
      style={{width: props.width}}>
      {props.options.map((option, i) =>
        <div
          className={S.cell + " " + (i%2 ? S.ord2 : S.ord1) + " " + (option.value === props.selected.value ? S.selected : "")}
          onClick={() => props.onOptionClick(option)}>{option.label}</div>)
      }
    </div>
  )
}

export interface IOption<T>{
  value: T
  label: string
}
