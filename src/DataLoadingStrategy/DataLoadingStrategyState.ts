import {observable, action} from 'mobx';
import { IDataLoadingStategyState } from './types';

export class DataLoadingStrategyState implements IDataLoadingStategyState {
  @observable public headLoadingActive = false;
  @observable public tailLoadingActive = false;

  @action.bound public setHeadLoadingActive(state: boolean) {
    this.headLoadingActive = state;
  }

  @action.bound public setTailLoadingActive(state: boolean) {
    this.tailLoadingActive = state;
  }
}