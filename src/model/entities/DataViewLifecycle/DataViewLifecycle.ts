import Axios from "axios";
import _ from "lodash";
import { action, computed, flow, observable, reaction } from "mobx";
import { navigateAsChild } from "model/actions/DataView/navigateAsChild";
import { handleError } from "model/actions/handleError";
import { getBindingChildren } from "model/selectors/DataView/getBindingChildren";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getDataViewLabel } from "model/selectors/DataView/getDataViewLabel";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getIsBindingParent } from "model/selectors/DataView/getIsBindingParent";
import { getIsBindingRoot } from "model/selectors/DataView/getIsBindingRoot";
import { getMasterRowId } from "model/selectors/DataView/getMasterRowId";
import { getParentRowId } from "model/selectors/DataView/getParentRowId";
import { getDontRequestData } from "model/selectors/getDontRequestData";
import { getSessionId } from "model/selectors/getSessionId";
import { getApi } from "../../selectors/getApi";
import { getSelectedRowId } from "../../selectors/TablePanelView/getSelectedRowId";
import { IDataViewLifecycle } from "./types/IDataViewLifecycle";
import { processCRUDResult } from "model/actions/DataLoading/processCRUDResult";

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

  @action.bound startSelectedRowReaction() {
    const self = this;
    return reaction(
      () => {
        return getSelectedRowId(this);
      },
      () => self.onSelectedRowIdChange()
    );
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

  *loadGetData() {
    try {
      this.inFlow++;
      const api = getApi(this);
      const data = yield api.getData({
        SessionFormIdentifier: getSessionId(this),
        ChildEntity: getEntity(this),
        ParentRecordId: getParentRowId(this)!,
        RootRecordId: getMasterRowId(this)!
      });
      const dataView = getDataView(this);
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
