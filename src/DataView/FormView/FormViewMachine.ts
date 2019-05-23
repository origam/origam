import { interpret, Machine, State } from "xstate";
import { Interpreter } from "xstate/lib/interpreter";
import { action, observable, computed } from "mobx";
import { IDataViewMediator } from "../types/IDataViewMediator";
import { unpack } from "../../utils/objects";
import { ML } from "../../utils/types";
import { isType } from "ts-action";
import * as DataViewActions from "../DataViewActions";
import * as FormViewActions from "./FormViewActions";
import { IDataTable } from "../types/IDataTable";
import { IRecCursor } from "../types/IRecCursor";
import { IFormViewMachine } from "./types";
import { IPropCursor } from "../types/IPropCursor";

export class FormViewMachine implements IFormViewMachine {
  constructor(
    public P: {
      dataTable: IDataTable;
      recCursor: IRecCursor;
      propCursor: IPropCursor;
      dispatch(action: any): void;
      listen(cb: (action: any) => void): void;
    }
  ) {
    this.interpreter = interpret(this.machine);
    this.interpreter.onTransition(
      action((state: State<any>, event: any) => {
        this.state = state;
        console.log("FormViewMachine:", state, event);
      })
    );
    this.state = this.interpreter.state;
    this.subscribeMediator();
    // this.interpreter.start();
  }

  subscribeMediator() {
    this.P.listen((event: any) => {
      /*if (isType(action, DataViewActions.dataTableLoaded)) {
        this.interpreter.send("DATA_TABLE_LOADED");
      }*/
      // this.interpreter.send(event);
    });
  }

  machine = Machine(
    {
      initial: "inactive",
      states: {
        inactive: {
          on: {
            [DataViewActions.ACTIVATE_VIEW]: {
              cond: "isFormView",
              target: "active"
            }
          }
        },
        active: {
          initial: "waitForData",
          states: {
            waitForData: {
              on: {
                "": { cond: "hasData", target: "running" }
              }
            },
            running: {
              // Delegate messages...
              onEntry: "runningEn",
              onExit: "runningEx",
              on: {
                "": [
                  { cond: "notHasData", target: "waitForData" },
                  { cond: "shallSleep", target: "sleeping" }
                ]
              }
            },
            sleeping: {
              on: {
                "": {
                  cond: "notShallSleep",
                  target: "running"
                }
              }
            }
          },
          on: {
            [DataViewActions.DEACTIVATE_VIEW]: "inactive"
          }
        }
      }
    },
    {
      actions: {
        runningEn: (ctx, event) => this.runningEn(),
        runningEx: (ctx, event) => this.runningEx()
      },
      guards: {
        shallSleep: (ctx, event) => this.shallSleep,
        notShallSleep: (ctx, event) => !this.shallSleep,
        hasData: (ctx, event) => this.hasData,
        notHasData: (ctx, event) => !this.hasData,
        isFormView: (ctx, event) => event.viewType === "Form"
      }
    }
  );

  @action.bound
  send(event: any): void {
    this.interpreter.send(event);
  }

  @action.bound
  runningEn() {
    if (!this.isCellSelected) {
      this.dispatch(FormViewActions.selectFirstField());
    }
    this.dispatch(DataViewActions.startEditing());
  }

  @action.bound
  runningEx() {
    this.dispatch(DataViewActions.finishEditing());
  }

  @observable shallSleep = false;

  @computed get hasData() {
    return this.P.dataTable.hasContent;
  }

  @computed get isCellSelected() {
    return this.P.recCursor.isSelected && this.P.propCursor.isSelected;
  }

  @computed get isActive() {
    return this.state.matches("active");
  }

  @action.bound dispatch(event: any) {
    this.P.dispatch(event);
  }

  /*definition = Machine(
    {
      initial: "PRE_INIT",
      states: {
        PRE_INIT: {
          on: {
            "": {
              target: "START_EDIT",
              cond: "dataTableHasContent"
            },
            DATA_TABLE_LOADED: {
              target: "START_EDIT",
              cond: "dataTableHasContent"
            }
          }
        },
        START_EDIT: {
          invoke: { src: "startEdit" },
          onEntry: "startEdit",
          on: {
            "": "IDLE"
          }
        },
        IDLE: {
          on: {
            DATA_TABLE_LOADED: {
              target: "START_EDIT",
              cond: "dataTableHasContent"
            }
          }
        }
      }
    },
    {
      actions: {
        startEdit: action(() => {
          console.log("Start edit");
          this.P.dispatch(DataViewActions.selectFirstCell());
          this.P.dispatch(DataViewActions.startEditing());
        })
      },
      services: {},
      guards: {
        dataTableHasContent: () => {
          console.log("FVM cond", this.dataTable.hasContent);
          return this.dataTable.hasContent;
        }
      }
    }
  );*/

  interpreter: Interpreter<any, any, any>;
  @observable state: State<any, any>;

  @computed get stateValue(): any {
    return this.state.value;
  }

  @action.bound start() {
    this.interpreter.start();
  }

  @action.bound stop() {
    this.interpreter.stop();
  }

  get dataTable() {
    return unpack(this.P.dataTable);
  }

  get recCursor() {
    return unpack(this.P.recCursor);
  }
}
