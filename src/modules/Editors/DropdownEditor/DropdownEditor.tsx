import { TypeSymbol } from "dic/Container";
import { MobXProviderContext, Observer } from "mobx-react";
import React, { createContext, useContext, useState } from "react";
import { findStopping } from "xmlInterpreters/xmlUtils";
import { BooleanCellDriver } from "./Cells/BooleanCellDriver";
import { DefaultHeaderCellDriver } from "./Cells/HeaderCell";
import { NumberCellDriver } from "./Cells/NumberCellDriver";
import { TextCellDriver } from "./Cells/TextCellDriver";
import { DropdownLayout } from "./Dropdown/DropdownLayout";
import { DropdownLayoutBody } from "./Dropdown/DropdownLayoutBody";
import { DropdownEditorApi } from "./DropdownEditorApi";
import { DropdownEditorBehavior } from "./DropdownEditorBehavior";
import { DropdownEditorBody } from "./DropdownEditorBody";
import { DropdownEditorControl } from "./DropdownEditorControl";
import { DropdownEditorData, IDropdownEditorData } from "./DropdownEditorData";
import { DropdownEditorLookupListCache } from "./DropdownEditorLookupListCache";
import { DropdownColumnDrivers, DropdownDataTable } from "./DropdownTableModel";
import { IDataView } from "../../../model/entities/types/IDataView";
import { CtxDropdownRefCtrl } from "./Dropdown/DropdownCommon";
import { TagInputEditorData } from "./TagInputEditorData";

export interface IDropdownEditorContext {
  behavior: DropdownEditorBehavior;
  editorData: IDropdownEditorData;
  editorDataTable: DropdownDataTable;
  columnDrivers: DropdownColumnDrivers;
}

export const CtxDropdownEditor = createContext<IDropdownEditorContext>(null as any);

export class DropdownEditorSetup {
  constructor(
    public propertyId: string,
    public lookupId: string,
    public columnNames: string[],
    public visibleColumnNames: string[],
    public columnNameToIndex: Map<string, number>,
    public showUniqueValues: boolean,
    public identifier: string,
    public identifierIndex: number,
    public parameters: { [key: string]: any },
    public dropdownType: string,
    public cached: boolean,
    public searchByFirstColumnOnly: boolean
  ) {}
}
export const IGetDropdownEditorSetup = TypeSymbol<() => DropdownEditorSetup>(
  "IGetDropdownEditorSetup"
);

export function DropdownEditor(props: { editor?: JSX.Element }) {
  const beh = useContext(CtxDropdownEditor).behavior;
  return (
    <Observer>
      {() => (
        <DropdownLayout
          isDropped={beh.isDropped}
          renderCtrl={() => (props.editor ? props.editor : <DropdownEditorControl />)}
          renderDropdown={() => <DropdownLayoutBody render={() => <DropdownEditorBody />} />}
        />
      )}
    </Observer>
  );
}

export function XmlBuildDropdownEditor(props: {
    xmlNode: any; isReadOnly: boolean; tagEditor?: JSX.Element }) {
  const mobxContext = useContext(MobXProviderContext);
  const dataView = mobxContext.dataView as IDataView;
  const { dataViewRowCursor, dataViewApi, dataViewData } = dataView;
  const workbench = mobxContext.workbench;
  const { lookupListCache } = workbench;

  // const dataViewRowCursor = new RowCursor(() => null);
  // const dataViewApi = new DataViewAPI(() => null, () => null, null);
  // const dataViewData = new DataViewData(() => null, (propId) => null);

  const [dropdownEditorInfrastructure] = useState<IDropdownEditorContext>(() => {
    const dropdownEditorApi: DropdownEditorApi = new DropdownEditorApi(
      () => dropdownEditorSetup,
      dataViewRowCursor,
      dataViewApi,
      () => dropdownEditorBehavior
    );
    const dropdownEditorData: IDropdownEditorData = props.tagEditor
      ? new TagInputEditorData(dataViewData, dataViewRowCursor, () => dropdownEditorSetup)
      : new DropdownEditorData(dataViewData, dataViewRowCursor, () => dropdownEditorSetup);
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
      props.isReadOnly
    );

    const rat = props.xmlNode.attributes;
    const lookupId = rat.LookupId;
    const propertyId = rat.Id;
    const showUniqueValues = rat.DropDownShowUniqueValues === "true";
    const identifier = rat.Identifier;
    let identifierIndex = 0; //= parseInt(rat.IdentifierIndex, 10);
    const dropdownType = rat.DropDownType;
    const cached = rat.Cached === "true";
    const searchByFirstColumnOnly = rat.SearchByFirstColumnOnly === "true";

    const columnNames: string[] = [identifier];
    const visibleColumnNames: string[] = [];
    const columnNameToIndex = new Map<string, number>([[identifier, identifierIndex]]);
    let index = 0;
    const drivers = new DropdownColumnDrivers();
    for (let ddp of findStopping(props.xmlNode, (n) => n.name === "Property")) {
      index++;
      const pat = ddp.attributes;
      const id = pat.Id;
      //const index = parseInt(pat.Index, 10);
      columnNames.push(id);
      columnNameToIndex.set(id, index);

      visibleColumnNames.push(id);
      const name = pat.Name;
      const column = pat.Column;
      const entity = pat.Entity;

      let bodyCellDriver;
      switch (column) {
        case "Text":
          bodyCellDriver = new TextCellDriver(
            index,
            dropdownEditorDataTable,
            dropdownEditorBehavior
          );
          break;
        case "Number":
          bodyCellDriver = new NumberCellDriver(
            index,
            dropdownEditorDataTable,
            dropdownEditorBehavior
          );
          break;
        case "CheckBox":
          bodyCellDriver = new BooleanCellDriver(
            index,
            dropdownEditorDataTable,
            dropdownEditorBehavior
          );
          break;
        default:
          throw new Error("Unknown column type " + column);
      }

      drivers.drivers.push({
        headerCellDriver: new DefaultHeaderCellDriver(name),
        bodyCellDriver,
      });
    }

    const parameters: { [k: string]: any } = {};

    for (let ddp of findStopping(props.xmlNode, (n) => n.name === "ComboBoxParameterMapping")) {
      const pat = ddp.attributes;
      parameters[pat.ParameterName] = pat.FieldName;
    }

    const dropdownEditorSetup = new DropdownEditorSetup(
      propertyId,
      lookupId,
      columnNames,
      visibleColumnNames,
      columnNameToIndex,
      showUniqueValues,
      identifier,
      identifierIndex,
      parameters,
      dropdownType,
      cached,
      searchByFirstColumnOnly
    );

    return {
      behavior: dropdownEditorBehavior,
      editorData: dropdownEditorData,
      columnDrivers: drivers,
      editorDataTable: dropdownEditorDataTable,
    };
  });

  return (
    <CtxDropdownEditor.Provider value={dropdownEditorInfrastructure}>
      <DropdownEditor editor={props.tagEditor} />
    </CtxDropdownEditor.Provider>
  );
}
