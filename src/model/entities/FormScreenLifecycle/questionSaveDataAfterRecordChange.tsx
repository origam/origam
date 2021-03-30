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
import {processCRUDResult} from "../../actions/DataLoading/processCRUDResult";

export function questionSaveDataAfterRecordChange(ctx: any) {
  return new Promise(
    action((resolve: (value: IQuestionChangeRecordAnswer) => void) => {
      const closeDialog = getDialogStack(ctx).pushDialog(
        "",
        <ChangeMasterRecordDialog
          screenTitle={getOpenedScreen(ctx).title}
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
  const api = getApi(dataView);
  const openedScreen = getOpenedScreen(dataView);
  const sessionId = getSessionId(openedScreen.content.formScreen);

  if (isInfiniteScrollingActive(dataView)) {
    switch (await questionSaveDataAfterRecordChange(dataView)) {
      case IQuestionChangeRecordAnswer.Cancel: // eslint-disable-line @typescript-eslint/no-use-before-define
        return false;
      case IQuestionChangeRecordAnswer.Yes: // eslint-disable-line @typescript-eslint/no-use-before-define
        await api.saveSessionQuery(sessionId);
        const result = await api.saveSession(sessionId);
        await flow(() => processCRUDResult(dataView, result));
        return true;
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
