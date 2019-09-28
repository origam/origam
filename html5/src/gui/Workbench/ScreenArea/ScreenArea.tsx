import { CloseButton, ModalWindow } from "gui/Components/Dialog/Dialog";
import { inject, observer, Observer } from "mobx-react";
import { onFormTabCloseClick } from "model/actions/onFormTabCloseClick";
import { onSelectionDialogActionButtonClick } from "model/actions/SelectionDialog/onSelectionDialogActionButtonClick";
import { getDialogStack } from "model/selectors/getDialogStack";
import { getOpenedDialogItems } from "model/selectors/getOpenedDialogItems";
import React, { useEffect } from "react";
import { IOpenedScreen } from "../../../model/entities/types/IOpenedScreen";
import { getOpenedScreenItems } from "../../../model/selectors/getOpenedScreenItems";
import { getWorkbenchLifecycle } from "../../../model/selectors/getWorkbenchLifecycle";
import S from "./ScreenArea.module.css";
import { DialogScreenBuilder, ScreenBuilder } from "./ScreenBuilder";

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

@inject(({ workbench }) => {
  return {
    openedScreenItems: getOpenedScreenItems(workbench),
    openedDialogItems: getOpenedDialogItems(workbench),
    onScreenTabHandleClick: getWorkbenchLifecycle(workbench)
      .onScreenTabHandleClick,
    onScreenTabCloseClick: (event: any, item: IOpenedScreen) =>
      onFormTabCloseClick(item)(event)
  };
})
@observer
export class ScreenArea extends React.Component<{
  openedScreenItems?: IOpenedScreen[];
  openedDialogItems?: IOpenedScreen[];
  onScreenTabHandleClick?: (event: any, openedScreen: IOpenedScreen) => void;
  onScreenTabCloseClick?: (event: any, openedScreen: IOpenedScreen) => void;
}> {
  render() {
    return (
      <div className={S.Root}>
        {/*<ColumnsDialog />*/}
        {/*<ModalWindowOverlay>
          <ModalWindow
            title="Columns"
            buttonsCenter={
              <>
                <button>OK</button>
                <button>Save As...</button>
                <button>Cancel</button>
              </>
            }
            buttonsLeft={null}
            buttonsRight={null}
          >
            <div className
          </ModalWindow>
          </ModalWindowOverlay>*/}

        <div className={S.TabHandles}>
          <Observer>
            {() => (
              <>
                {this.props.openedScreenItems!.map(item => (
                  <MainViewHandle
                    key={`${item.menuItemId}@${item.order}`}
                    label={
                      !item.content.isLoading
                        ? item.content.formScreen!.title
                        : item.title
                    }
                    order={item.order}
                    isActive={item.isActive}
                    onClick={(event: any) =>
                      this.props.onScreenTabHandleClick!(event, item)
                    }
                    onCloseClick={(event: any) =>
                      this.props.onScreenTabCloseClick!(event, item)
                    }
                  />
                ))}
              </>
            )}
          </Observer>
        </div>
        <Observer>
          {() => (
            <>
              {this.props.openedScreenItems!.map(item => (
                <ScreenBuilder
                  key={`${item.menuItemId}@${item.order}`}
                  openedScreen={item}
                />
              ))}
            </>
          )}
        </Observer>
        <Observer>
          {() => {
            console.log(this.props.openedDialogItems);
            return (
              <>
                {this.props.openedDialogItems!.map(item => (
                  <DialogScreen
                    openedScreen={item}
                    key={`${item.menuItemId}@${item.order}`}
                  />
                ))}
              </>
            );
          }}
        </Observer>
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
            titleButtons={
              <CloseButton
                onClick={event =>
                  workbenchLifecycle.onScreenTabCloseClick(
                    event,
                    props.openedScreen
                  )
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
                    <DialogLoadingContent />
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
