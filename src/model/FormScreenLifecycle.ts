import { IFormScreenLifecycle } from "./types/IFormScreenLifecycle";
import { createAtom, flow, action, when } from "mobx";
import { interpret, Machine } from "xstate";
import { getApi } from "./selectors/getApi";
import { getMenuItemId } from "./selectors/getMenuItemId";
import { interpretScreenXml } from "../xmlInterpreters/screenXml";
import { getOpenedScreen } from "./selectors/getOpenedScreen";
import { getFormScreen } from "./selectors/FormScreen/getFormScreen";
import { loadFreshData } from "./actions/DataView/loadFreshData";
import { getDataViewList } from "./selectors/FormScreen/getDataViewList";

const loadScreenSuccess = "loadScreenSuccess";
const loadScreenFailed = "loadScreenFailed";

const sLoadScreen = "sLoadScreen";
const sLoadDataViews = "sLoadDataViews";
const sIdle = "sIdle";

export class FormScreenLifecycle implements IFormScreenLifecycle {
  $type_IFormScreenLifecycle: 1 = 1;

  machine = Machine(
    {
      initial: sLoadScreen,
      states: {
        [sLoadScreen]: {
          invoke: { src: "loadScreen", onDone: sLoadDataViews }
        },
        [sLoadDataViews]: {
          invoke: { src: "loadDataViews", onDone: sIdle }
        },
        [sIdle]: {}
      }
    },
    {
      services: {
        loadScreen: (ctx, event) => flow(this.loadScreen.bind(this))() as any,
        loadDataViews: (ctx, event) =>
          flow(this.loadDataViews.bind(this))() as any
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
    getDataViewList(screen).forEach(dv => dv.run());
    // console.log(screen);
  }

  *loadDataViews() {
    const screen = getFormScreen(this);
    screen.rootDataViews.forEach(s => loadFreshData(s));
    yield when(() => screen.rootDataViews.every(dv => !dv.isWorking));
  }

  @action.bound
  run(): void {
    this.interpreter.start();
  }

  parent?: any;
}
