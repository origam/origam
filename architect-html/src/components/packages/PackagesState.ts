import { observable } from "mobx";
import {
  IArchitectApi,
  IPackage,
  IPackagesInfo
} from "src/API/IArchitectApi.ts";
import { ProgressBarState } from "src/components/topBar/ProgressBarState.ts";
import { TabViewState } from "src/components/tabView/TabViewState.ts";
import { ModelTreeState } from "src/components/modelTree/ModelTreeState.ts";

export class PackagesState {
  @observable.shallow accessor packages: IPackage[] = [];
  @observable accessor activePackageId: string | undefined;

  constructor(
    private progressBarState: ProgressBarState,
    private sideBarTabViewState: TabViewState,
    private modelTreeState: ModelTreeState,
    private architectApi: IArchitectApi) {

  }

  * loadPackages(): Generator<Promise<IPackagesInfo>, void, IPackagesInfo> {
    const packagesInfo = yield this.architectApi.getPackages();
    this.packages = packagesInfo.packages ?? [];
    if (packagesInfo.activePackageId) {
      yield* this.setActivePackage(packagesInfo.activePackageId)() as any;
    }
  }

  setActivePackage(packageId: string) {
    return function* (this: PackagesState) {
      this.progressBarState.isWorking = true;
      try {
        yield this.architectApi.setActivePackage(packageId);
        this.activePackageId = packageId;
        yield* this.modelTreeState.loadPackageNodes();
        this.sideBarTabViewState.showModelTree();
      } finally {
        this.progressBarState.isWorking = false;
      }
    }.bind(this);
  }
}