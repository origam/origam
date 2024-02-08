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

import { flow } from "mobx";
import { closeForm } from "model/actions/closeForm";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";
import { handleError } from "model/actions/handleError";
import { closingScreens } from "model/entities/FormScreenLifecycle/FormScreenLifecycle";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { IQuestionDeleteDataAnswer } from "model/entities/FormScreenLifecycle/questionSaveDataAfterRecordChange";
import { askYesNoQuestion } from "gui/Components/Dialog/DialogUtils";
import { T } from "utils/translation";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";

export function onScreenTabCloseMouseDown(ctx: any) {
  return function (event: any) {
    // OMG, how ugly is this...
    const openedScreen = getOpenedScreen(ctx);
    if (openedScreen) {
      openedScreen.isBeingClosed = true;
    }
  };
}


export function onScreenTabCloseClick(ctx: any) {
  return flow(function*onFormTabCloseClick(event: any, closeWithoutSaving?: boolean) {
    const openedScreen = getOpenedScreen(ctx);
    const formScreen = openedScreen.content?.formScreen;
    if(formScreen){
      const lifecycle = getFormScreenLifecycle(formScreen);
      if(formScreen?.showWorkflowNextButton) {
        return yield*lifecycle.onWorkflowAbortClick(null);
      }
    }
    let dataViews = openedScreen.content?.formScreen?.dataViews ?? [];
    for (const dataView of dataViews) {
      getTablePanelView(dataView)?.setEditing(false);
    }
    try {
      event?.stopPropagation?.();
      // TODO: Wait for other async operation to finish?
      if (closingScreens.has(openedScreen)) return;
      closingScreens.add(openedScreen);
      // TODO: Better lifecycle handling
      if (openedScreen.content && !openedScreen.content.isLoading) {
        const lifecycle = getFormScreenLifecycle(openedScreen.content.formScreen!);
        yield*lifecycle.onRequestScreenClose(closeWithoutSaving);
      } else {
        yield*closeForm(ctx)();
        return true;
      }
    } catch (e) {
      yield*handleError(ctx)(e);
      throw e;
    } finally {
      closingScreens.delete(openedScreen);
    }
  });
}
