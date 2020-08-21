import { CloseButton, ModalWindow } from "gui/Components/Dialog/Dialog";
import { observer, Observer } from "mobx-react";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { onSelectionDialogActionButtonClick } from "model/actions-ui/SelectionDialog/onSelectionDialogActionButtonClick";
import { getIsScreenOrAnyDataViewWorking } from "model/selectors/FormScreen/getIsScreenOrAnyDataViewWorking";
import { getDialogStack } from "model/selectors/getDialogStack";
import React, { useEffect } from "react";
import { IOpenedScreen } from "../../../model/entities/types/IOpenedScreen";
import { getWorkbenchLifecycle } from "../../../model/selectors/getWorkbenchLifecycle";
import S from "./ScreenArea.module.scss";
import { DialogScreenBuilder } from "./ScreenBuilder";
import { CtxPanelVisibility } from "gui02/contexts/GUIContexts";
import { onWorkflowAbortClick } from "../../../model/actions-ui/ScreenHeader/onWorkflowAbortClick";
import { onWorkflowNextClick } from "../../../model/actions-ui/ScreenHeader/onWorkflowNextClick";
import {T} from "../../../utils/translation";

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
        <button className={S.TabHandleCloseBtn} onClick={this.props.onCloseClick}>
          <i className="fas fa-times" />
        </button>
      </div>
    );
  }
}

export const DialogScreen: React.FC<{
  openedScreen: IOpenedScreen;
}> = observer((props) => {
  const key = `ScreenDialog@${props.openedScreen.menuItemId}@${props.openedScreen.order}`;
  const workbenchLifecycle = getWorkbenchLifecycle(props.openedScreen);

  function renderActionButtons() {
    const content = props.openedScreen.content;
    const isNextButton = content.formScreen && content.formScreen.showWorkflowNextButton;
    const isCancelButton = content.formScreen && content.formScreen.showWorkflowCancelButton;
    return (
      <div className={S.actionButtonHeader}>
        {isCancelButton && (
          <button
            className={S.workflowActionBtn}
            onClick={onWorkflowAbortClick(content.formScreen!)}
          >
            {T("Cancel","button_cancel")}
          </button>
        )}
        {isNextButton && (
          <button
            className={S.workflowActionBtn}
            onClick={onWorkflowNextClick(content.formScreen!)}
          >
            {T("Next","button_next")}
          </button>
        )}
      </div>
    );
  }

  useEffect(() => {
    getDialogStack(workbenchLifecycle).pushDialog(
      key,
      <Observer>
        {() => (
          <ModalWindow
            title={
              /*!props.openedScreen.content.isLoading
                ? props.openedScreen.content.formScreen!.title
                : */ props
                .openedScreen.title
            }
            titleIsWorking={
              props.openedScreen.content.isLoading ||
              getIsScreenOrAnyDataViewWorking(props.openedScreen.content.formScreen!) ||
              !!window.localStorage.getItem("debugKeepProgressIndicatorsOn")
            }
            titleButtons={
              <CloseButton onClick={(event) => onScreenTabCloseClick(props.openedScreen)(event)} />
            }
            buttonsCenter={null}
            buttonsLeft={null}
            buttonsRight={
              <Observer>
                {() =>
                  !props.openedScreen.content.isLoading ? (
                    <>
                      {props.openedScreen.content.formScreen!.dialogActions.map((action) => (
                        <button
                          key={action.id}
                          onClick={(event: any) => {
                            onSelectionDialogActionButtonClick(action)(event, action);
                          }}
                        >
                          {action.caption}
                        </button>
                      ))}
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
                    height: props.openedScreen.dialogInfo!.height,
                    display: "flex",
                    flexDirection: "column",
                  }}
                >
                  {
                    !props.openedScreen.content.isLoading ? (
                      <CtxPanelVisibility.Provider value={{ isVisible: true }}>
                        {renderActionButtons()}
                        <DialogScreenBuilder openedScreen={props.openedScreen} />
                      </CtxPanelVisibility.Provider>
                    ) : null /*<DialogLoadingContent />*/
                  }
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

export const DialogLoadingContent: React.FC = (props) => {
  return (
    <div className={S.dialogLoadingContent}>
      <i className="fas fa-cog fa-spin fa-2x" />
    </div>
  );
};
