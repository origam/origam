import React from "react";
import { action } from "mobx";
import { getDialogStack } from "model/selectors/DialogStack/getDialogStack";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
import { T } from "utils/translation";
import S from "./processActionQueryResult.module.scss";

export interface IQueryInfo {
  entityName: string;
  fieldName: string;
  message: string;
  severity: number;
}

export interface IProcessActionQueryInfoResult {
  canContinue: boolean;
}

export function processActionQueryInfo(ctx: any) {
  return function* processActionQueryInfo(
    queryInfo: any[]
  ): Generator<any, IProcessActionQueryInfoResult> {
    let canContinue = true;
    if (queryInfo.length > 0) {
      for (let queryInfoItem of queryInfo) {
        if (queryInfoItem.severity === 0) canContinue = false;
      }

      yield new Promise(
        action((resolve: () => void) => {
          const closeDialog = getDialogStack(ctx).pushDialog(
            "",
            <ModalWindow
              title={T("Action Error", "question_title")}
              titleButtons={null}
              buttonsCenter={
                <>
                  <button
                    tabIndex={0}
                    onClick={() => {
                      canContinue = false;
                      closeDialog();
                      resolve();
                    }}
                  >
                    {T("Cancel", "button_cancel")}
                  </button>
                  {canContinue && (
                    <button
                      tabIndex={0}
                      autoFocus={true}
                      onClick={() => {
                        canContinue = true;
                        closeDialog();
                        resolve();
                      }}
                    >
                      {T("Continue", "button_continue")}
                    </button>
                  )}
                </>
              }
              buttonsLeft={null}
              buttonsRight={null}
            >
              <div className={S.dialogContent}>
                {T("There has been a problem:", "there_has_been_a_problem")}
                <ul>
                  {queryInfo.map((item, idx) => (
                    <li key={idx}>{item.message}</li>
                  ))}
                </ul>
                {canContinue && (
                  <>{T("Do you wish to continue anyway?", "do_you_wish_to_continue")}</>
                )}
              </div>
            </ModalWindow>
          );
        })
      );
    }
    return { canContinue };
  };
}
