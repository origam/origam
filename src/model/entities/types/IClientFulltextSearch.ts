import {IMenuItemIcon} from '../../../gui/Workbench/MainMenu/MainMenu';
import { IMenuSearchResult } from './ISearchResult';

export interface IClientFullTextSearch {
  // foundItems: ISearchResultSection[];
  searchResults: IMenuSearchResult[];
  onSearchFieldChange(searchTerm: string): void;
  clearResults(): void;
  indexMainMenu(mainMenu: any): void;

  subscribeOpenSearchSection(open: () => void): () => void;

  parent?: any;
}


// export interface ISearchResultSection {
//   label: string;
//   itemCount: number;
//   items: ISearchResultItem[];
// }
