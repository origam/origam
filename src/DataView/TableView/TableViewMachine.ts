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
import { ITableViewMachine } from "./types/ITableViewMachine";
import { IEditing } from "../types/IEditing";
import { ISelection } from "../Selection";

export class TableViewMachine implements ITableViewMachine {
  constructor(
    public P: {
      dataTable: IDataTable;
      recCursor: IRecCursor;
      propCursor: IPropCursor;
      editing: IEditing;
      selection: ISelection;
      dispatch(action: any): void;
      listen(cb: (action: any) => void): void;
    }
  ) {
    this.interpreter = interpret(this.machine);
    this.interpreter.onTransition(
      action((state: State<any>, event: any) => {
        this.state = state;
        console.log("TableViewMachine:", state, event);
      })
    );
    this.state = this.interpreter.state;
    this.subscribeMediator();
    this.interpreter.start();
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
                ],
                [TableViewActions.ON_CELL_CLICK]: {
                  actions: "onCellClick"
                },
                [TableViewActions.ON_NO_CELL_CLICK]: {
                  actions: "onNoCellClick"
                },
                [TableViewActions.ON_OUTSIDE_TABLE_CLICK]: {
                  actions: "onOutsideTableClick"
                }
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
        runningEx: (ctx, event) => this.runningEx(),

        onCellClick: (ctx, event) =>
          this.onCellClick(event as TableViewActions.IOnCellClick),
        onNoCellClick: (ctx, event) => this.onNoCellClick(),
        onOutsideTableClick: (ctx, event) => this.onOutsideTableClick()
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
  send(event: any): void {
    this.interpreter.send(event);
  }

  @action.bound
  runningEn() {
    console.log("---RUNNINGED");
    if (!this.isCellSelected) {
      this.dispatch(TableViewActions.selectFirstCell());
    }
    this.dispatch(TableViewActions.makeSelectedCellVisible());
  }

  @action.bound
  runningEx() {
    this.dispatch(DataViewActions.finishEditing());
  }

  @action.bound onCellClick(event: TableViewActions.IOnCellClick) {
    console.log("on cell click from machine");
    // TODO: Bool field behaviour.
    // debugger
    if (!this.P.editing.isEditing) {
      if (
        this.P.selection.isSelectedCellByIdx({
          rowIdx: event.rowIdx,
          colIdx: event.columnIdx
        })
      ) {
        this.dispatch(DataViewActions.startEditing());
      } else {
        this.dispatch(
          DataViewActions.selectCellByIdx({
            rowIdx: event.rowIdx,
            columnIdx: event.columnIdx
          })
        );
      }
    } else {
      if (
        !this.P.selection.isSelectedCellByIdx({
          rowIdx: event.rowIdx,
          colIdx: event.columnIdx
        })
      ) {
        this.dispatch(DataViewActions.finishEditing());
        this.dispatch(
          DataViewActions.selectCellByIdx({
            rowIdx: event.rowIdx,
            columnIdx: event.columnIdx
          })
        );
        this.dispatch(DataViewActions.startEditing());
      }
    }
  }

  @action.bound onNoCellClick() {
    console.log("on no cell click from machine");
    if (this.P.editing.isEditing) {
      this.P.dispatch(DataViewActions.finishEditing());
    }
  }

  @action.bound onOutsideTableClick() {
    console.log("on outside table click from machine");
    if (this.P.editing.isEditing) {
      this.P.dispatch(DataViewActions.finishEditing());
    }
  }

  @observable shallSleep = false;

  @computed get hasData() {
    return this.P.dataTable.hasContent;
  }

  @computed get isCellSelected() {
    return this.P.propCursor.isSelected && this.P.recCursor.isSelected;
  }

  @computed get isActive(): boolean {
    return this.state && this.state.matches("active");
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
