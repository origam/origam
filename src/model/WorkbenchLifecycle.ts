import { Machine, interpret } from "xstate";
import { IWorkbenchLifecycle } from "./types/IWorkbenchLifecycle";
import { action, createAtom, flow } from "mobx";

import { getApi } from "./selectors/getApi";
import { getWorkbench } from "./selectors/getWorkbench";
import { LoadingMainMenu, MainMenu } from "./MainMenu";
import { findMenu } from "../xmlInterpreters/menuXml";

const sLoadMenu = "sLoadMenu";
const sLoadMenuFailed = "sLoadMenuFailed";
const sIdle = "sIdle";

const loadMenuFailed = "loadMenuFailed";
const loadMenuSuccessful = "loadMenuSuccessful";

export class WorkbenchLifecycle implements IWorkbenchLifecycle {
  machine = Machine(
    {
      initial: sLoadMenu,
      states: {
        [sLoadMenu]: {
          invoke: { src: "loadMenu" },
          on: {
            [loadMenuSuccessful]: sIdle,
            [loadMenuFailed]: sLoadMenuFailed
          }
        },
        [sLoadMenuFailed]: {
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
        loadMenu: (ctx, event) => (send, onEvent) =>
          flow(this.loadMenu.bind(this))()
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

  *loadMenu() {
    try {
      const api = getApi(this);
      const workbench = getWorkbench(this);
      workbench.setMainMenu(new LoadingMainMenu());
      const menu = yield api.getMenu();
      workbench.setMainMenu(new MainMenu({ menuUI: findMenu(menu) }));
      this.interpreter.send(loadMenuSuccessful);
    } catch (e) {
      this.interpreter.send(loadMenuFailed);
      console.error(e);
    }
  }

  @action.bound
  run(): void {
    this.interpreter.start();
  }
}
