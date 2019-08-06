import { sInitUI, onInitUIDone, sFormScreenRunning } from "./constants";

export const FormScreenDef = {
  initial: sInitUI,
  states: {
    [sInitUI]: {
      invoke: { src: "initUI" },
      on: {
        onInitUIDone: {
          target: sFormScreenRunning,
          actions: "applyInitUIResult"
        }
      }
    },
    [sFormScreenRunning]: {}
  }
};
