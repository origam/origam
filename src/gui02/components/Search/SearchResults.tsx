import React, { useContext } from "react";
import S from "gui02/components/Search/SearchResults.module.scss";
import { ISearchResult } from "model/entities/types/ISearchResult";
import { MobXProviderContext } from "mobx-react";
import { IApplication } from "model/entities/types/IApplication";
import { onSearchResultClick } from "model/actions/Workbench/onSearchResultClick";

export class SearchResults extends React.Component<{
  results: ISearchResult[];
}> {
  render() {
    return (
      <div className={S.root}>
        {Array.isArray(this.props.results)
          ? this.props.results.map((result) => <SearchResultItem result={result} />)
          : null}
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
