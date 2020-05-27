import Axios from "axios";
import {action, comparer, computed, flow, observable, reaction} from "mobx";
import {navigateAsChild} from "model/actions/DataView/navigateAsChild";
import {handleError} from "model/actions/handleError";
import {getBindingChildren} from "model/selectors/DataView/getBindingChildren";
import {getDataView} from "model/selectors/DataView/getDataView";
import {getDataViewLabel} from "model/selectors/DataView/getDataViewLabel";
import {getEntity} from "model/selectors/DataView/getEntity";
import {getIsBindingParent} from "model/selectors/DataView/getIsBindingParent";
import {getIsBindingRoot} from "model/selectors/DataView/getIsBindingRoot";
import {getMasterRowId} from "model/selectors/DataView/getMasterRowId";
import {getParentRowId} from "model/selectors/DataView/getParentRowId";
import {getDontRequestData} from "model/selectors/getDontRequestData";
import {getSessionId} from "model/selectors/getSessionId";
import {getApi} from "../../selectors/getApi";
import {getSelectedRowId} from "../../selectors/TablePanelView/getSelectedRowId";
import {IDataViewLifecycle} from "./types/IDataViewLifecycle";
import {processCRUDResult} from "model/actions/DataLoading/processCRUDResult";
import {IDataView} from "../types/IDataView";
import {getMenuItemId} from "../../selectors/getMenuItemId";
import {getDataStructureEntityId} from "../../selectors/DataView/getDataStructureEntityId";
import {SCROLL_DATA_INCREMENT_SIZE} from "../../../gui/Workbench/ScreenArea/TableView/InfiniteScrollLoader";
import {getColumnNamesToLoad} from "../../selectors/DataView/getColumnNamesToLoad";
import {joinWithAND, toFilterItem} from "../OrigamApiHelpers";

export class DataViewLifecycle implements IDataViewLifecycle {
  $type_IDataViewLifecycle: 1 = 1;

  @observable _inFlow = 0;

  set inFlow(value: number) {
    // console.log('Setting inFlow to', value)
    this._inFlow = value;
  }

  get inFlow() {
    return this._inFlow;
  }

  @computed get isWorking() {
    return this.inFlow > 0;
  }
  disposers: any[] = [];

  @action.bound
  start(): void {
    if (getDontRequestData(this)) {
      this.disposers.push(this.startSelectedRowReaction());
    }
  }

  onSelectedRowIdChangeImm = flow(
    function*(this: DataViewLifecycle) {
      try {
        this.inFlow++;
        console.log(getDataViewLabel(this), "detected control id change", getSelectedRowId(this));
        if (getSelectedRowId(this)) {
          if (getIsBindingRoot(this)) {
            yield* this.changeMasterRow();
            yield* this.navigateChildren();
          } else if (getIsBindingParent(this)) {
            yield* this.navigateChildren();
          }
        }
      } catch (e) {
        // TODO: Move this method to action handler file?
        yield* handleError(this)(e);
        throw e;
      } finally {
        this.inFlow--;
      }
    }.bind(this)
  );

  _selectedRowIdChangeDebounceTimeout: any;
  @action.bound
  onSelectedRowIdChange() {
    if (this._selectedRowIdChangeDebounceTimeout) {
      clearTimeout(this._selectedRowIdChangeDebounceTimeout);
    } else {
      this.inFlow++;
    }
    this._selectedRowIdChangeDebounceTimeout = setTimeout(() => {
      this.onSelectedRowIdChangeImm();
      this._selectedRowIdChangeDebounceTimeout = undefined;
      this.inFlow--;
    }, 100);
  }

  _selectedRowReactionDisposer: any;
  @action.bound startSelectedRowReaction(fireImmediately?: boolean) {
    console.log('selrow reaction started')
    const self = this;
    return (this._selectedRowReactionDisposer = reaction(
      () => {
        return getSelectedRowId(this);
      },
      () => self.onSelectedRowIdChange(),
      { equals: comparer.structural, fireImmediately }
    ));
  }

  @action.bound stopSelectedRowReaction() {
    if (this._selectedRowReactionDisposer) {
      this._selectedRowReactionDisposer();
      console.log('selrow reaction stopped')
    }
  }

  *navigateAsChild() {
    yield* this.loadGetData();
  }

  changeMasterRowCanceller: any;

  *changeMasterRow() {
    try {
      this.inFlow++;
      const api = getApi(this);
      this.changeMasterRowCanceller && this.changeMasterRowCanceller();
      this.changeMasterRowCanceller = api.createCanceller();
      const crudResult = yield api.setMasterRecord(
        {
          SessionFormIdentifier: getSessionId(this),
          Entity: getEntity(this),
          RowId: getSelectedRowId(this)!
        },
        this.changeMasterRowCanceller
      );
      yield* processCRUDResult(this, crudResult);
    } catch (error) {
      if (Axios.isCancel(error)) {
        return;
      }
      /*console.error(error);
      yield errDialogPromise(this)(error);*/
      throw error;
    } finally {
      this.inFlow--;
    }
  }

  buildDetailFilter(dataView: IDataView){
    const filterItems =[];
    for (let binding of dataView.parentBindings) {
      const selectedRow = binding.parentDataView.selectedRow;
      if (!selectedRow) {
        continue;
      }
      for (let bindingPair of binding.bindingPairs) {
        const parentDsField = binding.parentDataView.dataSource.getFieldByName(
          bindingPair.parentPropertyId
        );
        if (!parentDsField) {
          continue;
        }
        const parentValue = binding.parentDataView.dataTable.getCellValueByDataSourceField(
          selectedRow,
          parentDsField
        );
        const childDsField = binding.childDataView.dataSource.getFieldByName(
          bindingPair.childPropertyId
        );
        if (!childDsField) {
          continue;
        }
        filterItems.push(toFilterItem(childDsField.name, parentValue));
      }
    }
    return joinWithAND(filterItems);
  }


  *loadGetData() {
    try {
      this.inFlow++;
      const dataView = getDataView(this);
      const api = getApi(this);
      let data;
      if(dataView.isRootEntity && !dataView.isRootGrid && getDontRequestData(this)){
        data = yield api.getRows({
          MenuId: getMenuItemId(dataView),
          SessionFormIdentifier: getSessionId(this),
          DataStructureEntityId: getDataStructureEntityId(dataView),
          Filter: this.buildDetailFilter(dataView),
          Ordering: [],
          RowLimit: SCROLL_DATA_INCREMENT_SIZE,
          RowOffset: 0,
          ColumnNames: getColumnNamesToLoad(dataView),
          MasterRowId: undefined,
        });
      }else{
        data = yield api.getData({
          SessionFormIdentifier: getSessionId(this),
          ChildEntity: getEntity(this),
          ParentRecordId: getParentRowId(this)!,
          RootRecordId: getMasterRowId(this)!
        });
      }
      dataView.dataTable.clear();
      dataView.dataTable.setRecords(data);
      dataView.selectFirstRow();
    } finally {
      this.inFlow--;
    }
  }

  *navigateChildren(): Generator<any, any> {
    try {
      this.inFlow++;
      yield Promise.all(
        getBindingChildren(this).map(bch =>
          flow(function*() {
            try {
              yield* navigateAsChild(bch)();
            } catch (e) {
              yield* handleError(bch)(e);
            }
          })()
        )
      );
      /*for (let bch of getBindingChildren(this)) {
        yield navigateAsChild(bch)();
      }*/
    } finally {
      this.inFlow--;
    }
  }

  parent?: any;
}
