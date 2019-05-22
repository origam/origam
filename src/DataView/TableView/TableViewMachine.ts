import { IDataTable } from "../types/IDataTable";
import { IRecCursor } from "../types/IRecCursor";
import { interpret, State, Machine } from "xstate";
import { action, observable, computed } from "mobx";
import { isType } from "ts-action";
import { Interpreter } from "xstate/lib/interpreter";
import { unpack } from "../../utils/objects";
import * as DataViewActions from "../DataViewActions";
import * as TableViewActions from "./TableViewActions";
import { IPropCursor } from "../types/IPropCursor";

export class TableViewMachine {
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
      action((state: State<any>) => {
        this.state = state;
        console.log("TableViewMachine:", state);
      })
    );
    this.state = this.interpreter.state;
    this.subscribeMediator();
  }

  subscribeMediator() {}

  machine = Machine(
    {
      initial: "inactive",
      states: {
        inactive: {
          on: {
            [DataViewActions.ACTIVATE_VIEW]: {
              cond: "isTableView",
              target: "active"
            }
          }
        },
        active: {
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
        isTableView: (ctx, event) => event.viewType === "Table"
      }
    }
  );

  @action.bound
  runningEn() {
    if (!this.isCellSelected) {
      this.dispatch(DataViewActions.selectFirstCell());
    }
    this.dispatch(TableViewActions.makeSelectedCellVisible());
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
    return this.P.propCursor.isSelected && this.P.recCursor.isSelected;
  }

  @action.bound dispatch(event: any) {
    this.P.dispatch(event);
  }

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
