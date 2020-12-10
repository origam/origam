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
              title={T("Action Error", "action_error")}
              titleButtons={null}
              buttonsCenter={
                <>
                  {!canContinue && (
                    <button
                      tabIndex={0}
                      onClick={() => {
                        canContinue = false;
                        closeDialog();
                        resolve();
                      }}
                    >
                      {T("Ok", "button_ok")}
                    </button>
                  )}
                  {canContinue && (
                    <>
                      <button
                        tabIndex={0}
                        autoFocus={true}
                        onClick={() => {
                          canContinue = true;
                          closeDialog();
                          resolve();
                        }}
                      >
                        {T("Yes", "button_yes")}
                      </button>
                      <button
                        tabIndex={0}
                        onClick={() => {
                          canContinue = false;
                          closeDialog();
                          resolve();
                        }}
                      >
                        {T("No", "button_no")}
                      </button>
                    </>
                  )}
                </>
              }
              buttonsLeft={null}
              buttonsRight={null}
            >
              <div className={S.dialogContent}>
                <ul>
                  {queryInfo.map((item, idx) => (
                    <li key={idx}>{item.message}</li>
                  ))}
                </ul>
                {canContinue && (
                  <>
                    {T(
                      "Do you wish to continue anyway?",
                      "do_you_wish_to_continue"
                    )}
                  </>
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
