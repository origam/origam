export type IListener = (
  action: any,
  sender: any,
) => any;

export type IForwardee = (action: any, sender?: any) => any;

export interface IDataViewMediator {
  dispatch: IForwardee;
  listen(listener: IListener): () => void;
  openScope(): IMediatorScope;
}

export interface IMediatorScope {
  dispatch: (action: any, sender: any) => any;
  listen(listener: IListener): () => void;
  closeScope(): void;
}
