import {action, observable} from "mobx";
import FlexSearch from "flexsearch";
import _ from "lodash";
import {IClientFullTextSearch} from "./types/IClientFulltextSearch";
import {IMenuItemIcon} from "../../gui/Workbench/MainMenu/MainMenu";
import { IMenuSearchResult } from "./types/ISearchResult";

class SearchResultItem implements IMenuSearchResult {
  constructor(
    public id: string,
    public type: string,
    public icon: IMenuItemIcon,
    public label: string,
    public description: string,
    public node: any
  ) {}
}

function makeMenuPath(node: any) {
  const path: string[] = [];
  let cn = node;
  while (cn !== undefined) {
    path.push(cn);
    cn = cn.parent;
  }
  path.reverse();
  return path.slice(2); // Strip out root and Menu node
}

export class ClientFullTextSearch implements IClientFullTextSearch {
  parent?: any;
  // @observable foundItems: ISearchResultSection[] = [];
  @observable  searchResults: IMenuSearchResult[] = [];
  index: any;

  @action.bound onSearchFieldChange(searchTerm: string) {
    this.doSearchTerm(searchTerm);
  }

  doSearchTerm = _.throttle(this.doSearchTermImm, 100);

  @action.bound doSearchTermImm(term: string) {
    if (!this.index) return;
    this.triggerOpenSearchSection();
    this.searchResults = 
        this.index.search(term).map((res: any) => {
          switch (res.name) {
            case "Submenu":
              return new SearchResultItem(
                res.attributes.id,
                "Submenu",
                res.attributes.icon,
                res.attributes.label,
                makeMenuPath(res)
                  .map((n: any) => n.attributes.label)
                  .join(" > "),
                res
              );
            case "Command":
              return new SearchResultItem(
                res.attributes.id,
                "Command",
                res.attributes.icon,
                res.attributes.label,
                makeMenuPath(res)
                  .map((n: any) => n.attributes.label)
                  .join(" > "),
                res
              );
          }
        })
      // );
  }

  @action.bound
  clearResults(): void {
    this.searchResults.length = 0;
  }

  @action.bound
  indexMainMenu(mainMenu: any) {
    this.index = FlexSearch.create({
      encode: "extra",
      doc: {
        id: "attributes:id",
        field: "attributes:label"
      }
    } as any);
    const documents: any[] = [];
    const recursive = (node: any) => {
      if (node.attributes.isHidden === "true") {
        return;
      }
      switch (node.name) {
        case "Submenu":
        case "Command":
          documents.push(node);
      }
      node.elements.forEach((element: any) => recursive(element));
    };
    mainMenu.elements.forEach((element: any) => {
      recursive(element);
    });
    this.index.add(documents);
  }

  openSearchSectionHandlers: Map<number, () => void> = new Map();
  openSearchSectionHandlersId = 0;

  @action.bound
  subscribeOpenSearchSection(open: () => void): () => void {
    const myId = this.openSearchSectionHandlersId++;
    this.openSearchSectionHandlers.set(myId, open);
    return () => this.openSearchSectionHandlers.delete(myId);
  }

  @action.bound triggerOpenSearchSection() {
    for (let handler of this.openSearchSectionHandlers.values()) {
      handler();
    }
  }
}
