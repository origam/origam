import { interpret, Machine, State } from "xstate";
import { Interpreter } from "xstate/lib/interpreter";
import { action, observable, computed } from "mobx";
import { IDataViewMediator } from "../types/IDataViewMediator";
import { unpack } from "../../utils/objects";
import { ML } from "../../utils/types";
import { isType } from "ts-action";
import * as DataViewActions from "../DataViewActions";
import { IDataTable } from "../types/IDataTable";
import { IRecCursor } from "../types/IRecCursor";
import { IFormViewMachine } from "./types";

export class FormViewMachine implements IFormViewMachine {
  constructor(
    public P: {
      mediator: ML<IDataViewMediator>;
      dataTable: ML<IDataTable>;
      recCursor: ML<IRecCursor>;
    }
  ) {
    this.interpreter = interpret(this.definition);
    this.interpreter.onTransition(
      action((state: State<any>) => {
        this.state = state;
        console.log("FormViewMachine:", state);
      })
    );
    this.state = this.interpreter.state;
    this.subscribeMediator();
  }

  subscribeMediator() {
    this.mediator.listen((action: any) => {
      if (isType(action, DataViewActions.dataTableLoaded)) {
        this.interpreter.send("DATA_TABLE_LOADED");
      }
    });
  }

  definition = Machine(
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
          this.mediator.dispatch(DataViewActions.selectFirstCell());
          this.mediator.dispatch(DataViewActions.startEditing());
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
  );

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

  get mediator() {
    return unpack(this.P.mediator);
  }

  get dataTable() {
    return unpack(this.P.dataTable);
  }

  get recCursor() {
    return unpack(this.P.recCursor);
  }
}
