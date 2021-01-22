import { IMenuSearchResult } from './ISearchResult';

export interface ISearcher {
  onSearchFieldChange(searchTerm: string): void;
  indexMainMenu(mainMenu: any): void;
  subscribeToResultsChange(subscriber: (searchResults: IMenuSearchResult[])=> void): ()=> void;

  parent?: any;
}