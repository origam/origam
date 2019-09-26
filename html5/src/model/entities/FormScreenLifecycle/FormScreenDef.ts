import {
  onCreateRow,
  onCreateRowDone,
  onDeleteRow,
  onDeleteRowDone,
  onExecuteAction,
  onExecuteActionDone,
  onFlushData,
  onFlushDataDone,
  onPerformCancel,
  onPerformNoSave,
  onPerformSave,
  onRefreshSessionDone,
  onRequestScreenClose,
  onRequestScreenReload,
  onSaveSession,
  onSaveSessionDone,
  sCreateRow,
  sDeleteRow,
  sExecuteAction,
  sFlushData,
  sFormScreenRunning,
  sInitUI,
  sLoadData,
  sQuestionSaveDataBeforeClosing,
  sQuestionSaveDataBeforeRefresh,
  sRefreshSession,
  sSaveSession,
  onExecuteActionFailed
} from "./constants";
import { ErrorStateDef } from "../ErrorDialog";

export const FormScreenDef = () => ({
  initial: sInitUI,
  states: {
    [sInitUI]: {
      invoke: { src: "initUI" },
      on: {
        onInitUIDone: [
          {
            cond: "isReadData",
            target: sFormScreenRunning,
            actions: "applyInitUIResult"
          },
          {
            target: sLoadData,
            actions: "applyInitUIResult"
          }
        ]
      }
    },
    [sLoadData]: {
      invoke: { src: "loadData" },
      on: {
        onLoadDataDone: {
          target: sFormScreenRunning
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
        },
        [onSaveSession]: {
          target: sSaveSession
        },
        [onExecuteAction]: {
          target: sExecuteAction
        },
        [onRequestScreenClose]: sQuestionSaveDataBeforeClosing,
        [onRequestScreenReload]: sQuestionSaveDataBeforeRefresh
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
    },
    [sSaveSession]: {
      invoke: { src: "saveSession" },
      on: {
        [onSaveSessionDone]: sFormScreenRunning
      }
    },
    [sRefreshSession]: {
      invoke: { src: "refreshSession" },
      on: {
        [onRefreshSessionDone]: sFormScreenRunning
      }
    },
    [sExecuteAction]: {
      invoke: { src: "executeAction" },
      on: {
        [onExecuteActionDone]: sFormScreenRunning,
        [onExecuteActionFailed]: 'sError'
      }
    },
    [sQuestionSaveDataBeforeClosing]: RequestCloseFormDef(),
    [sQuestionSaveDataBeforeRefresh]: RequestReloadFormDef(),

    sError: {
      ...ErrorStateDef(),
      onDone: sFormScreenRunning
    }
  }
});

const RequestCloseFormDef = () => ({
  initial: "sInit",
  states: {
    sInit: {
      on: {
        "": [
          { cond: "isDirtySession", target: "sQuestion" },
          { target: "sCloseForm" }
        ]
      }
    },

    sQuestion: {
      invoke: { src: "questionSaveData" },
      on: {
        [onPerformSave]: {
          target: sSaveSession
        },
        [onPerformNoSave]: {
          target: "sCloseForm"
        },
        [onPerformCancel]: {
          target: `#(machine).${sFormScreenRunning}`
        }
      }
    },

    [sSaveSession]: {
      invoke: { src: "saveSession" },
      on: {
        [onSaveSessionDone]: {
          target: "sCloseForm"
        }
      }
    },

    sCloseForm: {
      on: {
        "": {
          actions: "closeForm",
          target: `#(machine).${sFormScreenRunning}`
        }
      }
    }
  }
});

const RequestReloadFormDef = () => ({
  initial: "sInit",
  states: {
    sInit: {
      on: {
        "": [
          { cond: "isDirtySession", target: "sQuestion" },
          { target: "sRefreshSession" }
        ]
      }
    },

    sQuestion: {
      invoke: { src: "questionSaveData" },
      on: {
        [onPerformSave]: {
          target: sSaveSession
        },
        [onPerformNoSave]: {
          target: sRefreshSession
        },
        [onPerformCancel]: {
          target: `#(machine).${sFormScreenRunning}`
        }
      }
    },

    [sSaveSession]: {
      invoke: { src: "saveSession" },
      on: {
        [onSaveSessionDone]: sRefreshSession
      }
    },

    [sRefreshSession]: {
      invoke: { src: "refreshSession" },
      on: {
        [onRefreshSessionDone]: `#(machine).${sFormScreenRunning}`
      }
    }
  }
});


