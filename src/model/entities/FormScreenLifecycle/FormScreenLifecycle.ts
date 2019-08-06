import { action, createAtom, flow, when } from "mobx";
import { interpret, Machine } from "xstate";
import { loadFreshData } from "../../actions/DataView/loadFreshData";
import { getFormScreen } from "../../selectors/FormScreen/getFormScreen";
import { getApi } from "../../selectors/getApi";
import { IFormScreenLifecycle } from "../types/IFormScreenLifecycle";
import { FormScreenDef } from "./FormScreenDef";
import { getMenuItemId } from "../../selectors/getMenuItemId";
import { onInitUIDone } from "./constants";
import { getMenuItemType } from "model/selectors/getMenuItemType";
import { interpretScreenXml } from "xmlInterpreters/screenXml";
import { getDataViewList } from "model/selectors/FormScreen/getDataViewList";
import { IOpenedScreen } from "../types/IOpenedScreen";
import { getOpenedScreen } from "../../selectors/getOpenedScreen";

export class FormScreenLifecycle implements IFormScreenLifecycle {
  $type_IFormScreenLifecycle: 1 = 1;

  machine = Machine(FormScreenDef, {
    services: {
      initUI: (ctx, event) => (send, onEvent) => flow(this.initUI.bind(this))()
    },
    actions: {
      applyInitUIResult: (ctx, event) => this.applyInitUIResult(event as any)
    }
  });

  stateAtom = createAtom("formScreenLifecycleState");
  interpreter = interpret(this.machine).onTransition((state, event) => {
    console.log("FormScreen lifecycle:", state, event);
    this.stateAtom.reportChanged();
  });

  get state() {
    this.stateAtom.reportObserved();
    return this.interpreter.state;
  }

  *initUI() {
    const api = getApi(this);
    const menuItemId = getMenuItemId(this);
    const menuItemType = getMenuItemType(this);
    const initUIResult = yield api.initUI({
      Type: menuItemType,
      ObjectId: menuItemId,
      FormSessionId: undefined,
      IsNewSession: true,
      RegisterSession: true,
      DataRequested: false
    });
    console.log(initUIResult);
    this.interpreter.send({ type: onInitUIDone, initUIResult });
    return null;
  }

  @action.bound
  applyInitUIResult(args: { initUIResult: any }) {
    const openedScreen = getOpenedScreen(this);
    const screenXmlObj = args.initUIResult.formDefinition;
    const screen = interpretScreenXml(screenXmlObj, this);
    openedScreen.setContent(screen);
    getDataViewList(screen).forEach(dv => dv.run());
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
