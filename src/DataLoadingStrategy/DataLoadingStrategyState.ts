import {observable, action} from 'mobx';
import { IDataLoadingStategyState } from './types';


export class DataLoadingStrategyState implements IDataLoadingStategyState {
  
  @observable public headLoadingActive = false;
  @observable public tailLoadingActive = false;
  @observable public loadingActive = true;
  @observable public isLoading = false;

  @action.bound public setHeadLoadingActive(state: boolean) {
    this.headLoadingActive = state;
  }

  @action.bound public setTailLoadingActive(state: boolean) {
    this.tailLoadingActive = state;
  }

  @action.bound public setLoadingActive(state: boolean) {
    this.loadingActive = state;
  }

  @action.bound public setLoading(state: boolean) {
    this.isLoading = state;
  }
}