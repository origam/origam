import { CloseButton, ModalWindow } from "gui/Components/Dialog/Dialog";
import { inject, observer, Observer } from "mobx-react";

import { onSelectionDialogActionButtonClick } from "model/actions-ui/SelectionDialog/onSelectionDialogActionButtonClick";
import { getDialogStack } from "model/selectors/getDialogStack";
import { getOpenedDialogItems } from "model/selectors/getOpenedDialogItems";
import React, { useEffect } from "react";
import { IOpenedScreen } from "../../../model/entities/types/IOpenedScreen";
import { getOpenedScreenItems } from "../../../model/selectors/getOpenedScreenItems";
import { getWorkbenchLifecycle } from "../../../model/selectors/getWorkbenchLifecycle";
import S from "./ScreenArea.module.css";
import { DialogScreenBuilder, ScreenBuilder } from "./ScreenBuilder";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { getIsDataViewOrFormScreenWorking } from "model/selectors/DataView/getIsDataViewOrFormScreenWorking";
import { getIsScreenOrAnyDataViewWorking } from "model/selectors/FormScreen/getIsScreenOrAnyDataViewWorking";

@observer
class MainViewHandle extends React.Component<{
  order: number;
  label: string;
  isActive: boolean;
  onClick?: (event: any) => void;
  onCloseClick?: (event: any) => void;
}> {
  render() {
    return (
      <div
        className={S.TabHandle + (this.props.isActive ? ` ${S.active}` : "")}
        onClick={this.props.onClick}
      >
        {this.props.label}
        {this.props.order > 0 ? ` [${this.props.order}] ` : ""}
        <button
          className={S.TabHandleCloseBtn}
          onClick={this.props.onCloseClick}
        >
          <i className="fas fa-times" />
        </button>
      </div>
    );
  }
}

export const DialogScreen: React.FC<{
  openedScreen: IOpenedScreen;
}> = observer(props => {
  const key = `ScreenDialog@${props.openedScreen.menuItemId}@${props.openedScreen.order}`;
  const workbenchLifecycle = getWorkbenchLifecycle(props.openedScreen);
  useEffect(() => {
    getDialogStack(workbenchLifecycle).pushDialog(
      key,
      <Observer>
        {() => (
          <ModalWindow
            title={
              !props.openedScreen.content.isLoading
                ? props.openedScreen.content.formScreen!.title
                : props.openedScreen.title
            }
            titleIsWorking={
              props.openedScreen.content.isLoading ||
              getIsScreenOrAnyDataViewWorking(
                props.openedScreen.content.formScreen!
              )
            }
            titleButtons={
              <CloseButton
                onClick={event =>
                  onScreenTabCloseClick(props.openedScreen)(event)
                }
              />
            }
            buttonsCenter={null}
            buttonsLeft={null}
            buttonsRight={
              <Observer>
                {() =>
                  !props.openedScreen.content.isLoading ? (
                    <>
                      {props.openedScreen.content.formScreen!.dialogActions.map(
                        action => (
                          <button
                            key={action.id}
                            onClick={(event: any) => {
                              onSelectionDialogActionButtonClick(action)(
                                event,
                                action
                              );
                            }}
                          >
                            {action.caption}
                          </button>
                        )
                      )}
                    </>
                  ) : (
                    <></>
                  )
                }
              </Observer>
            }
          >
            <Observer>
              {() => (
                <div
                  style={{
                    width: props.openedScreen.dialogInfo!.width,
                    height: props.openedScreen.dialogInfo!.height
                  }}
                >
                  {!props.openedScreen.content.isLoading ? (
                    <DialogScreenBuilder openedScreen={props.openedScreen} />
                  ) : (
                    null /*<DialogLoadingContent />*/
                  )}
                </div>
              )}
            </Observer>
          </ModalWindow>
        )}
      </Observer>
    );
    return () => getDialogStack(workbenchLifecycle).closeDialog(key);
  }, []);
  return null;
});

export const DialogLoadingContent: React.FC = props => {
  return (
    <div className={S.dialogLoadingContent}>
      <i className="fas fa-cog fa-spin fa-2x" />
    </div>
  );
};
