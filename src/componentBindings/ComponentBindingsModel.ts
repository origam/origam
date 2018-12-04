import { action, computed, autorun } from "mobx";
import { IGridPanelBacking } from "../GridPanel/types";
import { ILoadingGate } from "src/DataLoadingStrategy/types";

export class BindingFilter {
  @computed
  public get filterCondition(): any {
    return;
  }
}

export class DependentGrid implements ILoadingGate {
  
  constructor(
    public gridPaneBacking: IGridPanelBacking,
    
  ) {}

  public parent: DependentGrid | undefined;
  public myParentPropertyId: string | undefined;
  public myChildPropertyId: string | undefined;
  public controllingPropertyValueOld: string | undefined;

  @computed get myGridLoading(): boolean {
    return this.gridPaneBacking.dataLoadingStrategySelectors.isLoading;
  }

  @computed get someParentLoading(): boolean {
    return Boolean(this.parent && this.parent.meOrSomeParentLoading);
  }

  @computed get meOrSomeParentLoading(): boolean {
    return this.myGridLoading || this.someParentLoading;
  }

  @computed get myPropertyValueUseful(): boolean {
    return !this.meOrSomeParentLoading && Boolean(this.myPropertyValue);
  }

  @computed public get isLoadingAllowed(): boolean {
    return this.myPropertyValueUseful;
  }

  @computed
  get myPropertyValue(): string | undefined {
    if(!this.myParentPropertyId) {
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
  get parentPropertyValue() {
    return this.parent && this.parent.myPropertyValue;
  }

  @computed
  get controllingPropertyValueChanged() {
    return this.parentPropertyValue !== this.controllingPropertyValueOld;
  }

  public start() {
    autorun(() => {
      if (this.controllingPropertyValueChanged) {
        console.log(this.parentPropertyValue);
      }
      this.controllingPropertyValueOld = this.parentPropertyValue;
    });
    autorun(() => {
      console.log(this.myGridLoading);
    })
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
    console.log(Array.from(this.gridBacking.entries()));
    console.log("CB START");

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
        gpb!.dataLoadingStrategyActions.addLoadingGate(dependentGrid)
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
    console.log("CB STOP");
  }
}
