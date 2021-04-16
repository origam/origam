import React from "react";
import S from "gui/Components/Search/SearchResults.module.scss";
import { ISearchResult } from "model/entities/types/ISearchResult";
import { observer } from "mobx-react";
import { ISearchResultGroup } from "model/entities/types/ISearchResultGroup";
import { observable } from "mobx";
import {Icon} from "../Icon/Icon";

export class SearchResults extends React.Component<{
  groups: ISearchResultGroup[];
  ctx: any;
}> {

  render() {
    return (
      <div className={S.root}>
        {this.props.groups.map(group => 
        <ResultGroup 
          key={group.name} 
          name={group.name} 
          results={group.results} />)}
      </div>
    );
  }
}

@observer
export class ResultGroup extends React.Component<{
  name: string;
  results: ISearchResult[];
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
            <SearchResultItem result={result} />
            )}
        </div>
      </div>
    );
  }
}

function SearchResultItem(props: { result: ISearchResult }) {
  return (
    <div className={S.resultItem} 
        onClick={()=> props.result.onClick()}>
      <Icon className={S.icon} src={props.result.iconUrl}/>
      <div className={S.textContainer}>
        <div className={S.resultItemName}>{props.result.label}</div>
        {props.result.description &&
          <div className={S.resultItemDescription}>{props.result.description}</div>
        }
      </div>
    </div>
  );
}
