import { Icon } from "gui02/components/Icon/Icon";
import { observer } from "mobx-react";
import React from "react";
import { T } from "utils/translation";
import { ModalWindow } from "../Dialog/Dialog";
import S from "gui/Components/Dialogs/SearchDialog.module.scss";
import { observable } from "mobx";
import { ISearchResult } from "model/entities/types/ISearchResult";
import { ISearchResultGroup } from "model/entities/types/ISearchResultGroup";
import { getSearcher } from "model/selectors/getClientFulltextSearch";

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

  searcher = getSearcher(this.props.ctx);

  @observable
  value = "";

  timeout: NodeJS.Timeout | undefined;

  componentDidMount(){
    this.input?.focus();
  }

  onKeyDown(event: any){
    if(event.key === "Escape"){
      this.props.onCloseClick();
    }
  }

  onResultItemClick(){
    this.props.onSearchResultsChange(this.searcher.resultGroups);
    this.props.onCloseClick();
  }

  async onInputKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter") {
      this.searcher.searchOnServer();
      return;
    }
    if(this.timeout)
    {
      clearTimeout(this.timeout);
    }
    this.timeout = setTimeout(()=>{
      this.timeout = undefined;
      this.searcher.searchOnServer();
    }, DELAY_BEFORE_SERVER_SEARCH_MS)
  }

  onInputChange(event: React.ChangeEvent<HTMLInputElement>): void {
    this.value = event.target.value;
    this.searcher.onSearchFieldChange(this.value);
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
              onChange={(event) => this.onInputChange(event)}
            />
          </div>
          {(this.searcher.resultGroups.length > 0 ) &&
            <div className={S.resultArea}>
              {this.searcher.resultGroups
                .map(group=> 
                  <ResultGroup 
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
              onResultItemClick={()=> this.props.onResultItemClick()}
              />)
        }
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
    this.props.onResultItemClick();
    this.props.result.onClick();
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

