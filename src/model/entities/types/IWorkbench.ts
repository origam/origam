import { IMainMenuEnvelope } from "./IMainMenu";
import { IWorkbenchLifecycle } from "./IWorkbenchLifecycle";
import { IClientFulltextSearch } from "./IClientFulltextSearch";
import { IOpenedScreens } from "./IOpenedScreens";
import { IWorkQueues } from "./IWorkQueues";
import { IRecordInfo } from "./IRecordInfo";
import { LookupListCacheMulti } from "../../../modules/Lookup/LookupListCacheMulti";

export interface IWorkbenchData {
  mainMenuEnvelope: IMainMenuEnvelope;
  workbenchLifecycle: IWorkbenchLifecycle;
  clientFulltextSearch: IClientFulltextSearch;
  openedScreens: IOpenedScreens;
  openedDialogScreens: IOpenedScreens;
  workQueues: IWorkQueues;
  recordInfo: IRecordInfo;

  lookupListCache: LookupListCacheMulti;
}

export interface IWorkbench extends IWorkbenchData {
  $type_IWorkbench: 1;
  isFullScreen: boolean;

  // loggedUserName: any
  mainMenuEnvelope: IMainMenuEnvelope;
  run(): Generator;
  setFullscreen(state: boolean): void;

  openedScreenIdSet: Set<string>;

  parent?: any;
}

export const isIWorkbench = (o: any): o is IWorkbench => o.$type_IWorkbench;
