/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { observer } from "mobx-react";
import React, { RefObject } from "react";
import { ISearchResultGroup } from "model/entities/types/ISearchResultGroup";
import { ISearchResult } from "model/entities/types/ISearchResult";
import S from "gui/Components/Search/ResultGroup.module.scss";
import { ResultItem } from "gui/Components/Search/ResultItem";
import { ISearcher } from "model/entities/types/ISearcher";

@observer
export class ResultGroup extends React.Component<{
  name: string;
  group: ISearchResultGroup;
  onResultItemClick: (result: ISearchResult) => void;
  searcher: ISearcher;
  registerElementRef: (id: string, ref: RefObject<HTMLDivElement>) => void;
}> {

  onGroupClick() {
    this.props.group.isExpanded = !this.props.group.isExpanded;
  }

  render() {
    return (
      <div>
        <div 
          key={this.props.name}
          className={S.resultGroupRow} 
          onClick={() => this.onGroupClick()}>
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
              selected={this.props.searcher.selectedResult?.id === result.id}
              registerElementRef={this.props.registerElementRef}
            />)
          }
        </div>
      </div>
    );
  }
}