import { action, flow } from "mobx";
import { getDialogStack } from "model/selectors/getDialogStack";
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

export function questionSaveDataAfterRecordChange(ctx: any) {
  return new Promise(
    action((resolve: (value: IQuestionChangeRecordAnswer) => void) => {
      const closeDialog = getDialogStack(ctx).pushDialog(
        "",
        <ChangeMasterRecordDialog
          screenTitle={getOpenedScreen(ctx).tabTitle}
          onSaveClick={() => {
            closeDialog();
            resolve(IQuestionChangeRecordAnswer.Yes);
          }}
          onDontSaveClick={() => {
            closeDialog();
            resolve(IQuestionChangeRecordAnswer.No);
          }}
          onCancelClick={() => {
            closeDialog();
            resolve(IQuestionChangeRecordAnswer.Cancel);
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
  const api = getApi(dataView);
  const openedScreen = getOpenedScreen(dataView);
  const sessionId = getSessionId(openedScreen.content.formScreen);
  const formScreen = getFormScreen(dataView);

  if (isInfiniteScrollingActive(dataView)) {
    switch (await questionSaveDataAfterRecordChange(dataView)) {
      case IQuestionChangeRecordAnswer.Cancel:
        return false;
      case IQuestionChangeRecordAnswer.Yes: // eslint-disable-line @typescript-eslint/no-use-before-define
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
      await flow(function* bla() {
        yield* selectors.error
          .getDialogController(dataView)
          .pushError(errorMessages.join("\n"));
      })();
      return false;
    }
  }
  return true;
}

enum IQuestionChangeRecordAnswer {
  Yes = 0,
  No = 1,
  Cancel = 2,
}
