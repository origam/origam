import { action, computed, autorun, trace } from "mobx";
import { IGridPanelBacking } from "../GridPanel/types";
import { ILoadingGate } from "src/DataLoadingStrategy/types";
import { IGridFilter } from "../DataLoadingStrategy/types";
import { DataLoadingStrategyActions } from "../DataLoadingStrategy/DataLoadingStrategyActions";

export class BindingFilter {
  @computed
  public get filterCondition(): any {
    return;
  }
}

export class DependentGrid implements ILoadingGate, IGridFilter {
  constructor(public gridPaneBacking: IGridPanelBacking) {}

  public parent: DependentGrid | undefined;
  public myParentPropertyId: string | undefined;
  public myChildPropertyId: string | undefined;
  public controllingPropertyValueOld: string | undefined;

  @computed
  get myGridLoading(): boolean {
    return this.gridPaneBacking.dataLoadingStrategyActions.isLoading;
  }

  @computed
  get someParentLoading(): boolean {
    return Boolean(this.parent && this.parent.meOrSomeParentLoading);
  }

  @computed
  get meOrSomeParentLoading(): boolean {
    return this.myGridLoading || this.someParentLoading;
  }

  @computed
  public get isLoadingAllowed(): boolean {
    // console.log('Computing loading state')
    return (
      Boolean(!this.parent) ||
      (!this.parent!.meOrSomeParentLoading &&
        Boolean(this.myChildPropertyValue))
    );
  }

  @computed
  get myParentPropertyValue(): string | undefined {
    if (!this.myParentPropertyId) {
      return;
    }
    const { selectedRecord } = this.gridPaneBacking.gridCursorView;
    const { dataTableSelectors } = this.gridPaneBacking;

    const field = dataTableSelectors.getFieldById(this.myParentPropertyId);
    if (selectedRecord && field) {
      const propertyValue = dataTableSelectors.getOriginalValue(
        selectedRecord,
        field
      );
      return propertyValue as string | undefined;
    }

    return undefined;
  }

  @computed
  public get myChildPropertyValue(): string | undefined {
    return this.parent && this.parent.myParentPropertyValue;
  }

  @computed
  get gridFilter(): any {
    if (this.myChildPropertyValue) {
      return [this.myChildPropertyId, "eq", this.myChildPropertyValue];
    } else {
      return [];
    }
  }

  public start() {
    autorun(
      () => {
        // if (this.controllingPropertyValueChanged && this.parentPropertyValueUseful) {
        // console.log(this.parentPropertyValue);
        // this.gridPaneBacking.dataLoadingStrategyActions.requestLoadFresh();
        // }
        // this.controllingPropertyValueOld = this.parentPropertyValue;
      },
      { name: `BoundLoad@${this.gridPaneBacking.modelInstanceId}` }
    );
    console.log('Start for', this.gridPaneBacking.dataStructureEntityId)
    autorun(() => {
      /*console.log(
        "******",
        this.gridPaneBacking.dataStructureEntityId,
        !this.meOrSomeParentLoading,
        this.myChildPropertyValue
      );*/
      if (
        this.parent &&
        !this.meOrSomeParentLoading &&
        !this.myChildPropertyValue
      ) {
        this.gridPaneBacking.dataTableActions.clearAll();
      }
      // console.log(`${this.gridPaneBacking.modelInstanceId}`, this.myChildPropertyValue)
      // console.log(this.gridPaneBacking.modelInstanceId, this.myGridLoading);
    });
    autorun(() => {
      console.log('###', this.gridPaneBacking.dataLoadingStrategyActions.inLoading);
    });
  }
}

export class ComponentBindingsModel {
  constructor(componentBindings: any) {
    console.log(componentBindings);
    this.componentBindings = componentBindings;
  }

  private disposers: Array<() => void> = [];
  public gridBacking: Map<string, IGridPanelBacking> = new Map();
  public dependentGrids: Map<string, DependentGrid> = new Map();
  public componentBindings: any[] = [];

  @action.bound
  public registerGridPaneBacking(gridPaneBacking: IGridPanelBacking) {
    // console.log(gridPaneBacking);
    this.gridBacking.set(gridPaneBacking.modelInstanceId, gridPaneBacking);
  }

  @action.bound
  public start() {
    // console.log(Array.from(this.gridBacking.entries()));
    // console.log("CB START");

    const interrestingIds = new Set();
    for (const binding of this.componentBindings) {
      interrestingIds.add(binding.parentId);
      interrestingIds.add(binding.childId);
    }

    for (const modelId of interrestingIds.values()) {
      const gpb = this.gridBacking.get(modelId);
      const dependentGrid = new DependentGrid(gpb!);
      this.dependentGrids.set(modelId, dependentGrid);
      this.disposers.push(
        gpb!.dataLoadingStrategyActions.addLoadingGate(dependentGrid),
        gpb!.dataLoadingStrategyActions.addBondFilter(dependentGrid)
      );
    }

    for (const binding of this.componentBindings) {
      const parent = this.dependentGrids.get(binding.parentId);
      const child = this.dependentGrids.get(binding.childId);
      child!.parent = parent;
      parent!.myParentPropertyId = binding.parentProperty;
      child!.myChildPropertyId = binding.childProperty;
    }

    for (const modelId of interrestingIds.values()) {
      this.dependentGrids.get(modelId)!.start();
    }
  }

  @action.bound
  public stop() {
    // console.log("CB STOP");
  }
}
