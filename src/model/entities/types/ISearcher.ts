import { ISearchResultGroup } from './ISearchResultGroup';

export interface ISearcher {
  resultGroups: ISearchResultGroup[];
  searchOnServer(): void;
  onSearchFieldChange(searchTerm: string): void;
  indexMainMenu(mainMenu: any): void;
  clear(): void;

  parent?: any;
}