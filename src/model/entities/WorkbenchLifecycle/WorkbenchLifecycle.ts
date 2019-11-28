import { createFormScreenEnvelope } from "model/factories/createFormScreenEnvelope";
import { createOpenedScreen } from "model/factories/createOpenedScreen";
import { getApi } from "model/selectors/getApi";
import { getClientFulltextSearch } from "model/selectors/getClientFulltextSearch";
import { getOpenedDialogScreens } from "model/selectors/getOpenedDialogScreens";
import { getOpenedScreens } from "model/selectors/getOpenedScreens";
import { getMainMenuEnvelope } from "model/selectors/MainMenu/getMainMenuEnvelope";
import { findMenu } from "xmlInterpreters/menuXml";
import { MainMenuContent } from "../MainMenu";
import { DialogInfo } from "../OpenedScreen";
import { IMainMenuItemType } from "../types/IMainMenu";
import { IDialogInfo, IOpenedScreen } from "../types/IOpenedScreen";
import { IWorkbenchLifecycle } from "../types/IWorkbenchLifecycle";

export class WorkbenchLifecycle implements IWorkbenchLifecycle {
  $type_IWorkbenchLifecycle: 1 = 1;

  *onMainMenuItemClick(args: { event: any; item: any }): Generator {
    const {
      type,
      id,
      label,
      dialogWidth,
      dialogHeight,
      dontRequestData
    } = args.item.attributes;
    const { event } = args;

    const openedScreens = getOpenedScreens(this);

    let dialogInfo: IDialogInfo | undefined;
    if (type === IMainMenuItemType.FormRefWithSelection) {
      dialogInfo = new DialogInfo(
        parseInt(dialogWidth, 10),
        parseInt(dialogHeight, 10)
      );
    }
    if (!event.ctrlKey) {
      const existingItem = openedScreens.findLastExistingItem(id);
      if (existingItem) {
        openedScreens.activateItem(id, existingItem.order);
      } else {
        yield* this.openNewForm(
          id,
          type,
          label,
          dontRequestData === "true",
          dialogInfo,
          {}
        );
      }
    } else {
      yield* this.openNewForm(
        id,
        type,
        label,
        dontRequestData === "true",
        dialogInfo,
        {}
      );
    }
  }

  *onScreenTabHandleClick(event: any, openedScreen: IOpenedScreen): Generator {
    const openedScreens = getOpenedScreens(this);
    openedScreens.activateItem(openedScreen.menuItemId, openedScreen.order);
  }

  *closeForm(openedScreen: IOpenedScreen): Generator {
    // TODO: Refactor to get rid of code duplication
    if (openedScreen.dialogInfo) {
      const openedScreens = getOpenedDialogScreens(openedScreen);
      if (openedScreen.isActive) {
        const closestScreen = openedScreens.findClosestItem(
          openedScreen.menuItemId,
          openedScreen.order
        );
        if (closestScreen) {
          openedScreens.activateItem(
            closestScreen.menuItemId,
            closestScreen.order
          );
        }
      }
      openedScreens.deleteItem(openedScreen.menuItemId, openedScreen.order);
    } else {
      const openedScreens = getOpenedScreens(openedScreen);
      if (openedScreen.isActive) {
        const closestScreen = openedScreens.findClosestItem(
          openedScreen.menuItemId,
          openedScreen.order
        );
        if (closestScreen) {
          openedScreens.activateItem(
            closestScreen.menuItemId,
            closestScreen.order
          );
        }
      }
      openedScreens.deleteItem(openedScreen.menuItemId, openedScreen.order);
    }
  }

  *openNewForm(
    id: string,
    type: IMainMenuItemType,
    label: string,
    dontRequestData: boolean,
    dialogInfo: IDialogInfo | undefined,
    parameters: { [key: string]: any },
    formSessionId?: string,
    isSessionRebirth?: boolean,
    registerSession?: true, //boolean
  ) {
    const openedScreens = getOpenedScreens(this);
    const openedDialogScreens = getOpenedDialogScreens(this);
    const existingItem = openedScreens.findLastExistingItem(id);
    const newFormScreen = createFormScreenEnvelope();
    const newScreen = createOpenedScreen(
      id,
      type,
      existingItem ? existingItem.order + 1 : 0,
      label,
      newFormScreen,
      dontRequestData,
      dialogInfo,
      parameters
    );
    if (newScreen.dialogInfo) {
      openedDialogScreens.pushItem(newScreen);
      openedDialogScreens.activateItem(newScreen.menuItemId, newScreen.order);
    } else {
      openedScreens.pushItem(newScreen);
      openedScreens.activateItem(newScreen.menuItemId, newScreen.order);
    }
    const api = getApi(this);

    const initUIResult = yield api.initUI({
      Type: type,
      ObjectId: id,
      FormSessionId: formSessionId,
      IsNewSession: !isSessionRebirth,
      RegisterSession: true, //!!registerSession,
      DataRequested: !dontRequestData,
      Parameters: parameters
    });
    console.log(initUIResult);

    yield* newFormScreen.start(initUIResult);
  }

  *initPortal() {
    const api = getApi(this);
    const portalInfo = yield api.initPortal();

    for (let session of portalInfo.sessions) {
      /*const resSession = yield api.initUI({
        Type: session.type,
        FormSessionId: session.formSessionId,
        IsNewSession: false,
        RegisterSession: true,
        DataRequested: true,
        ObjectId: session.objectId,
        Parameters: undefined
      });*/
      yield* this.openNewForm(
        session.objectId,
        session.type,
        "", // TODO: Find in menu
        false, // TODO: Find in menu
        undefined, // TODO: Find in... menu?
        {},
        session.formSessionId,
        true,
        true
      );
    }
    console.log(portalInfo);
    const menuUI = findMenu(portalInfo.menu);
    getMainMenuEnvelope(this).setMainMenu(new MainMenuContent({ menuUI }));
    getClientFulltextSearch(this).indexMainMenu(menuUI);
  }

  *initUI() {}

  *run(): Generator {
    yield* this.initPortal();
  }

  parent?: any;
}
