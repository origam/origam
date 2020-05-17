import React from "react";
import { action } from "mobx";
import { getDialogStack } from "model/selectors/DialogStack/getDialogStack";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
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
              title="Workflow error"
              titleButtons={null}
              buttonsCenter={
                <>
                  <button
                    onClick={() => {
                      canContinue = false;
                      closeDialog();
                      resolve();
                    }}
                  >
                    Cancel
                  </button>
                  {canContinue && (
                    <button
                      onClick={() => {
                        canContinue = true;
                        closeDialog();
                        resolve();
                      }}
                    >
                      Continue
                    </button>
                  )}
                </>
              }
              buttonsLeft={null}
              buttonsRight={null}
            >
              <div className={S.dialogContent}>
                There has been a problem:
                <ul>
                  {queryInfo.map((item, idx) => (
                    <li key={idx}>{item.message}</li>
                  ))}
                </ul>
                {canContinue && <>Do you wish to continue anyway?</>}
              </div>
            </ModalWindow>
          );
        })
      );
    }
    return { canContinue };
  };
}

function questionContinueWorkflow() {
  return;
}
