
export interface IDataViewMachine {
  start(): void;
  stop(): void;
  send(event: any): void;
  loadFresh(): void;
  isLoading: boolean;
  isMeOrAnyAscendantLoading: boolean;
  isAnyAscendantLoading: boolean;
  isMeOrAnyAscendantReadingData: boolean;
  isAnyAscendantReadingData: boolean;
  root: IDataViewMachine;
  parent: IDataViewMachine | undefined;
  children: IDataViewMachine[];
  controllingValueForChildren: string | undefined;
  controlledFieldId: string;
  controllingFieldId: string;
  masterId: string | undefined;
  treeDispatch(message: any): void;
  descendantsDispatch(message: any): void;
  addChild(child: IDataViewMachine): void;
  setParent(parent: IDataViewMachine): void;
}