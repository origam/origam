import {observable, action} from 'mobx';
import { IDataLoadingStategyState, ILoadingGate, IGridFilter } from './types';


export class DataLoadingStrategyState implements IDataLoadingStategyState {
  
  @observable public headLoadingActive = false;
  @observable public tailLoadingActive = false;
  @observable public loadingActive = true;
  
  public idGen = 1;
  @observable public loadingGates = new Map();
  @observable public bondFilters = new Map();

  @action.bound public setHeadLoadingActive(state: boolean) {
    this.headLoadingActive = state;
  }

  @action.bound public setTailLoadingActive(state: boolean) {
    this.tailLoadingActive = state;
  }

  @action.bound public setLoadingActive(state: boolean) {
    this.loadingActive = state;
  }

  @action.bound public addLoadingGate(gate: ILoadingGate) {
    const myId = this.idGen++;
    this.loadingGates.set(myId, gate);
    return () => {
      this.loadingGates.delete(myId);
    }
  }

  @action.bound public addBondFilter(filter: IGridFilter) {
    const myId = this.idGen++;
    this.bondFilters.set(myId, filter);
    return () => {
      this.bondFilters.delete(myId);
    }
  }
}