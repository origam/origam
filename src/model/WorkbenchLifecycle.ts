import { Machine } from "xstate";

export class WorkbenchLifecycle {
  machine = Machine({
    states: {
      sLoadMenu: {
        invoke: {
          src: "loadMenu",
          onDone: "sIdle",
          onError: "sLoadMenuFailed"
        }
      },
      sLoadMenuFailed: {
        on: {
          ok: { actions: "logout", target: "sEnd" }
        }
      },
      sIdle: {},
      sEnd: {
        type: "final"
      }
    }
  });
}
