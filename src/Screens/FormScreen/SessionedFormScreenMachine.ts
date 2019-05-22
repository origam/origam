import { Machine } from "xstate";

export class SessionedFormScreenMachine {
  machine = Machine({
    initial: "loadMaster",
    states: {
      loadMaster: { on: { DONE: "newSession" } },
      newSession: { on: { DONE: "loadDetails" } },
      loadDetails: { on: { DONE: "idle" } },
      idle: {}
    }
  });
}
