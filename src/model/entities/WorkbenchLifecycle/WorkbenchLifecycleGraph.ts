import { sInitPortal, onInitPortalDone, sIdle } from "./constants";

export const WorkbenchLifecycleGraph = {
  initial: sInitPortal,
  states: {
    [sInitPortal]: {
      invoke: { src: "initPortal" },
      on: {
        [onInitPortalDone]: sIdle
      }
    },
    [sIdle]: {
      on: {}
    }
  }
};
