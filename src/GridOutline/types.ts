export interface IGridOutlineSelectors {
  outlineItems: string[]
  lastSelectedItem: string | undefined;
}

export interface IGridOutlineActions {
  setOutlineItems(items: string[]): void;
  setLastSelectedItem(item: string | undefined): void;
}

export interface IGridOutlineState {
  outlineItems: string[];
  lastSelectedItem: string | undefined;

  setOutlineItems(items: string[]): void;
  setLastSelectedItem(item: string | undefined): void;
}
