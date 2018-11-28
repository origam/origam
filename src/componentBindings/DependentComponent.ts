import { computed } from "mobx";
import { IRecordId } from '../DataTable/types';

interface IInvolvedView {
  selectedRowId: IRecordId;
  isLoading: boolean;
  setLoadingActive(state: boolean): void;
}

export class Bond {

}

export class DependentComponent {
  public dependencies: DependentComponent[] = [];

  @computed get hasStableSelectedId(): boolean {
    throw new Error("Not yet implemented.")
  }

  @computed get generatedFilter(): any {
    throw new Error("Not yet implemented.")
  }
}