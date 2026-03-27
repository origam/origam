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

import { TabbedViewHandle } from "gui/Components/TabbedView/TabbedViewHandle";
import { TabbedViewHandleRow } from "gui/Components/TabbedView/TabbedViewHandleRow";
import { ErrorBoundaryEncapsulated } from "gui/Components/Utilities/ErrorBoundary";
import { Dropdown } from "gui/Components/Dropdown/Dropdown";
import { DropdownItem } from "gui/Components/Dropdown/DropdownItem";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { MobXProviderContext, observer } from "mobx-react";
import {
  onScreenTabCloseClick,
  onScreenTabCloseMouseDown,
  onScreenTabCloseAllUnchangedClick,
  onScreenTabCloseAllUnchangedButThisClick,
} from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { onScreenTabHandleClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabHandleClick";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getIsFormScreenDirty } from "model/selectors/FormScreen/getisFormScreenDirty";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import React from "react";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";
import { getIsScreenOrAnyDataViewWorking } from "model/selectors/FormScreen/getIsScreenOrAnyDataViewWorking";
import { isLazyLoading } from "model/selectors/isLazyLoading";
import { T } from "utils/translation";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";

@observer
export class CScreenTabbedViewHandleRow extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  render() {
    const openedScreenItems = getOpenedNonDialogScreenItems(this.workbench);

    return (
      <TabbedViewHandleRow className={"noPrint"}>
        {openedScreenItems.map((item) => (
          <ErrorBoundaryEncapsulated ctx={item} key={`${item.menuItemId}@${item.order}`}>
            <CScreenTabbedViewHandle item={item}/>
          </ErrorBoundaryEncapsulated>
        ))}
      </TabbedViewHandleRow>
    );
  }
}

@observer
class CScreenTabbedViewHandle extends React.Component<{ item: IOpenedScreen }> {

  getIsLoading(){
    const {item} = this.props;
    if(item.screenUrl){
      return false;
    }
    const content = getOpenedScreen(item).content;
    if(!content){
      return false;
    }
    const isEagerLoading =  !isLazyLoading(item)
    return isEagerLoading && (content.isLoading || getIsScreenOrAnyDataViewWorking(content.formScreen!))
  }

  render() {
    const {item} = this.props;
    const label = getLabel(item);
    const isLoading = this.getIsLoading();
    return (
      <Dropdowner
        style={{width: "auto", minWidth: 0, flexShrink: 1, alignItems: "end"}}
        trigger={({refTrigger, setDropped}) => (
          <TabbedViewHandle
            refDom={refTrigger}
            title={label}
            key={`${item.menuItemId}@${item.order}`}
            isActive={item.isActive}
            hasCloseBtn={true}
            isDirty={getIsFormScreenDirty(item)}
            onClick={(event: any) => onScreenTabHandleClick(item)(event)}
            onContextMenu={(event: any) => {
              setDropped(true, event);
              event.preventDefault();
              event.stopPropagation();
            }}
            onCloseClick={(event: any) => onScreenTabCloseClick(item)(event)}
            onCloseMouseDown={(event: any) => onScreenTabCloseMouseDown(item)(event)}
            isInitializing={isLoading}
          >
            {label}
          </TabbedViewHandle>
        )}
        content={({setDropped}) => (
          <Dropdown>
            <DropdownItem
              onClick={() => {
                setDropped(false);
                runInFlowWithHandler({
                  ctx: item,
                  action: () => onScreenTabCloseClick(item)(undefined),
                });
              }}
            >
              {T("Close", "close")}
            </DropdownItem>
            <DropdownItem
              onClick={() => {
                setDropped(false);
                runInFlowWithHandler({
                  ctx: item,
                  action: () => onScreenTabCloseAllUnchangedClick(item)(),
                });
              }}
            >
              {T("Close all unchanged", "close_all_unchanged")}
            </DropdownItem>
            <DropdownItem
              onClick={() => {
                setDropped(false);
                runInFlowWithHandler({
                  ctx: item,
                  action: () => onScreenTabCloseAllUnchangedButThisClick(item)(),
                });
              }}
            >
              {T("Close all unchanged but this", "close_all_unchanged_but_this")}
            </DropdownItem>
          </Dropdown>
        )}
      />
    );
  }
}

export function getLabel(item: IOpenedScreen | undefined) {
  if(!item){
    return "";
  }
  const text = item.tabTitle;
  const order = item.order > 0 && !item.hasDynamicTitle ? `[${item.order}]` : "";
  return [text, order].join(" ");
}
