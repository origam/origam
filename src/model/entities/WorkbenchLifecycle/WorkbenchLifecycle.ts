import { action, createAtom, flow } from "mobx";
import { interpret, Machine } from "xstate";
import { IOpenedScreen, IDialogInfo } from "../types/IOpenedScreen";
import { IWorkbenchLifecycle } from "../types/IWorkbenchLifecycle";
import { IEvent } from "./types";
import { WorkbenchLifecycleGraph } from "./WorkbenchLifecycleGraph";
import { getOpenedScreens } from "model/selectors/getOpenedScreens";
import { getApi } from "model/selectors/getApi";
import { getMainMenuEnvelope } from "model/selectors/MainMenu/getMainMenuEnvelope";
import { findMenu } from "xmlInterpreters/menuXml";
import { MainMenuContent } from "../MainMenu";
import { createLoadingFormScreen } from "model/factories/createLoadingFormScreen";
import { createOpenedScreen } from "model/factories/createOpenedScreen";
import { IMainMenuItemType } from "../types/IMainMenu";
import { DialogInfo } from "../OpenedScreen";
import { onInitPortalDone } from "./constants";

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

  @action.bound
  onMainMenuItemClick(args: { event: any; item: any }): void {
    const { type, id, label, dialogWidth, dialogHeight } = args.item.attributes;
    const { event } = args;

    const openedScreens = getOpenedScreens(this);
    const existingItem = openedScreens.findLastExistingItem(id);
    let dialogInfo: IDialogInfo | undefined;
    if (type === IMainMenuItemType.FormRefWithSelection) {
      dialogInfo = new DialogInfo(
        parseInt(dialogWidth, 10),
        parseInt(dialogHeight, 10)
      );
    }
    if (!event.ctrlKey) {
      if (existingItem) {
        openedScreens.activateItem(id, existingItem.order);
      } else {
        const newFormScreen = createLoadingFormScreen();
        const newScreen = createOpenedScreen(id, type, 0, label, newFormScreen, dialogInfo);
        openedScreens.pushItem(newScreen);
        openedScreens.activateItem(newScreen.menuItemId, newScreen.order);
        newFormScreen.run();
      }
    } else {
      if (existingItem) {
        const newFormScreen = createLoadingFormScreen();
        const newScreen = createOpenedScreen(
          id,
          type,
          existingItem.order + 1,
          label,
          newFormScreen, dialogInfo
        );
        openedScreens.pushItem(newScreen);
        openedScreens.activateItem(newScreen.menuItemId, newScreen.order);
        newFormScreen.run();
      } else {
        const newFormScreen = createLoadingFormScreen();
        const newScreen = createOpenedScreen(id, type, 0, label, newFormScreen, dialogInfo);
        openedScreens.pushItem(newScreen);
        openedScreens.activateItem(id, 0);
        newFormScreen.run();
      }
    }
  }

  @action.bound
  onScreenTabHandleClick(event: any, openedScreen: IOpenedScreen): void {
    const openedScreens = getOpenedScreens(this);
    openedScreens.activateItem(openedScreen.menuItemId, openedScreen.order);
  }

  @action.bound
  onScreenTabCloseClick(event: any, openedScreen: IOpenedScreen): void {
    event.stopPropagation();
    console.log(openedScreen);
    const openedScreens = getOpenedScreens(this);
    const closestScreen = openedScreens.findClosestItem(
      openedScreen.menuItemId,
      openedScreen.order
    );
    if (closestScreen) {
      openedScreens.activateItem(closestScreen.menuItemId, closestScreen.order);
    }
    openedScreens.deleteItem(openedScreen.menuItemId, openedScreen.order);
  }

  *initPortal() {
    const api = getApi(this);
    const portalInfo = yield api.initPortal();
    console.log(portalInfo);
    getMainMenuEnvelope(this).setMainMenu(
      new MainMenuContent({ menuUI: findMenu(portalInfo.menu) })
    );
    this.interpreter.send(onInitPortalDone);
  }

  @action.bound
  run(): void {
    this.interpreter.start();
  }
  parent?: any;
}
