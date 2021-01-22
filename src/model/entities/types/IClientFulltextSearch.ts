import { IMenuSearchResult, ISearchResult } from './ISearchResult';
import { ISearchResultGroup } from './ISearchResultGroup';

export interface ISearcher {
  resultGroups: ISearchResultGroup[];
  searchOnServer(): void;
  onSearchFieldChange(searchTerm: string): void;
  indexMainMenu(mainMenu: any): void;

  parent?: any;
}