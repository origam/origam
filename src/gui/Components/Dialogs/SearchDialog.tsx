import { Icon } from "gui02/components/Icon/Icon";
import { observer } from "mobx-react";
import React from "react";
import { T } from "utils/translation";
import { ModalWindow } from "../Dialog/Dialog";
import S from "gui/Components/Dialogs/SearchDialog.module.scss";
import { observable } from "mobx";
import { getApi } from "model/selectors/getApi";
import { IMenuSearchResult, ISearchResult, IServerSearchResult } from "model/entities/types/ISearchResult";
import { onSearchResultClick } from "model/actions/Workbench/onSearchResultClick";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { ISearchResultGroup } from "model/entities/types/ISearchResultGroup";
import { getClientFullTextSearch } from "model/selectors/getClientFulltextSearch";
import { IMenuItemIcon } from "gui/Workbench/MainMenu/MainMenu";
import { onMainMenuItemClick } from "model/actions-ui/MainMenu/onMainMenuItemClick";

const DELAY_BEFORE_SERVER_SEARCH_MS = 1000;
export const SEARCH_DIALOG_KEY = "Search Dialog";

@observer
export class SearchDialog extends React.Component<{
  ctx: any;
  onCloseClick: () => void;
  onSearchResultsChange: (groups: ISearchResultGroup[]) => void;
}> {

  input: HTMLInputElement | undefined;
  refInput = (elm: HTMLInputElement) => (this.input = elm);

  menuSearch = getClientFullTextSearch(this.props.ctx);
  
  @observable
  value = "";

  timeout: NodeJS.Timeout | undefined;

  @observable
  groups: ISearchResultGroup[] = [];

  componentDidMount(){
    this.input?.focus();
  }

  onKeyDown(event: any){
    if(event.key === "Escape"){
      this.props.onCloseClick();
    }
  }

  onItemServerClick(searchResult: IServerSearchResult){
    this.props.onSearchResultsChange(this.groups);
    onSearchResultClick(this.props.ctx)(searchResult.dataSourceLookupId, searchResult.referenceId)
  }

  onResultItemClick(){
    this.props.onCloseClick();
  }

  searchOnServer(){
    if(!this.value.trim()){
      this.groups = [];
      return;
    }
    runInFlowWithHandler({
      ctx: this.props.ctx, 
      action : async ()=> 
      {
        const api = getApi(this.props.ctx);
        const searchResults = await api.search(this.value);
        for (const searchResult of searchResults) {
          searchResult.icon = IMenuItemIcon.Form;
          searchResult.onClick = ()=> this.onItemServerClick(searchResult);
        }
        const groupMap =  searchResults.groupBy((item:IServerSearchResult) => item.group);  
        this.groups = Array.from(groupMap.keys())
                .sort()
                .map(name => { return {name: name, results: groupMap.get(name)!}})
      } 
    });
  }

  async onInputKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter") {
      this. searchOnServer();
      return;
    }
    if(this.timeout)
    {
      clearTimeout(this.timeout);
    }
    this.timeout = setTimeout(()=>{
      this.timeout = undefined;
      this.searchOnServer();
    }, DELAY_BEFORE_SERVER_SEARCH_MS)

    this.menuSearch.onSearchFieldChange((event.target as any).value);
  }

  onChange(event: React.ChangeEvent<HTMLInputElement>): void {
    this.value = event.target.value;
  }

  render() {
    return (
      <ModalWindow
        title={null}
        titleButtons={null}
        buttonsCenter={null}
        onKeyDown={(event:any) => this.onKeyDown(event)}
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={S.root}>
          <div className={S.inputRow}>
            <Icon className={S.icon} src="./icons/search.svg" />
            <input
              ref={this.refInput}
              className={S.input}
              placeholder={T("Search for anything here", "type_search_here")}
              onKeyDown={(event) => this.onInputKeyDown(event)}
              onChange={(event) => this.onChange(event)}
            />
          </div>
          {(this.groups.length > 0 || this.menuSearch.searchResults.length > 0) &&
            <div className={S.resultArea}>
              {this.menuSearch.searchResults.length > 0 &&
                <ResultGroup 
                    key={"Menu"} 
                    name={"Menu"} 
                    results={this.menuSearch.searchResults}
                    onResultItemClick={()=> this.onResultItemClick()}
                />
              }
              {this.groups.length > 0 && 
                this.groups
                  .map(group=> 
                    <ResultGroup 
                      key={group.name} 
                      name={group.name} 
                      results={group.results}
                      onResultItemClick={()=> this.onResultItemClick()}
                      />) 
              }
            </div>
          }
        </div>
      </ModalWindow>
    );
  }
}

@observer
export class ResultGroup extends React.Component<{
  name: string;
  results: ISearchResult[];
  onResultItemClick: ()=> void;
}> {
  @observable
  isExpanded = true;

  onGroupClick() {
    this.isExpanded = !this.isExpanded;
  }

  render() {
    return (
      <div>
        <div className={S.resultGroupRow} onClick={() => this.onGroupClick()}>
          {this.isExpanded ? (
            <i className={"fas fa-angle-up " + S.arrow} />
          ) : (
            <i className={"fas fa-angle-down " + S.arrow} />
          )}
          <div className={S.groupName}>
            {this.props.name}
          </div>
        </div>
        <div>
          {this.isExpanded && this.props.results.map(result => 
            <ResultItem 
              result={result} 
              key={result.label + result.description}
              onResultItemClick={()=> this.props.onResultItemClick()}
            /> )}
        </div>
      </div>
    );
  }
}

@observer
export class ResultItem extends React.Component<{
  result: ISearchResult;
  onResultItemClick: ()=> void;
}> {

  onClick(){
    this.props.result.onClick();
    this.props.onResultItemClick();
  }

  render() {
    return (
      <div className={S.resultIemRow} onClick={() => this.onClick()} >
        <div className={S.itemIcon}>
          <Icon src="./icons/document.svg" />
        </div>
        <div className={S.itemContents}>
          <div className={S.itemTitle}>
            {this.props.result.label}
          </div>
          <div className={S.itemTextSeparator}>
            {" "}
          </div>
          <div className={S.itemText}>
            {this.props.result.description}
          </div>
        </div>
      </div>
    );
  }
}

