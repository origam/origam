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

import { action, flow } from "mobx";
import { showDialog } from "model/selectors/getDialogStack";
import React from "react";
import { getOpenedScreen } from "../../selectors/getOpenedScreen";
import { ChangeMasterRecordDialog } from "../../../gui/Components/Dialogs/ChangeMasterRecordDialog";
import { getApi } from "model/selectors/getApi";
import { IDataView } from "../types/IDataView";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getSessionId } from "model/selectors/getSessionId";
import { isInfiniteScrollingActive } from "model/selectors/isInfiniteScrollingActive";
import { getSelectedRowErrorMessages } from "model/selectors/DataView/getSelectedRowErrorMessages";
import selectors from "model/selectors-tree";
import { processCRUDResult } from "../../actions/DataLoading/processCRUDResult";
import { processActionQueryInfo } from "model/actions/Actions/processActionQueryInfo";
import { runGeneratorInFlowWithHandler } from "utils/runInFlowWithHandler";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { IApi } from "model/entities/types/IApi";
import { IFormScreen } from "model/entities/types/IFormScreen";

export function questionSaveDataAfterRecordChange(ctx: any) {
  return new Promise(
    action((resolve: (value: IQuestionChangeRecordAnswer) => void) => {
      const closeDialog = showDialog(ctx,
        "",
        <ChangeMasterRecordDialog
          screenTitle={getOpenedScreen(ctx).tabTitle}
          onSaveClick={() => {
            closeDialog();
            resolve(IQuestionChangeRecordAnswer.Yes); // eslint-disable-line @typescript-eslint/no-use-before-define
          }}
          onDontSaveClick={() => {
            closeDialog();
            resolve(IQuestionChangeRecordAnswer.No); // eslint-disable-line @typescript-eslint/no-use-before-define
          }}
          onCancelClick={() => {
            closeDialog();
            resolve(IQuestionChangeRecordAnswer.Cancel); // eslint-disable-line @typescript-eslint/no-use-before-define
          }}
        />
      );
    })
  );
}

export enum IQuestionDeleteDataAnswer {
  No = 0,
  Yes = 1,
}

export async function handleUserInputOnChangingRow(dataView: IDataView) {
  const openedScreen = getOpenedScreen(dataView);
  const sessionId = getSessionId(openedScreen.content.formScreen);
  const formScreen = getFormScreen(dataView);

  if (isInfiniteScrollingActive(dataView)) {
    if (formScreen.autoSaveOnListRecordChange) {
      return await saveChanges(dataView, sessionId);
    }
    switch (await questionSaveDataAfterRecordChange(dataView)) {
      case IQuestionChangeRecordAnswer.Cancel: // eslint-disable-line @typescript-eslint/no-use-before-define
        return false;
      case IQuestionChangeRecordAnswer.Yes: // eslint-disable-line @typescript-eslint/no-use-before-define
        return await saveChanges(dataView, sessionId);
      case IQuestionChangeRecordAnswer.No: // eslint-disable-line @typescript-eslint/no-use-before-define
        await flow(() =>
          getFormScreenLifecycle(dataView).throwChangesAway(dataView)
        )();
        return true;
      default:
        throw new Error("Option not implemented");
    }
  } else {
    const errorMessages = dataView.childBindings
      .map((binding) => binding.childDataView)
      .concat(dataView)
      .flatMap((dataView) => getSelectedRowErrorMessages(dataView));
    if (errorMessages.length > 0) {
      await flow(function*bla() {
        yield*selectors.error
          .getDialogController(dataView)
          .pushError(errorMessages.join("\n"));
      })();
      return false;
    }
  }
  return true;
}

async function saveChanges(dataView: IDataView, sessionId: string) {
  const api = getApi(dataView);
  const formScreen = getFormScreen(dataView);
  return runGeneratorInFlowWithHandler({
    ctx: dataView,
    generator: function*(): Generator<any> {
      const queryResult = yield api.saveSessionQuery(sessionId);
      const processQueryInfoResult = yield*processActionQueryInfo(dataView)(
        queryResult as any[],
        formScreen.title
      );
      if (!processQueryInfoResult.canContinue) {
        return false;
      }
      const saveResult = yield api.saveSession(sessionId);
      yield*processCRUDResult(dataView, saveResult as any);
      return true;
    }()
  });
}

enum IQuestionChangeRecordAnswer {
  Yes = 0,
  No = 1,
  Cancel = 2,
}
