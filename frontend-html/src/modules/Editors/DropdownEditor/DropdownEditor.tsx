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

import { TypeSymbol } from "dic/Container";
import { MobXProviderContext, Observer } from "mobx-react";
import React, { createContext, ReactNode, useContext, useEffect, useState } from "react";
import { findStopping } from "xmlInterpreters/xmlUtils";
import { BooleanCellDriver } from "./Cells/BooleanCellDriver";
import { DefaultHeaderCellDriver } from "./Cells/HeaderCell";
import { NumberCellDriver } from "./Cells/NumberCellDriver";
import { TextCellDriver } from "./Cells/TextCellDriver";
import { DropdownLayout, DropdownLayoutBody } from "@origam/components";
import { DropdownEditorApi } from "./DropdownEditorApi";
import { DropdownEditorBehavior, IBehaviorData, IDropdownEditorBehavior } from "./DropdownEditorBehavior";
import { DropdownEditorBody } from "./DropdownEditorBody";
import { DropdownEditorControl } from "./DropdownEditorControl";
import { DropdownEditorData, IDropdownEditorData } from "./DropdownEditorData";
import { DropdownEditorLookupListCache } from "./DropdownEditorLookupListCache";
import { DropdownColumnDrivers, DropdownDataTable } from "./DropdownTableModel";
import { IDataView } from "../../../model/entities/types/IDataView";
import { TagInputEditorData } from "./TagInputEditorData";
import { IFocusable } from "../../../model/entities/FormFocusManager";
import { DateCellDriver } from "./Cells/DateCellDriver";
import { getMomentFormat } from "../../../xmlInterpreters/getMomentFormat";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";

export interface IDropdownEditorContext {
  behavior: IDropdownEditorBehavior;
  editorData: IDropdownEditorData;
  editorDataTable: DropdownDataTable;
  columnDrivers: DropdownColumnDrivers;
  setup: DropdownEditorSetup;
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
    public searchByFirstColumnOnly: boolean,
    public isLink?: boolean
  ) {
  }
}

export const IGetDropdownEditorSetup = TypeSymbol<() => DropdownEditorSetup>(
  "IGetDropdownEditorSetup"
);

export function DropdownEditor(props: {
  editor?: JSX.Element;
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
}) {
  const beh = useContext(CtxDropdownEditor).behavior;
  const workbench = useContext(MobXProviderContext).workbench as IWorkbench;
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
                backgroundColor={props.backgroundColor}
                foregroundColor={props.foregroundColor}
                customStyle={props.customStyle}
              />
            )
          }
          renderDropdown={() =>
            <DropdownLayoutBody
              render={() => <DropdownEditorBody/>}
              minSideMargin={isMobileLayoutActive(workbench) ? 20 : 50}
            />
          }
        />
      )}
    </Observer>
  );
}

export function XmlBuildDropdownEditor(props: {
  xmlNode: any;
  isReadOnly: boolean;
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
  tagEditor?: JSX.Element;
  isLink?: boolean;
  autoSort?: boolean;
  onTextOverflowChanged?: (toolTip: string | null | undefined) => void;
  onDoubleClick?: (event: any) => void;
  onClick?: (event: any) => void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  onKeyDown?(event: any): void;
}) {
  const mobxContext = useContext(MobXProviderContext);
  return(
    <XmlBuildDropdownEditorInternal
      {...props}
      dataView={mobxContext.dataView as IDataView}
      control={
        <DropdownEditor
          editor={props.tagEditor}
          backgroundColor={props.backgroundColor}
          foregroundColor={props.foregroundColor}
          customStyle={props.customStyle}
        />
      }
      makeBehavior={data => new DropdownEditorBehavior(data)}
    />
  );
}


export function XmlBuildDropdownEditorInternal(props: {
  xmlNode: any;
  isReadOnly: boolean;
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
  tagEditor?: JSX.Element;
  isLink?: boolean;
  autoSort?: boolean;
  onTextOverflowChanged?: (toolTip: string | null | undefined) => void;
  onDoubleClick?: (event: any) => void;
  onClick?: (event: any) => void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  onKeyDown?(event: any): void;
  dataView: IDataView
  control: ReactNode,
  mobileBehavior?: boolean,
  makeBehavior: (data: IBehaviorData)=> IDropdownEditorBehavior;
}) {
  const mobxContext = useContext(MobXProviderContext);
  const {dataViewRowCursor, dataViewApi, dataViewData} = props.dataView;
  const workbench = mobxContext.workbench;
  const {lookupListCache} = workbench;

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

    const dropdownEditorBehavior = props.makeBehavior({
      api: dropdownEditorApi,
      data: dropdownEditorData,
      dataTable: dropdownEditorDataTable,
      setup: () => dropdownEditorSetup,
      cache: dropdownEditorLookupListCache,
      isReadOnly: props.isReadOnly,
      onDoubleClick: props.onDoubleClick,
      onClick: props.onClick,
      subscribeToFocusManager: props.subscribeToFocusManager,
      onKeyDown: props.onKeyDown,
      autoSort: props.autoSort,
      onTextOverflowChanged: props.onTextOverflowChanged,
    });


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
    if (rat.SuppressEmptyColumns === "true") {
      drivers.driversFilter = (driver) => {
        return dropdownEditorDataTable.columnIdsWithNoData.indexOf(driver.columnId) < 0;
      };
    }
    for (let propertyXml of findStopping(props.xmlNode, (n) => n.name === "Property")) {
      index++;
      const attributes = propertyXml.attributes;
      const id = attributes.Id;
      columnNames.push(id);
      columnNameToIndex.set(id, index);

      const formatterPattern = attributes.FormatterPattern
        ? getMomentFormat(propertyXml)
        : "";
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

      drivers.allDrivers.push({
        columnId: id,
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
      setup: dropdownEditorSetup,
    };
  });

  useEffect(() => {
      dropdownEditorInfrastructure.behavior.isReadOnly = props.isReadOnly;

    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [props.isReadOnly, dropdownEditorInfrastructure.behavior.isReadOnly]
  );

  dropdownEditorInfrastructure.behavior.onClick = props.onClick;
  dropdownEditorInfrastructure.behavior.onDoubleClick = props.onDoubleClick;
  dropdownEditorInfrastructure.setup.isLink = props.isLink;

  return (
    <CtxDropdownEditor.Provider value={dropdownEditorInfrastructure}>
      {props.control}
    </CtxDropdownEditor.Provider>
  );
}

