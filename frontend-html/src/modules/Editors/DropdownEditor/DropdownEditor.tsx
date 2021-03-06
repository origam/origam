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
import React, { createContext, useContext, useEffect, useState } from "react";
import { DropdownLayout, DropdownLayoutBody } from "@origam/components";
import { DropdownEditorApi } from "./DropdownEditorApi";
import { DropdownEditorBehavior} from "./DropdownEditorBehavior";
import { DropdownEditorBody } from "./DropdownEditorBody";
import { DropdownEditorControl } from "./DropdownEditorControl";
import { DropdownEditorData, IDropdownEditorData } from "./DropdownEditorData";
import { DropdownEditorLookupListCache } from "./DropdownEditorLookupListCache";
import { DropdownDataTable } from "./DropdownTableModel";
import { TagInputEditorData } from "./TagInputEditorData";
import { IFocusable } from "../../../model/entities/FormFocusManager";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { DropdownEditorSetup, DropdownEditorSetupFromXml } from "modules/Editors/DropdownEditor/DropdownEditorSetup";

export interface IDropdownEditorContext {
  behavior: DropdownEditorBehavior;
  editorData: IDropdownEditorData;
  editorDataTable: DropdownDataTable;
  setup: DropdownEditorSetup;
}

export const CtxDropdownEditor = createContext<IDropdownEditorContext>(null as any);

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
  const {dataViewRowCursor, dataViewApi, dataViewData} = mobxContext.dataView;
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

    const dropdownEditorBehavior = new DropdownEditorBehavior({
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

    const dropdownEditorSetup = DropdownEditorSetupFromXml(
      props.xmlNode, dropdownEditorDataTable, dropdownEditorBehavior, props.isLink);

    return {
      behavior: dropdownEditorBehavior,
      editorData: dropdownEditorData,
      columnDrivers: dropdownEditorSetup.columnDrivers,
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

  return (
    <CtxDropdownEditor.Provider value={dropdownEditorInfrastructure}>
      <DropdownEditor
        editor={props.tagEditor}
        backgroundColor={props.backgroundColor}
        foregroundColor={props.foregroundColor}
        customStyle={props.customStyle}
      />
    </CtxDropdownEditor.Provider>
  );
}
