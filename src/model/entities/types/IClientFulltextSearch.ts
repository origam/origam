import { IMenuSearchResult } from './ISearchResult';

export interface IClientFullTextSearch {
  searchResults: IMenuSearchResult[];
  onSearchFieldChange(searchTerm: string): void;
  clearResults(): void;
  indexMainMenu(mainMenu: any): void;

  subscribeOpenSearchSection(open: () => void): () => void;

  parent?: any;
}