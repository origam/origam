import {
  IDataViewMediator,
  IMediatorScope,
  IListener,
  IForwardee
} from "./types/IDataViewMediator";
import { action } from "mobx";

export class DataViewMediator implements IDataViewMediator {
  dispatch(act: any, sender: any): any {
    console.log("[DataView]", act)
    for (let listener of this.listeners.values()) {
      listener(act, sender);
    }
  }


  listeners: Map<number, IListener> = new Map();
  idGen = 0;

  listen(listener: IListener): () => void {
    let myId = this.idGen++;
    this.listeners.set(myId, listener);
    return () => this.listeners.delete(myId);
  }
}


