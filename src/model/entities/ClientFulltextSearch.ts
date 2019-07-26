import { action, observable } from "mobx";
import FlexSearch from "flexsearch";
import _ from "lodash";
import {
  ISearchResultSection,
  ISearchResultItem,
  IClientFulltextSearch
} from "./types/IClientFulltextSearch";
import { IMenuItemIcon } from "../../gui/Workbench/MainMenu/MainMenu";

class SearchResultSection implements ISearchResultSection {
  constructor(public label: string, public items: ISearchResultItem[]) {}

  get itemCount() {
    return this.items.length;
  }
}

class SearchResultItem implements ISearchResultItem {
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

export class ClientFulltextSearch implements IClientFulltextSearch {
  parent?: any;
  @observable foundItems: ISearchResultSection[] = [];
  index: any;

  @action.bound onSearchFieldChange(event: any) {
    this.doSearchTerm(event.target.value);
  }

  doSearchTerm = _.throttle(this.doSearchTermImm, 100);

  @action.bound doSearchTermImm(term: string) {
    if (!this.index) return;
    this.foundItems = [
      new SearchResultSection(
        "Menu",
        this.index.search(term).map((res: any) => {
          console.log(res);
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
      )
    ].filter(section => section.itemCount > 0);
  }

  @action.bound
  clearResults(): void {
    this.foundItems.length = 0;
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
      switch (node.name) {
        case "Submenu":
        case "Command":
          const doc = {
            id: node.attributes.id,
            label: node.attributes.label,
            node
          };
          console.log("Adding:", doc);
          documents.push(node);
      }
      node.elements.forEach((element: any) => recursive(element));
    };
    mainMenu.elements.forEach((element: any) => {
      recursive(element);
    });
    this.index.add(documents);
  }
}
