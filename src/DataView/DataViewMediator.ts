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

  openScope(): IMediatorScope {
    return new MediatorScope(this);
  }

  listeners: Map<number, IListener> = new Map();
  idGen = 0;

  listen(listener: IListener): () => void {
    let myId = this.idGen++;
    this.listeners.set(myId, listener);
    return () => this.listeners.delete(myId);
  }
}

const STOP_DISPATCH = Symbol("STOP_DISPATCH");

export function stopDispatch() {
  throw STOP_DISPATCH;
}

class MediatorScope implements IMediatorScope {
  constructor(mediator: IDataViewMediator) {
    this.disposer = mediator.listen(this.dispatch);
  }

  disposer: () => void;

  listeners: Map<number, IListener> = new Map();
  idGen = 0;

  @action.bound dispatch(act: any, sender: any) {
    for (let listener of this.listeners.values()) {
      listener(act, sender);
    }
  }

  listen(listener: IListener): () => void {
    let myId = this.idGen++;
    this.listeners.set(myId, listener);
    return () => this.listeners.delete(myId);
  }

  closeScope(): void {
    this.disposer();
  }
}
