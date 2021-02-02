import React from "react";
import { action } from "mobx";
import { getDialogStack } from "model/selectors/DialogStack/getDialogStack";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
import { T } from "utils/translation";
import S from "./processActionQueryResult.module.scss";
import { Icon } from "gui02/components/Icon/Icon";

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
    queryInfo: any[],
    title: string
  ): Generator<any, IProcessActionQueryInfoResult> {
    let canContinue = true;
    if (queryInfo.length > 0) {
      for (let queryInfoItem of queryInfo) {
        if (queryInfoItem.severity === 0) canContinue = false;
      }

      yield new Promise(
        action((resolve: (p?: any) => void) => {
          const closeDialog = getDialogStack(ctx).pushDialog(
            "",
            <ModalWindow
              title={title}
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
                <div className="list">
                  {queryInfo.map((item, idx) => (
                    <div className="listItem" key={idx}>
                      {item.severity === 1 && (
                        <div className="itemIcon warning">
                          <Icon
                            src="./icons/warning-fill.svg"
                            tooltip={T("Refresh", "refresh_tool_tip")}
                          />
                        </div>
                      )}
                      {item.severity === 0 && (
                        <div className="itemIcon error">
                          <Icon
                            src="./icons/error-fill.svg"
                            tooltip={T("Refresh", "refresh_tool_tip")}
                          />
                        </div>
                      )}

                      <div className="itemMessage">{item.message}</div>
                    </div>
                  ))}
                </div>
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
