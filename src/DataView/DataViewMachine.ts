import { Machine, State, interpret } from "xstate";
import { Interpreter } from "xstate/lib/interpreter";
import { observable, action, computed } from "mobx";
import { IDataViewMachine } from "./types/IDataViewMachine";
import { IApi } from "../Api/IApi";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { IDataTable } from "./types/IDataTable";
import { IDataSource } from "../Screens/types";
import { IASelCell } from "./types/IASelCell";
import { IDataViewMediator } from "./types/IDataViewMediator";
import * as DataViewActions from './DataViewActions';

export class DataViewMachine implements IDataViewMachine {
  constructor(
    public P: {
      api: ML<IApi>;
      menuItemId: ML<string>;
      dataStructureEntityId: ML<string>;
      propertyIdsToLoad: ML<string[]>;
      dataTable: ML<IDataTable>;
      dataSource: ML<IDataSource>;
      mediator: IDataViewMediator;
    }
  ) {
    this.mediator = P.mediator;
    this.interpreter = interpret(this.definition);
    this.interpreter.onTransition(
      action((state: State<any>) => {
        this.state = state;
        console.log(state);
      })
    );
    this.state = this.interpreter.state;
  }

  mediator: IDataViewMediator;

  definition = Machine(
    {
      initial: "LOAD_FRESH",
      states: {
        IDLE: {
          on: {
            LOAD_FRESH: "LOAD_FRESH",
            LOAD_INCREMENT: "LOAD_INCREMENT"
          }
        },
        LOAD_FRESH: {
          on: {
            LOAD_FRESH: "LOAD_FRESH",
            DONE: "IDLE"
          },
          invoke: { src: "loadFresh" }
        },
        LOAD_INCREMENT: {
          on: {
            LOAD_FRESH: "LOAD_FRESH",
            LOAD_INCREMENT: "LOAD_INCREMENT",
            DONE: "IDLE"
          }
        }
      }
    },
    {
      services: {
        loadFresh: (ctx, event) => (send, onEvent) => {
          this.api
            .getEntities({
              MenuId: this.menuItemId,
              DataStructureEntityId: this.dataStructureEntityId,
              Ordering: [],
              ColumnNames: this.propertyIdsToLoad,
              Filter: ""
            })
            .then(
              action((entities: any) => {
                console.log("ENTITIES", entities);
                this.dataTable.resetDirty();
                this.dataTable.setRecords(entities);
                this.mediator.dispatch(DataViewActions.selectFirstCell());
                send("DONE");
              })
            );
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

  @action.bound loadFresh() {
    this.interpreter.send("LOAD_FRESH");
  }

  get api() {
    return unpack(this.P.api);
  }

  get menuItemId() {
    return unpack(this.P.menuItemId);
  }

  get dataStructureEntityId() {
    return unpack(this.P.dataStructureEntityId);
  }

  get propertyIdsToLoad() {
    return unpack(this.P.propertyIdsToLoad);
  }

  get dataTable() {
    return unpack(this.P.dataTable);
  }

  get dataSource() {
    return unpack(this.P.dataSource);
  }

}
