import { sInitUI, onInitUIDone, sFormScreenRunning, onFlushData, sFlushData, onFlushDataDone } from './constants';

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
    [sFormScreenRunning]: {
      on: {
        [onFlushData]: {
          target: sFlushData
        }
      }
    },
    [sFlushData]: {
      invoke: {src: "flushData"},
      on: {
        [onFlushDataDone]: {
          target: sFormScreenRunning
        }
      }
    }
  }
};
