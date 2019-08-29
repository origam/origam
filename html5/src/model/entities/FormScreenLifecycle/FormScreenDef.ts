import {
  sQuestionSaveData,
  onPerformSave,
  onPerformNoSave,
  onPerformCancel
} from "./constants";
import {
  sExecuteAction,
  onExecuteActionDone,
  onExecuteAction,
  onRequestScreenClose
} from "./constants";
import {
  onSaveSession,
  sSaveSession,
  sRefreshSession,
  onRefreshSession,
  onSaveSessionDone,
  onRefreshSessionDone
} from "./constants";
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
        },
        [onSaveSession]: {
          target: sSaveSession
        },
        [onRefreshSession]: {
          target: sRefreshSession
        },
        [onExecuteAction]: {
          target: sExecuteAction
        },
        [onRequestScreenClose]: [
          {
            target: sQuestionSaveData,
            cond: "isDirtySession"
          },
          {
            actions: "closeForm"
          }
        ]
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
        [onExecuteActionDone]: sFormScreenRunning
      }
    },
    [sQuestionSaveData]: {
      invoke: { src: "questionSaveData" },
      on: {
        [onPerformSave]: {
          target: sSaveSession
        },
        [onPerformNoSave]: {
          actions: "closeForm",
          target: sFormScreenRunning
        },
        [onPerformCancel]: {
          target: sFormScreenRunning
        }
      }
    },
  }
};

const sRequestCloseForm = {
  initial: "sDecideDialogDisplay",
  states: {
    sDecideDialogDisplay: {
      "": [
        {
          target: sQuestionSaveData,
          cond: "isDirtySession"
        },
        {
          actions: "closeForm"
        }
      ]
    },
    [sQuestionSaveData]: {
      invoke: { src: "questionSaveData" },
      on: {
        [onPerformSave]: {
          target: sSaveSession
        },
        [onPerformNoSave]: {
          actions: "closeForm",
          target: sFormScreenRunning
        },
        [onPerformCancel]: {
          target: sFormScreenRunning
        }
      }
    },
    [sSaveSession]: {
      invoke: { src: "saveSession" },
      on: {
        [onSaveSessionDone]: {
          actions: "closeForm",
          target: sFormScreenRunning
        }
      }
    }
  }
};
