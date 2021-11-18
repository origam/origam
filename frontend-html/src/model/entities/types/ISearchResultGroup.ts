import { ISearchResult } from "./ISearchResult";


export interface ISearchResultGroup {
  name: string;
  results: ISearchResult[];
  isExpanded: boolean;
}
