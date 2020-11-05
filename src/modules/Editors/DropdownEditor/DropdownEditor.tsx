import { TypeSymbol } from "dic/Container";
import { MobXProviderContext, Observer } from "mobx-react";
import React, { createContext, useContext, useState, useEffect } from "react";
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
import { TagInputEditorData } from "./TagInputEditorData";
import { IFocusable } from "../../../model/entities/FocusManager";
import { DateCellDriver } from "./Cells/DateCellDriver";
import {flf2mof} from "utils/flashDateFormat";

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

export function DropdownEditor(props: {
  editor?: JSX.Element;
  isInvalid?: boolean;
  invalidMessage?: string;
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
}) {
  const beh = useContext(CtxDropdownEditor).behavior;
  return (
    <Observer>
      {() => (
        <DropdownLayout
          isDropped={beh.isBodyDisplayed}
          onDropupRequest={beh.dropUp}
          renderCtrl={() =>
            props.editor ? (
              props.editor
            ) : (
              <DropdownEditorControl
                isInvalid={props.isInvalid}
                invalidMessage={props.invalidMessage}
                backgroundColor={props.backgroundColor}
                foregroundColor={props.foregroundColor}
                customStyle={props.customStyle}
              />
            )
          }
          renderDropdown={() => <DropdownLayoutBody render={() => <DropdownEditorBody />} />}
        />
      )}
    </Observer>
  );
}

export function XmlBuildDropdownEditor(props: {
  xmlNode: any;
  isReadOnly: boolean;
  isInvalid?: boolean;
  invalidMessage?: string;
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
  tagEditor?: JSX.Element;
  onDoubleClick?: (event: any) => void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  onKeyDown?(event: any): void;
}) {
  const mobxContext = useContext(MobXProviderContext);
  const dataView = mobxContext.dataView as IDataView;
  const { dataViewRowCursor, dataViewApi, dataViewData } = dataView;
  const workbench = mobxContext.workbench;
  const { lookupListCache } = workbench;

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
      props.isReadOnly,
      props.onDoubleClick,
      props.subscribeToFocusManager,
      props.onKeyDown
    );

    const rat = props.xmlNode.attributes;
    const lookupId = rat.LookupId;
    const propertyId = rat.Id;
    const showUniqueValues = rat.DropDownShowUniqueValues === "true";
    const identifier = rat.Identifier;
    let identifierIndex = 0;
    const dropdownType = rat.DropDownType;
    const cached = rat.Cached === "true";
    const searchByFirstColumnOnly = rat.SearchByFirstColumnOnly === "true";

    const columnNames: string[] = [identifier];
    const visibleColumnNames: string[] = [];
    const columnNameToIndex = new Map<string, number>([[identifier, identifierIndex]]);
    let index = 0;
    const drivers = new DropdownColumnDrivers();
    for (let propertyXml of findStopping(props.xmlNode, (n) => n.name === "Property")) {
      index++;
      const attributes = propertyXml.attributes;
      const id = attributes.Id;
      columnNames.push(id);
      columnNameToIndex.set(id, index);

      const formatterPattern =  attributes.FormatterPattern
        ? flf2mof(attributes.FormatterPattern)
        : ""
      visibleColumnNames.push(id);
      const name = attributes.Name;
      const column = attributes.Column;

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
        case "Date":
          bodyCellDriver = new DateCellDriver(
            index,
            dropdownEditorDataTable,
            dropdownEditorBehavior,
            formatterPattern
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

  useEffect(() => {
    dropdownEditorInfrastructure.behavior.isReadOnly = props.isReadOnly;
  }, [props.isReadOnly]);

  return (
    <CtxDropdownEditor.Provider value={dropdownEditorInfrastructure}>
      <DropdownEditor
        editor={props.tagEditor}
        isInvalid={props.isInvalid}
        invalidMessage={props.invalidMessage}
        backgroundColor={props.backgroundColor}
        foregroundColor={props.foregroundColor}
        customStyle={props.customStyle}
      />
    </CtxDropdownEditor.Provider>
  );
}
