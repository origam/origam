import React, { RefObject, useContext, useEffect, useRef, useState } from "react";
import S from "./CheckList.module.scss";
import { MobXProviderContext, observer } from "mobx-react";
import { action, computed, flow, observable } from "mobx";
import { IApi } from "model/entities/types/IApi";
import { getApi } from "model/selectors/getApi";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getSessionId } from "model/selectors/getSessionId";

export interface IRawCheckListProps {
  api: IApi;
  value: string[];

  Entity: string;
  SessionFormIdentifier: string;
  DataStructureEntityId: string;
  Identifier: string;
  ColumnNames: string[];
  Property: string;
  RowId: string | undefined;
  LookupId: string;
  Parameters: any;
  menuItemId: string;
  tabIndex?: number;

  onChange?(newValue: string[]): void;
}

export class CheckListControler {
  @observable lookupList: string[][] = [];

  @computed get items() {
    return this.lookupList.map((llitem) => ({
      value: llitem[0],
      label: llitem[1],
    }));
  }

  @action.bound
  loadLookupList() {
    const self = this;
    flow(function* () {
      const lookupList = yield self.props.api.getLookupList({
        Entity: self.props.Entity,
        SessionFormIdentifier: self.props.SessionFormIdentifier,
        DataStructureEntityId: self.props.DataStructureEntityId, // Data view entity identifier
        ColumnNames: [self.props.Identifier || "Id", ...self.props.ColumnNames], // Columns to download
        Property: self.props.Property!, // Columnn Id
        Id: self.props.RowId!, // Id of the selected row
        LookupId: self.props.LookupId!, // Id of the lookup object
        Parameters: self.props.Parameters!,
        MenuId: self.props.menuItemId!,
        ShowUniqueValues: false,
        SearchText: "",
        PageSize: 10000,
        PageNumber: 1,
      });
      self.lookupList = lookupList;
    })();
  }

  @action.bound handleClick(event: any, item: { value: string; label: string }) {
    event.preventDefault();
    const currentIndex = this.props.value.findIndex((id) => item.value === id);
    if (currentIndex > -1) {
      const newValue = [...this.props.value];
      newValue.splice(currentIndex, 1);
      this.props.onChange && this.props.onChange(newValue);
      return;
    } else {
      const newValue = [...this.props.value, item.value];
      this.props.onChange && this.props.onChange(newValue);
    }
  }

  @observable.ref props: IRawCheckListProps = undefined as any;
}

export const CheckList: React.FC<{
  value: string[];
  onChange?(newValue: string[]): void;
  tabIndex?: number;
}> = observer((props) => {
  const { property } = useContext(MobXProviderContext);

  return (
    <CheckListRaw
      value={props.value}
      onChange={props.onChange}
      api={getApi(property)}
      DataStructureEntityId={getDataStructureEntityId(property)}
      ColumnNames={property!.lookup.dropDownColumns.map((column: any) => column.id)}
      Property={property.id}
      Parameters={property!.lookup.parameters}
      RowId={getSelectedRowId(property)}
      Identifier={property.identifier}
      LookupId={property!.lookup.lookupId}
      menuItemId={getMenuItemId(property)}
      Entity={getEntity(property)}
      SessionFormIdentifier={getSessionId(property)}
      tabIndex={props.tabIndex}
    />
  );
});

export const CheckListRaw: React.FC<IRawCheckListProps> = observer(props => {
  const [controller] = useState(() => new CheckListControler());
  controller.props = props;
  useEffect(controller.loadLookupList, [props.RowId]);

  const inputRefs: InputReference[] = [];

  function focusLeft(x: number, y: number) {
    const inputsOnTheLeft = inputRefs.filter(input => input.hasYEqualTo(y) && input.isOnTheLeftOf(x))
      .sort((i1, i2) => i2.x - i1.x);

    if (inputsOnTheLeft.length > 0) {
      inputsOnTheLeft[0].focus();
    }
  }

  function focusRight(x: number, y: number){
    const inputsOnTheRight = inputRefs.filter(input => input.hasYEqualTo(y) && input.isOnTheRightOf(x))
      .sort((i1, i2) => i1.x - i2.x);

    if(inputsOnTheRight.length > 0){
      inputsOnTheRight[0].focus();
    }
  }

  function focusUp(x: number, y: number){
    const inputsAbove = inputRefs.filter(input => input.hasXEqualTo(x) && input.isAbove(y))
      .sort((i1, i2) => i2.y - i1.y);

    if(inputsAbove.length > 0){
      inputsAbove[0].focus();
    }
  }


  function focusDown(x: number, y: number){
    const inputsBelow = inputRefs.filter(input => input.hasXEqualTo(x) && input.isBelow(y))
      .sort((i1, i2) => i1.y - i2.y);

    if(inputsBelow.length > 0){
      inputsBelow[0].focus();
    }
  }


  return (
    <div className={S.root}>
      {controller.items.map((item, i) => (
        <CheckListItem
          key={item.value}
          checked={!!props.value.find((v) => v === item.value)}
          onClick={(event) => {
            controller.handleClick(event, item);
          }}
          tabIndex={i === 0 ? props.tabIndex : -1}
          inputSetter={(inputRef: InputReference) => inputRefs.push(inputRef)}
          focusLeft={focusLeft}
          focusRight={focusRight}
          focusUp={focusUp}
          focusDown={focusDown}
        >
          {item.label}
        </CheckListItem>
      ))}
    </div>
  );
});

export const CheckListItem: React.FC<{
  checked: boolean;
  onClick?(event: any): void;
  tabIndex?: number;
  inputSetter: (inputRef: InputReference) => void;
  focusLeft: (x: number, y: number)=>void;
  focusRight: (x: number, y: number)=>void;
  focusUp: (x: number, y: number)=>void;
  focusDown: (x: number, y: number)=>void;
}> = (props) => {
  function onKeyDown(event: any) {
    const boundingRect = refInput.current?.getBoundingClientRect()!;
    event.preventDefault();
    switch (event.key) {
      case "ArrowUp":
        props.focusUp(boundingRect.x, boundingRect.y);
        break;
      case "ArrowDown":
        props.focusDown(boundingRect.x, boundingRect.y);
        break;
      case "ArrowRight":
        props.focusRight(boundingRect.x, boundingRect.y);
        break;
      case "ArrowLeft":
        props.focusLeft(boundingRect.x, boundingRect.y);
        break;
    }
  }

  const refInput = useRef<HTMLInputElement>(null);
  props.inputSetter(new InputReference(refInput));

  return (
    <div className={S.item} onClick={props.onClick}>
      <input
        ref={refInput}
        type="checkbox"
        className={"checkbox " + S.input}
        checked={props.checked}
        tabIndex={props.tabIndex ? props.tabIndex : undefined}
        onKeyDown={onKeyDown}
      />
      <div className={"content"}>{props.children}</div>
    </div>
  );
};

class InputReference {
  constructor(private inputRef: RefObject<HTMLInputElement>) {
  }

  get x() {
    return this.inputRef.current?.getBoundingClientRect()!.x;
  }

  get y() {
    return this.inputRef.current?.getBoundingClientRect()!.y;
  }

  hasXEqualTo(x: number) {
    return Math.abs(this.x - x) < 1;
  }

  hasYEqualTo(y: number) {
    return Math.abs(this.y - y) < 1;
  }

  isOnTheLeftOf(x: number) {
    return this.x < x;
  }

  isOnTheRightOf(x: number) {
    return this.x > x;
  }

  isAbove(y: number) {
    return this.y < y;
  }

  isBelow(y: number) {
    return this.y > y;
  }

  focus() {
    this.inputRef.current?.focus();
  }
}
