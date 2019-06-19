import { action, computed, observable, reaction, runInAction } from "mobx";
import { interpret, Machine, State } from "xstate";
import { Interpreter } from "xstate/lib/interpreter";
import { IApi } from "../Api/IApi";
import { IDataSource } from "../Screens/types";
import { stateVariableChanged } from "../utils/mediator";
import { unpack } from "../utils/objects";
import { ML } from "../utils/types";
import { IDataTable } from "./types/IDataTable";
import { IDataViewMachine } from "./types/IDataViewMachine";
import { IRecord } from "./types/IRecord";
import * as DataViewActions from "./DataViewActions";


export class DataViewMachine implements IDataViewMachine {
  constructor(
    public P: {
      api: ML<IApi>;
      menuItemId: ML<string>;
      dataStructureEntityId: ML<string>;
      propertyIdsToLoad: ML<string[]>;
      dataTable: ML<IDataTable>;
      dataSource: ML<IDataSource>;
      isSessionedScreen: boolean;
      sessionId: string;
      selectedIdGetter: () => string | undefined;

      listen(cb: (action: any) => void): void;
      dispatch(action: any): void;
    }
  ) {
    this.subscribeMediator();
    this.interpreter = interpret(this.definition);
    this.interpreter.onTransition(
      action((state: State<any>, event: any) => {
        this.state = state;
        console.log("DataViewMachine:", state, event);
      })
    );
    this.state = this.interpreter.state;
  }

  subscribeMediator() {
    this.P.listen((event: any) => {
      this.interpreter.send(event);
    });
  }

  parent: IDataViewMachine | undefined;
  children: IDataViewMachine[] = [];

  controlledFieldId: string = "";
  controllingFieldId: string = "";

  definition = Machine(
    {
      initial: "IDLE",
      states: {
        IDLE: {
          on: {
            [DataViewActions.LOAD_FRESH]: {
              target: "LOAD_FRESH",
              cond: "loadingAllowed"
            },
            SESSION_MASTER_CHANGED: "SESSION_CHANGE_MASTER",
            [DataViewActions.LOAD_INCREMENT]: "LOAD_INCREMENT",
            [DataViewActions.REQUEST_SAVE_DATA]: "SAVE_DIRTY_DATA",
            [DataViewActions.REQUEST_CREATE_ROW]: "CREATE_NEW_RECORD"
          }
        },
        LOAD_FRESH: {
          on: {
            [DataViewActions.LOAD_FRESH]: [
              { target: "LOAD_FRESH", cond: "loadingAllowed" },
              { target: "IDLE" }
            ],
            DONE: "IDLE"
          },
          invoke: { src: "loadFresh" }
        },
        SESSION_CHANGE_MASTER: {
          on: {
            DONE: { target: "IDLE", actions: "descendantsLoadFresh" }
          },
          invoke: { src: "sessionChangeMaster" }
        },
        LOAD_INCREMENT: {
          on: {
            [DataViewActions.LOAD_FRESH]: "LOAD_FRESH",
            [DataViewActions.LOAD_INCREMENT]: "LOAD_INCREMENT",
            DONE: "IDLE"
          }
        },
        SAVE_DIRTY_DATA: {
          on: {
            DONE: "IDLE"
          },
          invoke: { src: "saveDirtyData" }
        },
        CREATE_NEW_RECORD: {
          on: {
            DONE: "IDLE"
          },
          invoke: { src: "createNewRecord" }
        }
      }
    },
    {
      services: {
        loadFresh: (ctx, event) => (send, onEvent) => {
          if (this.P.isSessionedScreen && !this.isRoot) {
            console.log(
              "DataView machine will load data from session:",
              this.P.sessionId
            );
            this.api
              .getSessionEntity({
                sessionFormIdentifier: this.P.sessionId,
                rootRecordId: this.rootMasterId!,
                childEntity: this.dataSource.id,
                parentRecordId: this.controlledValueFromParent!
              })
              .then(
                action((entities: any) => {
                  const records = [];
                  for (let row of entities) {
                    records.push(
                      this.dataSource.reorderedRow(this.propertyIdsToLoad, row)
                    );
                  }
                  this.dataTable.resetDirty();
                  this.dataTable.setRecords(records);
                  send("DONE");
                })
              );
          } else {
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
                  send("DONE");
                  this.descendantsDispatch(DataViewActions.loadFresh());
                })
              );
          }
        },
        sessionChangeMaster: (ctx, event) => (send, onEvent) => {
          this.api
            .sessionChangeMasterRecord({
              SessionFormIdentifier: this.P.sessionId,
              Entity: this.dataSource.id,
              // Entity: this.dataStructureEntityId!,
              RowId: this.controllingValueForChildren
            })
            .then(
              action(() => {
                send("DONE");
              })
            );
        },
        saveDirtyData: (ctx, event) => async (send, onEvent) => {
          if (this.P.isSessionedScreen && !this.isRoot) {
            this.saveDirtyDataSessioned();
          } else {
            this.saveDirtyDataNotSessioned();
          }
        },
        createNewRecord: (ctx, event) => async (send, onEvent) => {
          console.log("Create new record.");
          send("DONE");
        }
      },
      actions: {
        descendantsLoadFresh: (ctx, event) =>
          this.descendantsDispatch(DataViewActions.loadFresh())
      },
      guards: {
        loadingAllowed: () => this.isLoadingAllowed
      }
    }
  );

  async saveDirtyDataNotSessioned() {
    console.log("Dirty changed:", JSON.stringify(this.dataTable.dirtyValues));
    console.log(
      "Dirty deleted:",
      JSON.stringify(this.dataTable.dirtyDeletedIds)
    );
    for (let RowId of this.dataTable.dirtyDeletedIds.keys()) {
      console.log("Deleting", RowId);
      const result = await this.api.deleteEntity({
        MenuId: this.menuItemId,
        DataStructureEntityId: this.dataStructureEntityId,
        RowIdToDelete: RowId
      });
      console.log("...Deleted.");
      runInAction(() => {
        this.dataTable.removeDirtyDeleted(RowId);
        this.dataTable.removeRow(RowId);
      });
    }
    for (let [RowId, values] of this.dataTable.dirtyValues) {
      console.log("Updating", RowId);
      const result = await this.api.putEntity({
        MenuId: this.menuItemId,
        DataStructureEntityId: this.dataStructureEntityId,
        RowId,
        NewValues: values
      });
      console.log("...Updated.");
      runInAction(() => {
        const newRecord = Array(this.dataTable.properties.count) as IRecord;
        for (let prop of this.dataTable.properties.items) {
          newRecord[prop.dataIndex] =
            result.wrappedObject[prop.dataSourceIndex];
        }
        this.dataTable.substRecord(RowId, newRecord);
        this.dataTable.removeDirtyRow(RowId);
      });
    }
    this.send("DONE");
  }

  async saveDirtyDataSessioned() {
    for (let [RowId, values] of this.dataTable.dirtyValues) {
      console.log("Updating", RowId);
      for (let [prop, val] of values.entries()) {
        console.log(`    column ${prop}`);
        const result = await this.api.updateSessionEntity({
          SessionFormIdentifier: this.P.sessionId,
          Entity: this.dataSource.id,
          Id: RowId,
          Property: prop,
          NewValue: val
        });
        console.log("...Updated.");
        runInAction(() => {
          const newRecord = Array(this.dataTable.properties.count) as IRecord;
          for (let prop of this.dataTable.properties.items) {
            newRecord[prop.dataIndex] =
              result.wrappedObject[prop.dataSourceIndex];
          }
          this.dataTable.substRecord(RowId, newRecord);
          this.dataTable.removeDirtyRow(RowId);
        });
      }
    }
    this.send("DONE");
  }

  interpreter: Interpreter<any, any, any>;
  @observable state: State<any, any>;

  @computed get stateValue(): any {
    return this.state.value;
  }

  disposers: Array<() => void> = [];

  @action.bound
  send(event: any): void {
    this.interpreter.send(event);
  }

  @action.bound start() {
    let oldControllingValueForChildren = this.controllingValueForChildren;
    this.disposers.push(
      reaction(
        () => this.controllingValueForChildren,
        () => {
          console.log(" *** Controlling value for children changed");
          if (this.isRoot && this.P.isSessionedScreen) {
            this.send("SESSION_MASTER_CHANGED");
          } else {
            this.descendantsDispatch(DataViewActions.loadFresh());
          }

          oldControllingValueForChildren = this.controllingValueForChildren;
        }
      ),
      reaction(
        () => [this.isLoadingAllowed],
        () => this.P.dispatch(stateVariableChanged())
      )
    );
    this.interpreter.start();
    if (this.isRoot) {
      this.interpreter.send(DataViewActions.loadFresh());
    }
  }

  @action.bound stop() {
    this.disposers.forEach(d => d());
    this.interpreter.stop();
  }

  @action.bound loadFresh() {
    // this.interpreter.send("LOAD_FRESH");
    this.treeDispatch(DataViewActions.loadFresh());
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

  @computed get isLoadingAllowed() {
    if (this.isRoot) {
      return true;
    } else {
      return !this.isAnyAscendantReadingData && this.isControlIdValid;
    }
  }

  @computed
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

  get rootMasterId() {
    return this.root && this.root.masterId;
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

  @computed
  get isLoading(): boolean {
    switch (this.stateValue) {
      case "LOAD_FRESH":
      case "LOAD_INCREMENT":
      case "SAVE_DIRTY_DATA":
      case "CREATE_NEW_RECORD":
        return true;
      default:
        return false;
    }
  }

  @computed
  get isReadingData(): boolean {
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

  get isAnyAscendantReadingData(): boolean {
    return this.parent ? this.parent.isMeOrAnyAscendantReadingData : false;
  }

  get isMeOrAnyAscendantReadingData(): boolean {
    return this.isReadingData || this.isAnyAscendantReadingData;
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
