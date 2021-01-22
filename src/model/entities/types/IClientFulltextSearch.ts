import { IMenuSearchResult } from './ISearchResult';

export interface IClientFullTextSearch {
  searchResults: IMenuSearchResult[];
  onSearchFieldChange(searchTerm: string): void;
  clearResults(): void;
  indexMainMenu(mainMenu: any): void;
  subscribeToResultsChange(subscriber: (searchResults: IMenuSearchResult[])=> void): ()=> void;

  parent?: any;
}