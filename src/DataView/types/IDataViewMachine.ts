
export interface IDataViewMachine {
  start(): void;
  stop(): void;
  send(event: any): void;
  loadFresh(): void;
  isLoading: boolean;
  isMeOrAnyAscendantLoading: boolean;
  isAnyAscendantLoading: boolean;
  root: IDataViewMachine;
  parent: IDataViewMachine | undefined;
  children: IDataViewMachine[];
  controllingValueForChildren: string | undefined;
  controlledFieldId: string;
  controllingFieldId: string;
  treeDispatch(message: any): void;
  descendantsDispatch(message: any): void;
  addChild(child: IDataViewMachine): void;
  setParent(parent: IDataViewMachine): void;
}