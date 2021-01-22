import { IMenuSearchResult } from './ISearchResult';

export interface IClientFullTextSearch {
  onSearchFieldChange(searchTerm: string): void;
  indexMainMenu(mainMenu: any): void;
  subscribeToResultsChange(subscriber: (searchResults: IMenuSearchResult[])=> void): ()=> void;

  parent?: any;
}