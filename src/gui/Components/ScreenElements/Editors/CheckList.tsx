import React, { useEffect, useState, useContext } from "react";
import S from "./CheckList.module.scss";
import { observer, MobXProviderContext } from "mobx-react";
import { action, flow, observable, computed } from "mobx";
import { IApi } from "model/entities/types/IApi";
import { lookup } from "dns";
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

  onChange?(newValue: string[]): void;
}

export class CheckListControler {
  constructor() {}

  @observable lookupList: string[][] = [];

  @computed get items() {
    return this.lookupList.map(llitem => ({
      value: llitem[0],
      label: llitem[1]
    }));
  }

  @action.bound
  loadLookupList() {
    const self = this;
    flow(function*() {
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
        PageNumber: 1
      });
      self.lookupList = lookupList;
    })();
  }

  @action.bound handleClick(event: any, item: { value: string; label: string }) {
    event.preventDefault();
    const currentIndex = this.props.value.findIndex(id => item.value === id);
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
}> = observer(props => {
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
    />
  );
});

export const CheckListRaw: React.FC<IRawCheckListProps> = observer(props => {
  const [controller] = useState(() => new CheckListControler());
  controller.props = props;
  useEffect(controller.loadLookupList, [props.RowId]);
  console.log(props.value);
  return (
    <div className={S.root}>
      {controller.items.map(item => (
        <CheckListItem
          key={item.value}
          checked={!!props.value.find(v => v === item.value)}
          onClick={event => {
            controller.handleClick(event, item);
          }}
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
}> = props => {
  return (
    <div className={S.item} onClick={props.onClick}>
      <input type="checkbox" className="checkbox" checked={props.checked} />
      <div className={"content"}>{props.children}</div>
    </div>
  );
};
