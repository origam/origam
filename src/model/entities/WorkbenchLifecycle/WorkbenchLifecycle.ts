import { action, createAtom, flow } from "mobx";
import { createOpenedScreen } from "model/factories/createOpenedScreen";
import { getApi } from "model/selectors/getApi";
import { getClientFulltextSearch } from "model/selectors/getClientFulltextSearch";
import { getOpenedScreens } from "model/selectors/getOpenedScreens";
import { getMainMenuEnvelope } from "model/selectors/MainMenu/getMainMenuEnvelope";
import { findMenu } from "xmlInterpreters/menuXml";
import { interpret, Machine } from "xstate";
import { MainMenuContent } from "../MainMenu";
import { DialogInfo } from "../OpenedScreen";
import { IMainMenuItemType } from "../types/IMainMenu";
import { IDialogInfo, IOpenedScreen } from "../types/IOpenedScreen";
import { IWorkbenchLifecycle } from "../types/IWorkbenchLifecycle";
import { onInitPortalDone } from "./constants";
import { IEvent } from "./types";
import { WorkbenchLifecycleGraph } from "./WorkbenchLifecycleGraph";
import { createFormScreenEnvelope } from "model/factories/createFormScreenEnvelope";
import { getOpenedDialogScreens } from "model/selectors/getOpenedDialogScreens";

export class WorkbenchLifecycle implements IWorkbenchLifecycle {
  $type_IWorkbenchLifecycle: 1 = 1;

  machine = Machine<any, any, IEvent>(WorkbenchLifecycleGraph, {
    services: {
      initPortal: (ctx, event) => (send, onEvent) =>
        flow(this.initPortal.bind(this))()
    }
  });

  stateAtom = createAtom("workbenchLifecycleState");
  interpreter = interpret(this.machine).onTransition((state, event) => {
    console.log("Workbench lifecycle:", state, event);
    this.stateAtom.reportChanged();
  });

  get state() {
    this.stateAtom.reportObserved();
    return this.interpreter.state;
  }


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

  *onScreenTabCloseClick(event: any, openedScreen: IOpenedScreen): Generator {
    event.stopPropagation();
    this.closeForm(openedScreen);
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
    parameters: { [key: string]: any }
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
    yield* newFormScreen.start();
  }

  *initPortal() {
    const api = getApi(this);
    const portalInfo = yield api.initPortal();
    console.log(portalInfo);
    const menuUI = findMenu(portalInfo.menu);
    getMainMenuEnvelope(this).setMainMenu(new MainMenuContent({ menuUI }));
    getClientFulltextSearch(this).indexMainMenu(menuUI);
    this.interpreter.send(onInitPortalDone);
  }

  @action.bound
  run(): void {
    this.interpreter.start();
  }
  parent?: any;
}
