import bind from "bind-decorator";
import { reloadScreen } from "model/actions/FormScreen/reloadScreen";
import { handleError } from "model/actions/handleError";
import { createFormScreenEnvelope } from "model/factories/createFormScreenEnvelope";
import { createOpenedScreen } from "model/factories/createOpenedScreen";
import { getIsFormScreenDirty } from "model/selectors/FormScreen/getisFormScreenDirty";
import { getApi } from "model/selectors/getApi";
import { getClientFulltextSearch } from "model/selectors/getClientFulltextSearch";
import { getOpenedScreens } from "model/selectors/getOpenedScreens";
import { getMainMenuEnvelope } from "model/selectors/MainMenu/getMainMenuEnvelope";
import { getMainMenuItemById } from "model/selectors/MainMenu/getMainMenuItemById";
import { getWorkQueues } from "model/selectors/WorkQueues/getWorkQueues";
import { findMenu } from "xmlInterpreters/menuXml";
import { MainMenuContent } from "../MainMenu";
import { DialogInfo } from "../OpenedScreen";
import { IMainMenuItemType } from "../types/IMainMenu";
import { IDialogInfo, IOpenedScreen } from "../types/IOpenedScreen";
import { IWorkbenchLifecycle } from "../types/IWorkbenchLifecycle";
import { WebScreen } from "../WebScreen";
import { getSessionId } from "model/selectors/getSessionId";
import { scopeFor } from "dic/Container";
import { assignIIds } from "xmlInterpreters/xmlUtils";

export enum IRefreshOnReturnType {
  None = "None",
  ReloadActualRecord = "ReloadActualRecord",
  RefreshCompleteForm = "RefreshCompleteForm",
  MergeModalDialogChanges = "MergeModalDialogChanges",
}

export class WorkbenchLifecycle implements IWorkbenchLifecycle {
  $type_IWorkbenchLifecycle: 1 = 1;

  *onMainMenuItemClick(args: { event: any; item: any }): Generator {
    const { type, id, label, dialogWidth, dialogHeight, dontRequestData } = args.item.attributes;
    const { event } = args;

    const openedScreens = getOpenedScreens(this);

    let dialogInfo: IDialogInfo | undefined;
    if (type === IMainMenuItemType.FormRefWithSelection) {
      dialogInfo = new DialogInfo(parseInt(dialogWidth, 10), parseInt(dialogHeight, 10));
    }
    if (event && !event.ctrlKey) {
      const existingItem = openedScreens.findLastExistingItem(id);
      if (existingItem) {
        openedScreens.activateItem(id, existingItem.order);
        const openedScreen = existingItem;
        if (openedScreen.isSleeping) {
          openedScreen.isSleeping = false;
          const initUIResult = yield* this.initUIForScreen(openedScreen, false);
          yield* openedScreen.content!.start(initUIResult, openedScreen.isSleepingDirty);
        } else if (
          openedScreen.content &&
          openedScreen.content.formScreen &&
          openedScreen.content.formScreen.refreshOnFocus &&
          !openedScreen.content.isLoading
        ) {
          if (!getIsFormScreenDirty(openedScreen.content.formScreen)) {
            yield* reloadScreen(openedScreen.content.formScreen)();
          }
        }
      } else {
        yield* this.openNewForm(id, type, label, dontRequestData === "true", dialogInfo, {});
      }
    } else {
      yield* this.openNewForm(id, type, label, dontRequestData === "true", dialogInfo, {});
    }
  }

  *onWorkQueueListItemClick(event: any, item: any) {
    const openedScreens = getOpenedScreens(this);

    const id = item.id;
    const type = IMainMenuItemType.WorkQueue;
    const label = item.name;

    let dialogInfo: IDialogInfo | undefined;
    if (!event.ctrlKey) {
      const existingItem = openedScreens.findLastExistingItem(id);
      if (existingItem) {
        openedScreens.activateItem(id, existingItem.order);
        const openedScreen = existingItem;
        if (openedScreen.isSleeping) {
          openedScreen.isSleeping = false;
          const initUIResult = yield* this.initUIForScreen(openedScreen, false);
          yield* openedScreen.content!.start(initUIResult, openedScreen.isSleepingDirty);
        } else if (
          openedScreen.content &&
          openedScreen.content.formScreen &&
          openedScreen.content.formScreen.refreshOnFocus &&
          !openedScreen.content.isLoading
        ) {
          if (!getIsFormScreenDirty(openedScreen.content.formScreen)) {
            yield* reloadScreen(openedScreen.content.formScreen)();
          }
        }
      } else {
        yield* this.openNewForm(id, type, label, false, dialogInfo, {});
      }
    } else {
      yield* this.openNewForm(id, type, label, false, dialogInfo, {});
    }
  }

  *onScreenTabHandleClick(event: any, openedScreen: IOpenedScreen): Generator {
    const openedScreens = getOpenedScreens(this);
    openedScreens.activateItem(openedScreen.menuItemId, openedScreen.order);

    if (openedScreen.isSleeping) {
      openedScreen.isSleeping = false;
      const initUIResult = yield* this.initUIForScreen(openedScreen, false);
      yield* openedScreen.content!.start(initUIResult, openedScreen.isSleepingDirty);
    } else if (
      openedScreen.content &&
      openedScreen.content.formScreen &&
      openedScreen.content.formScreen.refreshOnFocus &&
      !openedScreen.content.isLoading
    ) {
      if (!getIsFormScreenDirty(openedScreen.content.formScreen)) {
        yield* reloadScreen(openedScreen.content.formScreen)();
      }
    }
  }

  *closeForm(openedScreen: IOpenedScreen): Generator {
    // TODO: Refactor to get rid of code duplication
    const openedScreens = getOpenedScreens(openedScreen);
    const closestScreen = openedScreens.findClosestItem(
      openedScreen.menuItemId,
      openedScreen.order
    );
    openedScreens.deleteItem(openedScreen.menuItemId, openedScreen.order);
    if (openedScreen.dialogInfo) {
      if (openedScreen.isActive) {
        if (closestScreen) {
          openedScreens.activateItem(closestScreen.menuItemId, closestScreen.order);

          if (closestScreen.isSleeping) {
            closestScreen.isSleeping = false;
            const initUIResult = yield* this.initUIForScreen(closestScreen, false);
            yield* closestScreen.content!.start(initUIResult, closestScreen.isSleepingDirty);
          } else if (
            closestScreen.content &&
            closestScreen.content.formScreen &&
            (closestScreen.content.formScreen.refreshOnFocus ||
              openedScreen.content.refreshOnReturnType ===
                IRefreshOnReturnType.RefreshCompleteForm) &&
            !closestScreen.content.isLoading
          ) {
            if (!getIsFormScreenDirty(closestScreen.content.formScreen)) {
              yield* reloadScreen(closestScreen.content.formScreen)();
            }
          }
        }
      }
    } else {
      if (openedScreen.isActive) {
        if (closestScreen) {
          openedScreens.activateItem(closestScreen.menuItemId, closestScreen.order);

          if (closestScreen.isSleeping) {
            closestScreen.isSleeping = false;
            const initUIResult = yield* this.initUIForScreen(closestScreen, false);
            yield* closestScreen.content!.start(initUIResult, closestScreen.isSleepingDirty);
          } else if (
            closestScreen.content &&
            closestScreen.content.formScreen &&
            closestScreen.content.formScreen.refreshOnFocus &&
            !closestScreen.content.isLoading
          ) {
            if (!getIsFormScreenDirty(closestScreen.content.formScreen)) {
              yield* reloadScreen(closestScreen.content.formScreen)();
            }
          }
        }
      }
    }

    yield* this.destroyUI(openedScreen);

    if (openedScreen.content && openedScreen.content.formScreen) {
      const scope = scopeFor(openedScreen.content.formScreen);
      if (scope) scope.disposeWithChildren();
    }
  }

  *destroyUI(openedScreen: IOpenedScreen) {
    const api = getApi(this);
    if (openedScreen.content) {
      if (openedScreen.content.formScreen) {
        yield api.destroyUI({ FormSessionId: getSessionId(openedScreen.content.formScreen) });
      } else if (openedScreen.content.preloadedSessionId) {
        yield api.destroyUI({ FormSessionId: openedScreen.content.preloadedSessionId });
      }
    }
  }

  @bind
  *openNewForm(
    id: string,
    type: IMainMenuItemType,
    label: string,
    dontRequestData: boolean,
    dialogInfo: IDialogInfo | undefined,
    parameters: { [key: string]: any },
    formSessionId?: string,
    isSessionRebirth?: boolean,
    isSleepingDirty?: boolean,
    refreshOnReturnType?: IRefreshOnReturnType
  ) {
    const openedScreens = getOpenedScreens(this);
    const existingItem = openedScreens.findLastExistingItem(id);
    const newFormScreen = createFormScreenEnvelope(formSessionId, refreshOnReturnType);
    const newScreen = createOpenedScreen(
      id,
      type,
      existingItem ? existingItem.order + 1 : 0,
      label,
      newFormScreen,
      dontRequestData,
      dialogInfo,
      parameters,
      isSessionRebirth,
      isSleepingDirty
    );
    try {
      openedScreens.pushItem(newScreen);
      if (!isSessionRebirth) {
        openedScreens.activateItem(newScreen.menuItemId, newScreen.order);
      }

      if (isSessionRebirth) {
        return;
      }

      const initUIResult = yield* this.initUIForScreen(newScreen, !isSessionRebirth);

      yield* newFormScreen.start(initUIResult);
    } catch (e) {
      yield* handleError(this)(e);
      yield* this.closeForm(newScreen);
      throw e;
    }
  }

  *initUIForScreen(screen: IOpenedScreen, isNewSession: boolean) {
    const api = getApi(this);
    const initUIResult = yield api.initUI({
      Type: screen.menuItemType,
      Caption: screen.title,
      ObjectId: screen.menuItemId,
      FormSessionId: screen.content!.preloadedSessionId,
      IsNewSession: isNewSession,
      RegisterSession: true, //!!registerSession,
      DataRequested: !screen.dontRequestData,
      Parameters: screen.parameters,
    });
    console.log(initUIResult);
    return initUIResult;
  }

  *openNewUrl(url: string, title: string) {
    console.log("open new ", url);
    const openedScreens = getOpenedScreens(this);
    const newScreen = new WebScreen(title, url, url, 0);
    openedScreens.pushItem(newScreen);
    openedScreens.activateItem(newScreen.menuItemId, newScreen.order);
  }

  *initPortal() {
    const api = getApi(this);
    const portalInfo = yield api.initPortal();

    console.log("portalInfo:");
    console.log(portalInfo);
    const menuUI = findMenu(portalInfo.menu);
    assignIIds(menuUI);
    getMainMenuEnvelope(this).setMainMenu(new MainMenuContent({ menuUI }));
    getClientFulltextSearch(this).indexMainMenu(menuUI);

    for (let session of portalInfo.sessions) {
      const menuItem = getMainMenuItemById(this, session.objectId);
      if (menuItem) {
        yield* this.openNewForm(
          session.objectId,
          session.type,
          menuItem.attributes.label, // TODO: Find in menu
          menuItem.attributes.dontRequestData === "true", // TODO: Find in menu
          undefined, // TODO: Find in... menu?
          {},
          session.formSessionId,
          true,
          session.isDirty
        );
      } else {
        console.log("No menu item for menuId", session.objectId);
      }
    }

    const openedScreens = getOpenedScreens(this);
    if (openedScreens.items.length > 0) {
      openedScreens.activateItem(openedScreens.items[0].menuItemId, openedScreens.items[0].order);
      openedScreens.items[0].isSleeping = false;
      const initUIResult = yield* this.initUIForScreen(openedScreens.items[0], false);
      yield* openedScreens.items[0].content!.start(
        initUIResult,
        openedScreens.items[0].isSleepingDirty
      );
    }

    const workQueues = getWorkQueues(this);
    yield* workQueues.setRefreshInterval(portalInfo.workQueueListRefreshInterval);
    yield* workQueues.startTimer();
  }

  *run(): Generator {
    yield* this.initPortal();
  }

  parent?: any;
}
