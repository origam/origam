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
import { IFocusable } from "../../../../model/entities/FormFocusManager";
import CS from "gui/Components/ScreenElements/Editors/CommonStyle.module.css";
import cx from "classnames";

export interface IRawCheckListProps {
  api: IApi;
  value: string[];
  onClick: () => void;
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
  isInvalid: boolean;
  isReadonly?: boolean;
  invalidMessage?: string;
  subscribeToFocusManager?: (obj: IFocusable) => void;

  onChange?(newValue: string[]): void;
  onKeyDown(event: any): void;
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
  isInvalid: boolean;
  isReadonly?: boolean;
  invalidMessage?: string;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  onKeyDown(event: any): void;
  onClick: () => void;
}> = observer((props) => {
  const { property } = useContext(MobXProviderContext);

  return (
    <CheckListRaw
      value={props.value}
      onChange={!props.isReadonly ? props.onChange : undefined}
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
      isInvalid={props.isInvalid}
      isReadonly={props.isReadonly}
      invalidMessage={props.invalidMessage}
      subscribeToFocusManager={props.subscribeToFocusManager}
      onKeyDown={props.onKeyDown}
      onClick={props.onClick}
    />
  );
});

export const CheckListRaw: React.FC<IRawCheckListProps> = observer((props) => {
  const [controller] = useState(() => new CheckListControler());
  controller.props = props;

  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(controller.loadLookupList, [props.RowId]);

  const inputRefs: InputReference[] = [];

  function focusLeft(x: number, y: number) {
    const inputsOnTheLeft = inputRefs
      .filter((input) => input.hasYEqualTo(y) && input.isOnTheLeftOf(x))
      .sort((i1, i2) => i2.x - i1.x);

    if (inputsOnTheLeft.length > 0) {
      inputsOnTheLeft[0].focus();
    }
  }

  function focusRight(x: number, y: number) {
    const inputsOnTheRight = inputRefs
      .filter((input) => input.hasYEqualTo(y) && input.isOnTheRightOf(x))
      .sort((i1, i2) => i1.x - i2.x);

    if (inputsOnTheRight.length > 0) {
      inputsOnTheRight[0].focus();
    }
  }

  function focusUp(x: number, y: number) {
    const inputsAbove = inputRefs
      .filter((input) => input.hasXEqualTo(x) && input.isAbove(y))
      .sort((i1, i2) => i2.y - i1.y);

    if (inputsAbove.length > 0) {
      inputsAbove[0].focus();
    }
  }

  function focusDown(x: number, y: number) {
    const inputsBelow = inputRefs
      .filter((input) => input.hasXEqualTo(x) && input.isBelow(y))
      .sort((i1, i2) => i1.y - i2.y);

    if (inputsBelow.length > 0) {
      inputsBelow[0].focus();
    }
  }

  return (
    <div className={S.editorContainer}>
      <div className={cx(S.root, { isReadonly: props.isReadonly })}>
        {controller.items.map((item, i) => (
          <CheckListItem
            key={item.value}
            checked={props.value && !!props.value.find((v) => v === item.value)}
            onClick={(event) => {
              props.onClick();
              if (!props.isReadonly) {
                controller.handleClick(event, item);
              }
            }}
            // tabIndex={i === 0 ? props.tabIndex : -1}
            subscribeToFocusManager={i === 0 ? props.subscribeToFocusManager : undefined}
            inputSetter={(inputRef: InputReference) => inputRefs.push(inputRef)}
            focusLeft={focusLeft}
            focusRight={focusRight}
            focusUp={focusUp}
            focusDown={focusDown}
            onKeyDown={!props.isReadonly ? props.onKeyDown : undefined}
            label={item.label}
          />
        ))}
      </div>
      {props.isInvalid && (
        <div className={CS.notification} title={props.invalidMessage}>
          <i className="fas fa-exclamation-circle red" />
        </div>
      )}
    </div>
  );
});

export const CheckListItem: React.FC<{
  checked: boolean;
  onClick?(event: any): void;
  tabIndex?: number;
  inputSetter: (inputRef: InputReference) => void;
  focusLeft: (x: number, y: number) => void;
  focusRight: (x: number, y: number) => void;
  focusUp: (x: number, y: number) => void;
  focusDown: (x: number, y: number) => void;
  label: string;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  onKeyDown?(event: any): void;
}> = (props) => {
  const [isFocused, setIsFocused] = useState<boolean>(false);

  const refInput = useRef<HTMLInputElement>(null);
  props.inputSetter(new InputReference(refInput));

  useEffect(() => {
    if (props.subscribeToFocusManager && refInput.current) {
      props.subscribeToFocusManager(refInput.current);
    }
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  function onKeyDown(event: any) {
    const boundingRect = refInput.current?.getBoundingClientRect()!;
    switch (event.key) {
      case "Tab":
        props.onKeyDown?.(event);
        break;
      case "ArrowUp":
        event.preventDefault();
        props.focusUp(boundingRect.x, boundingRect.y);
        break;
      case "ArrowDown":
        event.preventDefault();
        props.focusDown(boundingRect.x, boundingRect.y);
        break;
      case "ArrowRight":
        event.preventDefault();
        props.focusRight(boundingRect.x, boundingRect.y);
        break;
      case "ArrowLeft":
        event.preventDefault();
        props.focusLeft(boundingRect.x, boundingRect.y);
        break;
    }
  }

  function onInputFocus() {
    setIsFocused(true);
  }

  function onInputBlur() {
    setIsFocused(false);
  }

  function onClick(event: any) {
    props.onClick && props.onClick(event);
    refInput?.current?.focus();
  }



  return (
    <div className={S.item} onClick={onClick}>
      <input
        ref={refInput}
        type="checkbox"
        className={"checkbox " + S.input}
        checked={props.checked}
        tabIndex={props.tabIndex ? props.tabIndex : undefined}
        onKeyDown={onKeyDown}
        onFocus={onInputFocus}
        onBlur={onInputBlur}
      />
      <div className={"content " + (isFocused ? S.focusedLabel : S.unFocusedLabel)}>
        {props.label}
      </div>
    </div>
  );
};

class InputReference {
  constructor(private inputRef: RefObject<HTMLInputElement>) {}

  get x() {
    return this.inputRef.current?.getBoundingClientRect()!.x || 0;
  }

  get y() {
    return this.inputRef.current?.getBoundingClientRect()!.y || 0;
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
