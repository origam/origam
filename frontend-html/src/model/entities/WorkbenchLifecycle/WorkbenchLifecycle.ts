/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import bind from "bind-decorator";
import { reloadScreen } from "model/actions/FormScreen/reloadScreen";
import { handleError } from "model/actions/handleError";
import { createFormScreenEnvelope } from "model/factories/createFormScreenEnvelope";
import { getIsFormScreenDirty } from "model/selectors/FormScreen/getisFormScreenDirty";
import { getApi } from "model/selectors/getApi";
import { getSearcher } from "model/selectors/getSearcher";
import { getOpenedScreens } from "model/selectors/getOpenedScreens";
import { getMainMenuEnvelope } from "model/selectors/MainMenu/getMainMenuEnvelope";
import { getMainMenuItemById } from "model/selectors/MainMenu/getMainMenuItemById";
import { getWorkQueues } from "model/selectors/WorkQueues/getWorkQueues";
import { findMenu } from "xmlInterpreters/menuXml";
import { MainMenuContent } from "../MainMenu";
import { DialogInfo, OpenedScreen } from "../OpenedScreen";
import { IMainMenuItemType } from "../types/IMainMenu";
import { IDialogInfo, IOpenedScreen } from "../types/IOpenedScreen";
import { IWorkbenchLifecycle } from "../types/IWorkbenchLifecycle";
import { WebScreen } from "../WebScreen";
import { getSessionId } from "model/selectors/getSessionId";
import { scopeFor } from "dic/Container";
import { assignIIds } from "xmlInterpreters/xmlUtils";
import { DEBUG_CLOSE_ALL_FORMS } from "utils/debugHelpers";
import { getOpenedScreen } from "../../selectors/getOpenedScreen";
import { onWorkflowNextClick } from "model/actions-ui/ScreenHeader/onWorkflowNextClick";
import { observable } from "mobx";
import { IUserInfo } from "model/entities/types/IUserInfo";
import { getChatrooms } from "model/selectors/Chatrooms/getChatrooms";
import { openNewUrl } from "model/actions/Workbench/openNewUrl";
import { IUrlOpenMethod } from "../types/IUrlOpenMethod";
import { IPortalSettings } from "../types/IPortalSettings";
import { getNotifications } from "model/selectors/Chatrooms/getNotifications";
import selectors from "model/selectors-tree";
import { onMainMenuItemClick } from "model/actions-ui/MainMenu/onMainMenuItemClick";
import { getFavorites } from "model/selectors/MainMenu/getFavorites";
import { produce } from "immer";
import { IDataView } from "../types/IDataView";
import { FormScreenEnvelope } from "model/entities/FormScreen";
import { hexToRgb } from "utils/colorUtils";
import { KeyBuffer } from "model/entities/WorkbenchLifecycle/KeyBuffer";
import { EventHandler } from "utils/EventHandler";
import { getWorkbench } from "model/selectors/getWorkbench";
import { ClosedSessionTracker } from "./ClosedSessionTracker";

export enum IRefreshOnReturnType {
  None = "None",
  ReloadActualRecord = "ReloadActualRecord",
  RefreshCompleteForm = "RefreshCompleteForm",
  MergeModalDialogChanges = "MergeModalDialogChanges",
}

export class WorkbenchLifecycle implements IWorkbenchLifecycle {
  $type_IWorkbenchLifecycle: 1 = 1;

  @observable
  portalSettings: IPortalSettings | undefined;
  @observable
  userInfo: IUserInfo | undefined;
  @observable
  logoUrl: string | undefined;
  @observable
  customAssetsRoute: string | undefined;

  closedSessionTracker = new ClosedSessionTracker();

  keyBuffer =  new KeyBuffer(this);
  mainMenuItemClickHandler = new EventHandler();

  *onMainMenuItemClick(args: {
    event: any;
    item: any;
    idParameter: string | undefined;
    isSingleRecordEdit?: boolean;
    forceOpenNew?: boolean;
  }): Generator {
    const {
      type,
      id,
      label,
      dialogWidth,
      dialogHeight,
      lazyLoading,
      urlOpenMethod,
    } = args.item.attributes;
    const {event} = args;
    const alwaysOpenNew = args.item.attributes.alwaysOpenNew === "true" || args.forceOpenNew;
    this.mainMenuItemClickHandler.call();

    if (urlOpenMethod === "LaunchBrowserWindow") {
      const url = (yield this.getReportTabUrl(id)) as string;
      window.open(url);
      return;
    }

    const openedScreens = getOpenedScreens(this);

    let dialogInfo: IDialogInfo | undefined;
    if (type === IMainMenuItemType.FormRefWithSelection || type === IMainMenuItemType.ReportRefWithSelection) {
      dialogInfo = new DialogInfo(parseInt(dialogWidth, 10), parseInt(dialogHeight, 10));
    }
    if (event && !(event.ctrlKey || event.metaKey)) {
      const existingItem = openedScreens.findLastExistingTabItem(id);
      if (
        existingItem &&
        type !== IMainMenuItemType.FormRefWithSelection &&
        type !== IMainMenuItemType.ReportReferenceMenuItem &&
        type !== IMainMenuItemType.ReportRefWithSelection &&
        !alwaysOpenNew
      ) {
        openedScreens.activateItem(id, existingItem.order);
        const openedScreen = existingItem;
        if (openedScreen.isSleeping) {
          openedScreen.isSleeping = false;
          const initUIResult = yield*this.initUIForScreen(
            openedScreen,
            false,
            undefined,
            args.isSingleRecordEdit
          );
          yield*openedScreen.content!.start({
            initUIResult: initUIResult,
            preloadIsDirty: openedScreen.isSleepingDirty
          });
        } else if (
          openedScreen.content &&
          openedScreen.content.formScreen &&
          openedScreen.content.formScreen.refreshOnFocus &&
          !openedScreen.content.isLoading
        ) {
          if (!getIsFormScreenDirty(openedScreen.content.formScreen)) {
            yield*reloadScreen(openedScreen.content.formScreen)();
          }
        }
      } else {
        if (type === IMainMenuItemType.ReportReferenceMenuItem) {
          const url = (yield this.getReportTabUrl(id)) as string;
          yield*this.openNewUrl(url, args.item.attributes["label"]);
          return;
        } else {
          yield*this.openNewForm({
            id: id,
            type: type,
            label: label,
            isLazyLoading: lazyLoading === "true",
            dialogInfo: dialogInfo,
            parameters: args.idParameter ? {id: args.idParameter} : {},
            newRecordInitialValues: args.isSingleRecordEdit? {} : undefined
          });
        }
      }
    }
    else if (type === IMainMenuItemType.ReportReferenceMenuItem) {
      const url = (yield this.getReportTabUrl(id)) as string;
      yield*this.openNewUrl(url, args.item.attributes["label"]);
      return;
    } else {
      yield*this.openNewForm({
        id: id,
        type: type,
        label: label,
        isLazyLoading: lazyLoading === "true",
        dialogInfo: dialogInfo,
        parameters: args.idParameter ? {id: args.idParameter} : {},
        isSingleRecordEdit: args.isSingleRecordEdit
      });
    }
  }

  *onMainMenuItemIdClick(args: {
    event: any;
    itemId: any;
    idParameter: string | undefined;
    isSingleRecordEdit?: boolean;
  }) {
    let menuItem = args.itemId && selectors.mainMenu.getItemById(this, args.itemId);
    if (args.isSingleRecordEdit) {
      // Temporary hack o allow filtered screens to work unless single record edit is
      // implemented for paginated screens on server side. There is no need to paginate
      // when we have just one record, hence it is ok to execute the screen in without
      // pagination
      menuItem = {...menuItem};
      delete menuItem.parent;
      delete menuItem.elements;
      menuItem = produce(menuItem, (draft: any) => {
        draft.attributes.lazyLoading = "false";
      });
    }
    if (menuItem) {
      yield onMainMenuItemClick(this)({
        event: undefined,
        item: menuItem,
        idParameter: args.idParameter,
        isSingleRecordEdit: args.isSingleRecordEdit,
      });
    }
  }

  async getReportTabUrl(menuId: string) {
    const api = getApi(this);
    const url = await api.getReportFromMenu({menuId: menuId});
    return url;
  }

  *onWorkQueueListItemClick(event: any, item: any) {
    const openedScreens = getOpenedScreens(this);

    this.mainMenuItemClickHandler.call();
    const id = item.id;
    const type = IMainMenuItemType.WorkQueue;
    const label = item.name;

    let dialogInfo: IDialogInfo | undefined;
    if (!event || !(event.ctrlKey || event.metaKey)) {
      const existingItem = openedScreens.findLastExistingTabItem(id);
      if (existingItem) {
        openedScreens.activateItem(id, existingItem.order);
        const openedScreen = existingItem;
        if (openedScreen.isSleeping) {
          openedScreen.isSleeping = false;
          const initUIResult = yield*this.initUIForScreen(openedScreen, false, undefined);
          yield*openedScreen.content!.start({
            initUIResult: initUIResult,
            preloadIsDirty: openedScreen.isSleepingDirty
          });
        } else if (
          openedScreen.content &&
          openedScreen.content.formScreen &&
          openedScreen.content.formScreen.refreshOnFocus &&
          !openedScreen.content.isLoading
        ) {
          if (!getIsFormScreenDirty(openedScreen.content.formScreen)) {
            yield*reloadScreen(openedScreen.content.formScreen)();
          }
        }
      } else {
        yield*this.openNewForm({
          id: id,
          type: type,
          label: label,
          isLazyLoading: true,
          dialogInfo: dialogInfo,
          parameters: { }
      });
      }
    } else {
      yield*this.openNewForm({
        id: id,
        type: type,
        label: label,
        isLazyLoading: true,
        dialogInfo: dialogInfo,
        parameters: { }
      });
    }
  }

  *onChatroomsListItemClick(event: any, item: any) {

    const openedScreens = getOpenedScreens(this);
    const url = `/chatrooms/index.html#/chatroom?chatroomId=${item.id}`;
    const id = url;

    const existingItem = openedScreens.findLastExistingTabItem(id);
    if (existingItem) {
      openedScreens.activateItem(id, existingItem.order);
      const openedScreen = existingItem;
      if (openedScreen.isSleeping) {
        openedScreen.isSleeping = false;
        const initUIResult = yield*this.initUIForScreen(openedScreen, false);
        yield*openedScreen.content!.start({
          initUIResult: initUIResult,
          preloadIsDirty: openedScreen.isSleepingDirty
        });
      } else if (
        openedScreen.content &&
        openedScreen.content.formScreen &&
        openedScreen.content.formScreen.refreshOnFocus &&
        !openedScreen.content.isLoading
      ) {
        if (!getIsFormScreenDirty(openedScreen.content.formScreen)) {
          yield*reloadScreen(openedScreen.content.formScreen)();
        }
      }
    } else {
      yield*openNewUrl(this)(url, IUrlOpenMethod.OrigamTab, item.topic);
    }
  }

  *onScreenTabHandleClick(event: any, openedScreen: IOpenedScreen): Generator {
    const openedScreens = getOpenedScreens(this);
    openedScreens.activateItem(openedScreen.menuItemId, openedScreen.order);

    if (openedScreen.isSleeping) {
      openedScreen.isSleeping = false;
      const initUIResult = yield*this.initUIForScreen(openedScreen, false);
      yield*openedScreen.content!.start({
        initUIResult: initUIResult,
        preloadIsDirty: openedScreen.isSleepingDirty
      });
    } else if (
      openedScreen.content &&
      openedScreen.content.formScreen &&
      openedScreen.content.formScreen.refreshOnFocus &&
      !openedScreen.content.isLoading
    ) {
      if (!getIsFormScreenDirty(openedScreen.content.formScreen)) {
        yield*reloadScreen(openedScreen.content.formScreen)();
      }
    }
  }

  *closeForm(openedScreen: IOpenedScreen): Generator {
    // TODO: Refactor to get rid of code duplication
    const openedScreens = getOpenedScreens(openedScreen);

    const parentScreen = openedScreen.parentContext
      ? getOpenedScreen(openedScreen.parentContext)
      : undefined;

    const screenToActivate = parentScreen && !parentScreen.isClosed
      ? parentScreen
      : openedScreens.findTopmostItemExcept(openedScreen.menuItemId, openedScreen.order);

    openedScreens.deleteItem(openedScreen.menuItemId, openedScreen.order);
    if (openedScreen.dialogInfo) {
      if (openedScreen.isActive) {
        if (screenToActivate) {
          openedScreens.activateItem(screenToActivate.menuItemId, screenToActivate.order);
          if (screenToActivate.isSleeping) {
            screenToActivate.isSleeping = false;
            const initUIResult = yield*this.initUIForScreen(screenToActivate, false);
            yield*screenToActivate.content!.start({
              initUIResult: initUIResult,
              preloadIsDirty: screenToActivate.isSleepingDirty
            });
          }
        }
      }
    } else {
      if (openedScreen.isActive) {
        if (screenToActivate) {
          openedScreens.activateItem(screenToActivate.menuItemId, screenToActivate.order);

          if (screenToActivate.isSleeping) {
            screenToActivate.isSleeping = false;
            const initUIResult = yield*this.initUIForScreen(screenToActivate, false);
            yield*screenToActivate.content!.start({
              initUIResult: initUIResult,
              preloadIsDirty: screenToActivate.isSleepingDirty
            });
          } else if (
            screenToActivate.content &&
            screenToActivate.content.formScreen &&
            screenToActivate.content.formScreen.refreshOnFocus &&
            !screenToActivate.content.isLoading
          ) {
            if (!getIsFormScreenDirty(screenToActivate.content.formScreen)) {
              yield*reloadScreen(screenToActivate.content.formScreen)();
            }
          }
        }
      }
    }

    yield*this.destroyUI(openedScreen);

    if (openedScreen.content && openedScreen.content.formScreen) {
      const scope = scopeFor(openedScreen.content.formScreen);
      if (scope) scope.disposeWithChildren();
    }
    openedScreen.isClosed = true;
    openedScreen.content.formScreen?.formScreenLifecycle?.onClose?.();
  }

  *destroyUI(openedScreen: IOpenedScreen) {
    const api = getApi(this);
    if (openedScreen.content) {
      if (openedScreen.content.formScreen) {
        openedScreen.content.formScreen.dispose();
        yield *this.callCDestroyUI(getSessionId(openedScreen.content.formScreen));
      } else if (openedScreen.content.preloadedSessionId) {
        yield *this.callCDestroyUI(openedScreen.content.preloadedSessionId);
      }
    }
  }

  private *callCDestroyUI(sessionId: string): Generator {
    const api = getApi(this);
    yield api.destroyUI({FormSessionId: sessionId});
    this.closedSessionTracker.push(sessionId);
  }

  @bind
  *openNewForm(args: {
    id: string,
    type: IMainMenuItemType,
    label: string,
    isLazyLoading: boolean,
    dialogInfo: IDialogInfo | undefined,
    parameters: { [key: string]: any },
    parentContext?: any,
    requestParameters?: object | undefined,
    formSessionId?: string,
    isSessionRebirth?: boolean,
    isSleepingDirty?: boolean,
    refreshOnReturnType?: IRefreshOnReturnType,
    isSingleRecordEdit?: boolean,
    newRecordInitialValues?: {[p:string]: string},
    onClose?: ()=> void
  }
  ) {
    const openedScreens = getOpenedScreens(this);
    const existingItem = openedScreens.findLastExistingTabItem(args.id);
    const newFormScreen = createFormScreenEnvelope(args.formSessionId, args.refreshOnReturnType);
    newFormScreen.formScreenLifecycle.onClose = args.onClose;

    const newScreen = new OpenedScreen({
      menuItemId: args.id,
      menuItemType: args.type,
      order: existingItem ? existingItem.order + 1 : 0,
      tabTitle: args.label,
      content: newFormScreen,
      dialogInfo: args.dialogInfo,
      lazyLoading: args.isLazyLoading,
      parameters: args.parameters,
      isSleeping: args.isSessionRebirth,
      isSleepingDirty: args.isSleepingDirty,
      isNewRecordScreen: !!args.newRecordInitialValues,
    });
    try {
      openedScreens.pushItem(newScreen);
      if (!args.isSessionRebirth) {
        newScreen.parentContext = args.parentContext;
        openedScreens.activateItem(newScreen.menuItemId, newScreen.order);
      }

      if (args.isSessionRebirth) {
        return;
      }

      const initUIResult = yield*this.initUIForScreen(
        newScreen,
        !args.isSessionRebirth,
        args.requestParameters,
        args.isSingleRecordEdit,
        args.newRecordInitialValues
      );
      yield*newFormScreen.start(
        {
          initUIResult: initUIResult,
          preloadIsDirty: initUIResult.isDirty,
          createNewRecord: !!args.newRecordInitialValues,
        });
      const rowIdToSelect = args.parameters["id"];
      yield*this.selectAndOpenRowById(rowIdToSelect, newFormScreen);
      const formScreen = newScreen.content.formScreen;
      if (formScreen?.autoWorkflowNext) {
        yield onWorkflowNextClick(formScreen!)(undefined);
      }
    } catch (e) {
      yield*handleError(this)(e);
      yield*this.closeForm(newScreen);
      throw e;
    }
  }

  private*selectAndOpenRowById(rowIdToSelect: string, newFormScreen: FormScreenEnvelope) {
    if (rowIdToSelect && newFormScreen.formScreen) {
      for (const dataView of newFormScreen.formScreen.dataViews) {
        const hasTheRow = (dataView as IDataView).dataTable.rows
          .find(row => dataView.dataTable.getRowId(row) === rowIdToSelect) !== undefined;
        if (hasTheRow && dataView.activateFormView && !dataView.isHeadless) {
          yield*dataView.setSelectedRowId(rowIdToSelect);
          yield dataView.activateFormView({saveNewState: false});
          break;
        }
      }
    }
  }

  *initUIForScreen(
    screen: IOpenedScreen,
    isNewSession: boolean,
    requestParameters?: object | undefined,
    isSingleRecordEdit?: boolean,
    newRecordInitialValues?: {[p:string]: string}
  ): any {
    const api = getApi(this);
    if (requestParameters) {
      return yield api.initUI(requestParameters as any);
    }
    return yield api.initUI({
      Type: screen.menuItemType,
      Caption: screen.tabTitle,
      ObjectId: screen.menuItemId,
      FormSessionId: screen.content!.preloadedSessionId,
      IsNewSession: isNewSession,
      RegisterSession: true,
      DataRequested: !screen.lazyLoading,
      Parameters: screen.parameters,
      IsSingleRecordEdit: isSingleRecordEdit,
      NewRecordInitialValues: newRecordInitialValues,
      RequestCurrentRecordId: true
    });
  }

  *openNewUrl(url: string, title: string) {
    const openedScreens = getOpenedScreens(this);
    const canRefresh = yield*this.canRefresh(url);
    const newScreen = new WebScreen(title, url, url, 0, canRefresh);
    openedScreens.pushItem(newScreen);
    openedScreens.activateItem(newScreen.menuItemId, newScreen.order);
  }

  *canRefresh(url: string): Generator<any, boolean> {
    if(url.startsWith("internalApi/Report")){
      const reportId = url.split("/")[2];
      const api = getApi(this);
      const reportInfo = (yield api.getReportInfo({ReportId: reportId})) as any;
      return reportInfo.isWebReport === true;
    }else{
      return true;
    }
  }

  assignColors(colors: {[key: string]: string}){
    for (const colorEntry of Object.entries(colors)) {
      const hexColorName = colorEntry[0];
      let intColor = parseInt(colorEntry[1]);
      if(isNaN(intColor)){
        throw new Error(`Color code "${colorEntry[1]}" assigned to color "${hexColorName}" could not be parsed to integer`)
      }
      const hexColor = "#" + intColor.toString(16).padStart(6, '0');
      const root = document.querySelector(':root')! as any;
      root.style.setProperty(hexColorName, hexColor);

      const rgbColorName = hexColorName + "-rgb";
      const rgbColor = hexToRgb(hexColor);
      root.style.setProperty(rgbColorName, rgbColor);
    }
  }

  *initPortal(): any {
    const api = getApi(this);
    const portalInfo = yield api.initPortal();

    this.assignColors(portalInfo.style.colors);

    if (portalInfo.title) {
      document.title = portalInfo.title;
    }
    this.userInfo = {
      userName: portalInfo.userName,
      avatarLink: portalInfo.avatarLink,
    };
    this.logoUrl = portalInfo.logoUrl;
    this.customAssetsRoute = portalInfo.customAssetsRoute;
    this.portalSettings = {
      showChat: portalInfo.chatRefreshInterval > 0,
      showWorkQueues: portalInfo.workQueueListRefreshInterval > 0,
      helpUrl: portalInfo.helpUrl,
      showTooltipsForMemoFieldsOnly: portalInfo.showTooltipsForMemoFieldsOnly,
      rowStatesDebouncingDelayMilliseconds: portalInfo.rowStatesDebouncingDelayMilliseconds,
      dropDownTypingDebouncingDelayMilliseconds: portalInfo.dropDownTypingDebouncingDelayMilliseconds,
      filterConfig: {
        caseSensitive: portalInfo.filteringConfig.caseSensitive,
        accentSensitive: portalInfo.filteringConfig.accentSensitive
      }
    };
    getWorkbench(this).lookupMultiEngine.lookupLoaderMulti.getLookupLabelExDebouncingDelayMillis =
      portalInfo.getLookupLabelExDebouncingDelayMilliseconds
    const menuUI = findMenu(portalInfo.menu);
    assignIIds(menuUI);
    getFavorites(this).setXml(portalInfo.favorites);
    getMainMenuEnvelope(this).setMainMenu(new MainMenuContent({menuUI}));
    getSearcher(this).indexMainMenu(menuUI);

    if(portalInfo.initialScreenId && (portalInfo.sessions.length === 0 || !portalInfo.sessions.some((session: any) => session.objectId === portalInfo.initialScreenId))){
      const menuItem = getMainMenuItemById(this, portalInfo.initialScreenId);
      yield onMainMenuItemClick(this)({
        event: null,
        item: menuItem,
        idParameter: undefined
      });
    }

    if (!DEBUG_CLOSE_ALL_FORMS()) {
      for (let session of portalInfo.sessions) {
        const menuItem = getMainMenuItemById(this, session.objectId);
        const lazyLoading = menuItem
          ? menuItem?.attributes?.lazyLoading === "true"
          : session.type === IMainMenuItemType.WorkQueue;
        yield*this.openNewForm( {
          id: session.objectId,
          type: session.type,
          label: session.caption,
          isLazyLoading: lazyLoading,
          dialogInfo: undefined,
          parameters: {},
          formSessionId: session.formSessionId,
          isSessionRebirth: true,
          isSleepingDirty: session.isDirty
        });
      }
    } else {
      for (let session of portalInfo.sessions) {
        yield this.callCDestroyUI(session.formSessionId);
      }
    }
    const openedScreens = getOpenedScreens(this);
    if (openedScreens.items.length > 0) {
      const screenToOpen = portalInfo.initialScreenId
        ? openedScreens.items.find(screen => screen.menuItemId === portalInfo.initialScreenId) ?? openedScreens.items[0]
        : openedScreens.items[0];
      openedScreens.activateItem(screenToOpen.menuItemId, screenToOpen.order);
      if(screenToOpen.isSleeping){
        screenToOpen.isSleeping = false;
        const initUIResult = yield*this.initUIForScreen(screenToOpen, false);
        if (screenToOpen.content) {
          yield*screenToOpen.content.start(
            {
              initUIResult: initUIResult,
              preloadIsDirty: screenToOpen.isSleepingDirty
            }
          );
        }
      }
    }

    if (this.portalSettings?.showWorkQueues) {
      yield*getWorkQueues(this).startTimer(portalInfo.workQueueListRefreshInterval);
    }

    if (this.portalSettings?.showChat) {
      yield*getChatrooms(this).startTimer(portalInfo.chatRefreshInterval);
    }

    if (portalInfo.notificationBoxRefreshInterval > 0) {
      yield*getNotifications(this).startTimer(portalInfo.notificationBoxRefreshInterval);
    }
  }

  *run(): Generator {
    yield*this.initPortal();
  }

  wasRecentlyClosed(sessionId: string){
    return this.closedSessionTracker.has(sessionId);
  }

  parent?: any;
}
