import {
  onCreateRow,
  onDeleteRow,
  sDeleteRow,
  onDeleteRowDone
} from "./constants";
import {
  sInitUI,
  onInitUIDone,
  sFormScreenRunning,
  onFlushData,
  sFlushData,
  onFlushDataDone,
  sCreateRow,
  onCreateRowDone
} from "./constants";

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
        },
        [onCreateRow]: {
          target: sCreateRow
        },
        [onDeleteRow]: {
          target: sDeleteRow
        }
      }
    },
    [sFlushData]: {
      invoke: { src: "flushData" },
      on: {
        [onFlushDataDone]: {
          target: sFormScreenRunning
        }
      }
    },
    [sCreateRow]: {
      invoke: { src: "createRow" },
      on: {
        [onCreateRowDone]: {
          target: sFormScreenRunning
        }
      }
    },
    [sDeleteRow]: {
      invoke: { src: "deleteRow" },
      on: {
        [onDeleteRowDone]: {
          target: sFormScreenRunning
        }
      }
    }
  }
};
