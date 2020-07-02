import {IMenuItemIcon} from '../../../gui/Workbench/MainMenu/MainMenu';

export interface IClientFulltextSearch {
  foundItems: ISearchResultSection[];
  onSearchFieldChange(event: any): void;
  clearResults(): void;
  indexMainMenu(mainMenu: any): void;

  subscribeOpenSearchSection(open: () => void): () => void;

  parent?: any;
}


export interface ISearchResultSection {
  label: string;
  itemCount: number;
  items: ISearchResultItem[];
}

export interface ISearchResultItem {
  id: string;
  type: string;
  icon: IMenuItemIcon;
  label: string;
  description: string;
  node: any;
}