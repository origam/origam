import {action } from "mobx";
import FlexSearch from "flexsearch";
import _ from "lodash";
import {ISearcher} from "./types/IClientFulltextSearch";
import { IMenuSearchResult } from "./types/ISearchResult";
import { onMainMenuItemClick } from "model/actions-ui/MainMenu/onMainMenuItemClick";


export class Searcher implements ISearcher {
  parent?: any;
  index: any;

  @action.bound onSearchFieldChange(searchTerm: string) {
    if(searchTerm.trim() === ""){
      this.subscriber?.([]);
      return;
    }
    this.doSearchTerm(searchTerm);
  }

  doSearchTerm = _.throttle(this.doSearchTermImm, 100);

  @action.bound doSearchTermImm(term: string) {
    if (!this.index) return;
    const searchResults = 
        this.index.search(term).map((node: any) => {
          switch (node.name) {
            case "Submenu":
              return {
                id: node.attributes.id,
                type: "Submenu",
                icon: node.attributes.icon,
                label: node.attributes.label,
                description: "",
                node: node,
                onClick: ()=>{
                  onMainMenuItemClick(this)({
                    event: null,
                    item: node,
                    idParameter: undefined,
                  })
                }
              };
            case "Command":
              return {
                id: node.attributes.id,
                type: "Command",
                icon: node.attributes.icon,
                label: node.attributes.label,
                description: "",
                node: node,
                onClick: ()=>{
                  onMainMenuItemClick(this)({
                    event: null,
                    item: node,
                    idParameter: undefined,
                  })
                }
              };
          }
        })
      this.subscriber?.(searchResults);
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

  subscriber: ((searchResults: IMenuSearchResult[]) => void) | undefined;

  subscribeToResultsChange(subscriber: (searchResults: IMenuSearchResult[])=> void){
    this.subscriber = subscriber;
    return ()=> this.subscriber = undefined;
  }
}
