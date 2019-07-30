import { Machine, interpret } from "xstate";
import { IWorkbenchLifecycle } from "./types/IWorkbenchLifecycle";
import { action, createAtom, flow } from "mobx";

import { getApi } from "../selectors/getApi";
import { getWorkbench } from "../selectors/getWorkbench";
import { LoadingMainMenu, MainMenu } from "./MainMenu";
import { findMenu } from "../../xmlInterpreters/menuXml";
import { ClientFulltextSearch } from "./ClientFulltextSearch";
import { getOpenedScreens } from "model/selectors/getOpenedScreens";
import { createLoadingFormScreen } from "model/factories/createLoadingFormScreen";
import { createOpenedScreen } from "model/factories/createOpenedScreen";
import { IOpenedScreen } from "./types/IOpenedScreen";

// const sLoadMenu = "sLoadMenu";
const sInitPortalFailed = "sInitPortalFailed";
const sIdle = "sIdle";
const sPerformInitPortal = "sPerformInitPortal";

const loadMenuFailed = "loadMenuFailed";
const loadMenuSuccessful = "loadMenuSuccessful";
const onMainMenuItemClicked = "onMainMenuItemClicked";

const initPortalSuccessful = "initPortalSuccessful";
const initPortalFailed = "initPortalFailed";

/*const loadScreenSuccessful = "loadScreenSuccessful";
const loadScreenFailed = "loadScreenFailed";*/

export class WorkbenchLifecycle implements IWorkbenchLifecycle {
  $type_IWorkbenchLifecycle: 1 = 1;

  parent?: any;
  machine = Machine(
    {
      initial: sPerformInitPortal,
      states: {
        [sPerformInitPortal]: {
          invoke: { src: "initPortal" },
          on: {
            [initPortalSuccessful]: sIdle,
            [initPortalFailed]: sInitPortalFailed
          }
        },
        [sInitPortalFailed]: {
          on: {
            ok: { actions: "logout", target: "sEnd" }
          }
        },
        sIdle: {},
        sEnd: {
          type: "final"
        }
      }
    },
    {
      services: {
        initPortal: (ctx, event) => (send, onEvent) =>
          flow(this.initPortal.bind(this))()
      }
    }
  );

  stateAtom = createAtom("workbenchLifecycleState");
  interpreter = interpret(this.machine).onTransition((state, event) => {
    console.log("Workbench lifecycle:", state, event);
    this.stateAtom.reportChanged();
  });

  get state() {
    this.stateAtom.reportObserved();
    return this.interpreter.state;
  }

  /**loadMenu() {
    try {
      const api = getApi(this);
      const workbench = getWorkbench(this);
      workbench.setMainMenu(new LoadingMainMenu());
      const menu = yield api.getMenu();
      workbench.setMainMenu(new MainMenu({ menuUI: findMenu(menu) }));
      new ClientFulltextSearch().indexMainMenu(menu)
      this.interpreter.send(loadMenuSuccessful);
    } catch (e) {
      this.interpreter.send(loadMenuFailed);
      console.error(e);
    }
  }*/

  *initPortal() {
    const api = getApi(this);
    const workbench = getWorkbench(this);
    workbench.setMainMenu(new LoadingMainMenu());
    const portalInfo = yield api.initPortal();
    console.log(portalInfo);
    workbench.setMainMenu(new MainMenu({ menuUI: findMenu(portalInfo.menu) }));
    this.interpreter.send(initPortalSuccessful);
  }

  @action.bound
  onMainMenuItemClick(args: { event: any; item: any }): void {
    const { type, id, label } = args.item.attributes;
    const { event } = args;
    switch (type) {
      case "FormReferenceMenuItem":
        {
          const openedScreens = getOpenedScreens(this);
          const existingItem = openedScreens.findLastExistingItem(id);
          if (!event.ctrlKey) {
            if (existingItem) {
              openedScreens.activateItem(id, existingItem.order);
            } else {
              const newFormScreen = createLoadingFormScreen();
              const newScreen = createOpenedScreen(id, 0, label, newFormScreen);
              openedScreens.pushItem(newScreen);
              openedScreens.activateItem(newScreen.menuItemId, newScreen.order);
              newFormScreen.run();
            }
          } else {
            if (existingItem) {
              const newFormScreen = createLoadingFormScreen();
              const newScreen = createOpenedScreen(
                id,
                existingItem.order + 1,
                label,
                newFormScreen
              );
              openedScreens.pushItem(newScreen);
              openedScreens.activateItem(newScreen.menuItemId, newScreen.order);
              newFormScreen.run();
            } else {
              const newFormScreen = createLoadingFormScreen();
              const newScreen = createOpenedScreen(id, 0, label, newFormScreen);
              openedScreens.pushItem(newScreen);
              openedScreens.activateItem(id, 0);
              newFormScreen.run();
            }
          }
          console.log(openedScreens.items);
        }
        break;
      case "FormReferenceMenuItem_WithSelection":
        break;
    }
  }

  @action.bound
  onScreenTabHandleClick(event: any, openedScreen: IOpenedScreen) {
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


  @action.bound
  run(): void {
    this.interpreter.start();
  }
}
