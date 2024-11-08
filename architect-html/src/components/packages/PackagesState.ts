import { observable } from "mobx";
import { IArchitectApi, IPackage } from "src/API/IArchitectApi.ts";

export class PackagesState {
  @observable.shallow accessor packages: IPackage[] = [];
  @observable accessor activePackageId: string | undefined;

  constructor(private architectApi: IArchitectApi) {

  }

  * loadPackages(): Generator<Promise<IPackage[]>, void, IPackage[]> {
    this.packages = yield this.architectApi.getPackages();
  }

  * setActivePackage(packageId: string) {
    yield this.architectApi.setActivePackage(packageId);
    this.activePackageId = packageId;
  }
}