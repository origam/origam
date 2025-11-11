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
import { MobXProviderContext, observer } from "mobx-react";
import {
  onScreenTabCloseClick,
  onScreenTabCloseMouseDown
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

  render() {
    const {item} = this.props;
    const content = getOpenedScreen(item).content;
    const isEagerLoading =  !isLazyLoading(item)

    const isLoading= isEagerLoading && (content.isLoading || getIsScreenOrAnyDataViewWorking(content.formScreen!))
    const label = getLabel(item);
    return (
      <TabbedViewHandle
        title={label}
        key={`${item.menuItemId}@${item.order}`}
        isActive={item.isActive}
        hasCloseBtn={true}
        isDirty={getIsFormScreenDirty(item)}
        onClick={(event: any) => onScreenTabHandleClick(item)(event)}
        onCloseClick={(event: any) => onScreenTabCloseClick(item)(event)}
        onCloseMouseDown={(event: any) => onScreenTabCloseMouseDown(item)(event)}
        isInitializing={isLoading}
      >
        {label}
      </TabbedViewHandle>
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
