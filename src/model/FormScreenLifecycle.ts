import { IFormScreenLifecycle } from "./types/IFormScreenLifecycle";
import { createAtom, flow, action } from "mobx";
import { interpret, Machine } from "xstate";
import { getApi } from "./selectors/getApi";
import { getMenuItemId } from "./selectors/getMenuItemId";
import xmlJs from "xml-js";
import { interpretScreenXml } from "../xmlInterpreters/screenXml";
import { getOpenedScreen } from "./selectors/getOpenedScreen";

const loadScreenSuccess = "loadScreenSuccess";
const loadScreenFailed = "loadScreenFailed";

const sLoadScreen = "sLoadScreen";
const sIdle = "sIdle";

export class FormScreenLifecycle implements IFormScreenLifecycle {
  machine = Machine(
    {
      initial: sLoadScreen,
      states: {
        [sLoadScreen]: {
          invoke: { src: "loadScreen" },
          on: {
            [loadScreenSuccess]: sIdle
          }
        },
        [sIdle]: {}
      }
    },
    {
      services: {
        loadScreen: (ctx, event) => (send, onEvent) =>
          flow(this.loadScreen.bind(this))()
      },
      actions: {}
    }
  );

  stateAtom = createAtom("formScreenLifecycleState");
  interpreter = interpret(this.machine).onTransition((state, event) => {
    console.log("FormScreen lifecycle:", state, event);
    this.stateAtom.reportChanged();
  });

  get state() {
    this.stateAtom.reportObserved();
    return this.interpreter.state;
  }

  *loadScreen() {
    const api = getApi(this);
    const menuItemId = getMenuItemId(this);
    const openedScreen = getOpenedScreen(this);
    const screenXmlObj = yield api.getScreen(menuItemId);
    const screen = interpretScreenXml(screenXmlObj, this);
    openedScreen.setContent(screen);
    // console.log(screen);
  }

  @action.bound
  run(): void {
    this.interpreter.start();
  }

  parent?: any;
}
