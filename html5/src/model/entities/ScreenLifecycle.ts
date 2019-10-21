
import { IScreenLifecycle } from "./types/IScreenLifecycle";
import { Machine, interpret } from "xstate";
import { createAtom } from "mobx";

export class ScreenLifecycle implements IScreenLifecycle {
  $type_IScreenLifecycle: 1 = 1;
  
  machine = Machine({
    // initial: sLoadMenu,
    states: {}
  }, {
      services: {}
    });
  stateAtom = createAtom("screenLifecycleState");
  interpreter = interpret(this.machine).onTransition((state, event) => {
    console.log("Screen lifecycle:", state, event);
    this.stateAtom.reportChanged();
  });

  get state() {
    this.stateAtom.reportObserved();
    return this.interpreter.state;
  }

  parent?: any;
}
