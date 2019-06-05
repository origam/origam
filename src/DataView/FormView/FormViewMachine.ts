import { action, computed, observable } from "mobx";
import { interpret, Machine, State } from "xstate";
import { Interpreter } from "xstate/lib/interpreter";
import { unpack } from "../../utils/objects";
import * as DataViewActions from "../DataViewActions";
import { IDataTable } from "../types/IDataTable";
import { IEditing } from "../types/IEditing";
import { IPropCursor } from "../types/IPropCursor";
import { IRecCursor } from "../types/IRecCursor";
import * as FormViewActions from "./FormViewActions";
import { IFormViewMachine } from "./types";
import { IProperty } from "../types/IProperty";
import { ISelection } from "../Selection";


export class FormViewMachine implements IFormViewMachine {
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
              /* Start in deadPeriod because switching views triggers 
              onOutsideFormClick in the end which would end editing. */
              initial: "deadPeriod",
              // Delegate messages...
              onEntry: "runningEn",
              onExit: "runningEx",
              on: {
                "": [
                  { cond: "notHasData", target: "waitForData" },
                  { cond: "shallSleep", target: "sleeping" }
                ]
              },
              states: {
                /* TODO: Make this parallel so that user interface events 
                have deadPeriod, but others do not. */
                deadPeriod: { after: { 1: "receivingEvents" } },
                receivingEvents: {
                  on: {
                    [FormViewActions.ON_NO_FIELD_CLICK]: {
                      actions: "onNoFieldClick",
                      target: "deadPeriod"
                    },
                    [FormViewActions.ON_OUTSIDE_FORM_CLICK]: {
                      actions: "onOutsideFormClick",
                      target: "deadPeriod"
                    },
                    [FormViewActions.ON_FIELD_CLICK]: {
                      actions: "onFieldClick",
                      target: "deadPeriod"
                    },
                    [FormViewActions.ON_PREV_ROW_CLICK]: {
                      actions: "onPrevRowClick",
                      target: "deadPeriod"
                    },
                    [FormViewActions.ON_NEXT_ROW_CLICK]: {
                      actions: "onNextRowClick",
                      target: "deadPeriod"
                    }
                  }
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

        onNoFieldClick: (ctx, event) => this.onNoFieldClick(),
        onOutsideFormClick: (ctx, event) => this.onOutsideFormClick(),
        onFieldClick: (ctx, event) => this.onFieldClick(event.propId),
        onNextRowClick: () => this.onNextRowClick(),
        onPrevRowClick: () => this.onPrevRowClick()
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
      this.dispatch(FormViewActions.selectFirstCell());
    }
    this.dispatch(DataViewActions.startEditing());
  }

  @action.bound
  runningEx() {
    this.dispatch(DataViewActions.finishEditing());
  }

  @action.bound
  onNoFieldClick() {
    if (this.P.editing.isEditing) {
      this.dispatch(DataViewActions.finishEditing());
    }
  }

  @action.bound
  onOutsideFormClick() {
    if (this.P.editing.isEditing) {
      this.dispatch(DataViewActions.finishEditing());
    }
  }

  @action.bound
  onFieldClick(propId: string) {
    const finishEditing = () =>
      this.P.dispatch(DataViewActions.finishEditing());
    const startEditing = () => this.dispatch(DataViewActions.startEditing());
    const selectCellByPropId = (propId: string) =>
      this.dispatch(
        FormViewActions.selectFieldByPropId({
          propId
        })
      );

    const getValueFromSelectedCell = () =>
      this.dataTable.getValueById(
        this.P.selection.selRowId!,
        this.P.selection.selColId!
      );
    const isCheckBox = (prop: IProperty) =>
      prop ? prop.column === "CheckBox" : false;
    const cellAlreadySelected = (propId: string) =>
      this.P.selection.selColId === propId;

    if (!this.P.editing.isEditing) {
      selectCellByPropId(propId);
      startEditing();
    } else if (!cellAlreadySelected(propId)) {
      finishEditing();
      selectCellByPropId(propId);
      startEditing();
    }
  }

  withRefreshedEditing(fn: () => void) {
    const { isEditing } = this.P.editing;
    if (isEditing) {
      this.dispatch(DataViewActions.finishEditing());
    }
    fn();
    if (isEditing) {
      this.dispatch(DataViewActions.startEditing());
      this.dispatch(DataViewActions.focusEditor());
    }
  }

  @action.bound onNextRowClick() {
    // debugger;
    this.withRefreshedEditing(() =>
      this.dispatch(FormViewActions.selectNextRow())
    );
  }

  @action.bound onPrevRowClick() {
    this.withRefreshedEditing(() =>
      this.dispatch(FormViewActions.selectPrevRow())
    );
  }

  @observable shallSleep = false;

  @computed get hasData() {
    return this.P.dataTable.hasContent;
  }

  @computed get isCellSelected() {
    return this.P.recCursor.isSelected && this.P.propCursor.isSelected;
  }

  @computed get isActive() {
    return this.state && this.state.matches("active");
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
