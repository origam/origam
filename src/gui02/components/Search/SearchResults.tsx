import React from "react";
import S from "gui02/components/Search/SearchResults.module.scss";
import { ISearchResult } from "model/entities/types/ISearchResult";
import { observer } from "mobx-react";
import { ISearchResultGroup } from "model/entities/types/ISearchResultGroup";
import { observable } from "mobx";
import { uuidv4 } from "utils/uuid";

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

  items: any =[];

  componentWillMount() {
    this.items = this.props.results.map(item => { 
      return {id: uuidv4(), result: item};
    });
  }
  
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
          {this.isExpanded && this.items.map((item: any) => 
            <SearchResultItem result={item.result} key={item.id}/>
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
      <div className={S.resultItemName}>{props.result.label}</div>
      <div className={S.resultItemDescription}>{props.result.description}</div>
    </div>
  );
}
