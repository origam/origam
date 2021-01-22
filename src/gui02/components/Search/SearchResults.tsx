import React, { useContext } from "react";
import S from "gui02/components/Search/SearchResults.module.scss";
import { ISearchResult } from "model/entities/types/ISearchResult";
import { MobXProviderContext, observer } from "mobx-react";
import { IApplication } from "model/entities/types/IApplication";
import { onSearchResultClick } from "model/actions/Workbench/onSearchResultClick";
import { ISearchResultGroup } from "model/entities/types/ISearchResultGroup";
import { observable } from "mobx";

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
            <SearchResultItem result={result} key={result.name+result.group+result.referenceId}/>
            )}
        </div>
      </div>
    );
  }
}

function SearchResultItem(props: { result: ISearchResult }) {
  const application = useContext(MobXProviderContext).application as IApplication;

  return (
    <div className={S.resultItem} 
        onClick={()=> onSearchResultClick(application)(props.result.dataSourceLookupId, props.result.referenceId)}>
      <div className={S.resultItemName}>{props.result.name}</div>
      <div className={S.resultItemDescription}>{props.result.description}</div>
    </div>
  );
}
