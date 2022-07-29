import { observer } from "mobx-react";
import React, { RefObject } from "react";
import { ISearchResultGroup } from "model/entities/types/ISearchResultGroup";
import { ISearchResult } from "model/entities/types/ISearchResult";
import S from "gui/Components/Search/ResultGroup.module.scss";
import { ResultItem } from "gui/Components/Search/ResultItem";

@observer
export class ResultGroup extends React.Component<{
  name: string;
  group: ISearchResultGroup;
  onResultItemClick: (result: ISearchResult) => void;
  selectedResult: ISearchResult | undefined;
  registerElementRef: (id: string, ref: RefObject<HTMLDivElement>) => void;
}> {

  onGroupClick() {
    this.props.group.isExpanded = !this.props.group.isExpanded;
  }

  render() {
    return (
      <div>
        <div className={S.resultGroupRow} onClick={() => this.onGroupClick()}>
          {this.props.group.isExpanded ? (
            <i className={"fas fa-angle-up " + S.arrow}/>
          ) : (
            <i className={"fas fa-angle-down " + S.arrow}/>
          )}
          <div className={S.groupName}>
            {this.props.name}
          </div>
        </div>
        <div>
          {this.props.group.isExpanded && this.props.group.results.map(result =>
            <ResultItem
              key={result.id}
              result={result}
              onResultItemClick={() => this.props.onResultItemClick(result)}
              selected={this.props.selectedResult?.id === result.id}
              registerElementRef={this.props.registerElementRef}
            />)
          }
        </div>
      </div>
    );
  }
}