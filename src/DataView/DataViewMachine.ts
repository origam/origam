import { Machine, State, interpret } from "xstate";
import { Interpreter } from "xstate/lib/interpreter";
import { observable, action, computed, reaction, runInAction } from "mobx";
import { IDataViewMachine } from "./types/IDataViewMachine";
import { IApi } from "../Api/IApi";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { IDataTable } from "./types/IDataTable";
import { IDataSource } from "../Screens/types";
import { IASelCell } from "./types/IASelCell";
import { IDataViewMediator } from "./types/IDataViewMediator";
import * as DataViewActions from "./DataViewActions";
import { isType } from "ts-action";

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
      selectedIdGetter: () => string | undefined;
    }
  ) {
    this.mediator = P.mediator;
    this.subscribeMediator();
    this.interpreter = interpret(this.definition);
    this.interpreter.onTransition(
      action((state: State<any>) => {
        this.state = state;
        console.log(state);
      })
    );
    this.state = this.interpreter.state;
  }

  subscribeMediator() {
    this.mediator.listen((action: any) => {
      if (isType(action, DataViewActions.requestSaveData)) {
        this.interpreter.send("REQUEST_SAVE_DATA");
      }
    });
  }

  parent: IDataViewMachine | undefined;
  children: IDataViewMachine[] = [];

  controlledFieldId: string = "";
  controllingFieldId: string = "";

  mediator: IDataViewMediator;

  definition = Machine(
    {
      initial: "IDLE",
      states: {
        IDLE: {
          on: {
            LOAD_FRESH: { target: "LOAD_FRESH", cond: "loadingAllowed" },
            LOAD_INCREMENT: "LOAD_INCREMENT",
            REQUEST_SAVE_DATA: "SAVE_DIRTY_DATA"
          }
        },
        LOAD_FRESH: {
          on: {
            LOAD_FRESH: [
              { target: "LOAD_FRESH", cond: "loadingAllowed" },
              { target: "IDLE" }
            ],
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
        },
        SAVE_DIRTY_DATA: {
          on: {
            DONE: "IDLE"
          },
          invoke: { src: "saveDirtyData" }
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
              Filter: this.filterString,
              MasterRowId: this.masterId
            })
            .then(
              action((entities: any) => {
                // console.log("ENTITIES", entities);
                this.dataTable.resetDirty();
                this.dataTable.setRecords(entities);
                this.mediator.dispatch(DataViewActions.selectFirstCell());
                send("DONE");
                this.descendantsDispatch("LOAD_FRESH");
              })
            );
        },
        saveDirtyData: (ctx, event) => async (send, onEvent) => {
          console.log(
            "Dirty changed:",
            JSON.stringify(this.dataTable.dirtyValues)
          );
          for(let [RowId, values] of this.dataTable.dirtyValues) {
            console.log('Saving', RowId);
            const result = await this.api.putEntity({
              MenuId: this.menuItemId,
                DataStructureEntityId: this.dataStructureEntityId,
                RowId,
                NewValues: values
            });
            console.log("...Saved.")
            runInAction(() => {
              this.dataTable.substRecord(RowId, result.itemArray);
              this.dataTable.removeDirtyRow(RowId);
            })
          }
          send("DONE")
        }
      },
      guards: {
        loadingAllowed: () => {
          // console.log("Evaluating loadingAllowed");
          if (this.isRoot) {
            return true;
          } else {
            /*console.log(
              "Loading allowed in child:",
              !this.isAnyAscendantLoading && this.isControlIdValid
            );*/
            return !this.isAnyAscendantLoading && this.isControlIdValid;
          }
        }
      }
    }
  );

  interpreter: Interpreter<any, any, any>;
  @observable state: State<any, any>;

  @computed get stateValue(): any {
    return this.state.value;
  }

  disposers: Array<() => void> = [];

  @action.bound start() {
    this.disposers.push(
      reaction(
        () => this.controlledValueFromParent,
        () => {
          this.treeDispatch("LOAD_FRESH");
        }
      )
    );
    this.interpreter.start();
    if (this.isRoot) {
      this.interpreter.send("LOAD_FRESH");
    }
  }

  @action.bound stop() {
    this.disposers.forEach(d => d());
    this.interpreter.stop();
  }

  @action.bound loadFresh() {
    // this.interpreter.send("LOAD_FRESH");
    this.treeDispatch("LOAD_FRESH");
  }

  @action.bound treeDispatch(message: any) {
    this.interpreter.send(message);
    this.descendantsDispatch(message);
  }

  @action.bound descendantsDispatch(message: any) {
    for (let c of this.children) {
      c.treeDispatch(message);
    }
  }

  @action.bound
  addChild(child: IDataViewMachine): void {
    this.children.push(child);
  }

  @action.bound
  setParent(parent: IDataViewMachine): void {
    this.parent = parent;
  }

  get filterString() {
    if (this.isMasterDetail && !this.isRoot) {
      return JSON.stringify([
        this.controlledFieldId,
        "eq",
        this.controlledValueFromParent
      ]);
    } else {
      return "";
    }
  }

  get controllingValueForChildren() {
    const controllingId = this.P.selectedIdGetter();
    if (controllingId) {
      return this.dataTable.getValueById(
        controllingId,
        this.controllingFieldId
      );
    }
    return;
  }

  get controlledValueFromParent() {
    return this.parent && this.parent.controllingValueForChildren
      ? this.parent.controllingValueForChildren
      : undefined;
  }

  get isControlIdValid() {
    // console.log(this.controlledValueFromParent);
    return this.controlledValueFromParent !== undefined;
  }

  get isMasterDetail(): boolean {
    return this.parent !== undefined || this.children.length > 0;
  }

  get masterId() {
    return this.root.controllingValueForChildren;
  }

  get root(): IDataViewMachine {
    let rootCandidate: IDataViewMachine = this;
    while (rootCandidate.parent) {
      rootCandidate = rootCandidate.parent;
    }
    return rootCandidate;
  }

  get isRoot() {
    return !this.parent;
  }

  get isLoading(): boolean {
    switch (this.stateValue) {
      case "LOAD_FRESH":
      case "LOAD_INCREMENT":
        return true;
      default:
        return false;
    }
  }

  get isAnyAscendantLoading(): boolean {
    return this.parent ? this.parent.isMeOrAnyAscendantLoading : false;
  }

  get isMeOrAnyAscendantLoading(): boolean {
    return this.isLoading || this.isAnyAscendantLoading;
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
