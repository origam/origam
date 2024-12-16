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
import S from "gui/Components/Search/SearchView.module.scss";
import { Icon } from "gui/Components/Icon/Icon";
import { T } from "utils/translation";
import { ResultGroup } from "gui/Components/Search/ResultGroup";
import { getSearcher } from "model/selectors/getSearcher";
import { action, observable } from "mobx";
import { ISearchResult } from "model/entities/types/ISearchResult";
import { getMainMenuState } from "model/selectors/MainMenu/getMainMenuState";
import { ISearcher } from "model/entities/types/ISearcher";
import Measure from "react-measure";
import { requestFocus } from "utils/focus";

const DELAY_BEFORE_SERVER_SEARCH_MS = 1000;
export const SEARCH_DIALOG_KEY = "Search Dialog";

@observer
export class SearchView extends React.Component<{
  state: SearchViewState
}> {

  @observable
  resultContentHeight = 0;

  get viewState() {
    return this.props.state;
  }

  componentDidMount() {
    requestFocus(this.viewState.input);
  }

  render() {
    const viewportHeight = Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0);
    const maxResultWindowHeight = viewportHeight * 0.35

    return (
      <div className={"searchView"}>
        <div className={S.inputRow}>
          <Icon className={S.icon} src="./icons/search.svg"/>
          <input
            ref={this.viewState.refInput}
            className={S.input}
            autoComplete={"off"}
            placeholder={T("Search for anything here", "type_search_here")}
            onChange={(event) => this.viewState.onInputChange(event)}
          />
        </div>
        {(this.viewState.searcher.resultGroups.length > 0) &&
          <div
            className={"resultArea"}
            ref={this.viewState.scrollDivRef}
            style={{
              maxHeight: maxResultWindowHeight + "px",
              overflowY: this.resultContentHeight > maxResultWindowHeight ? "auto" : "hidden"
          }}
          >
            <Measure
              bounds
              onResize={contentRect => {
                this.resultContentHeight = contentRect.bounds?.height ?? 0;
              }}
            >
              {({measureRef}) => (
                <div ref={measureRef} className={S.resultsContainer}>
                  {this.viewState.searcher.resultGroups
                    .map(group =>
                      <ResultGroup
                        key={group.name}
                        name={group.name}
                        group={group}
                        onResultItemClick={result => this.viewState.onResultItemClick(result)}
                        searcher={this.viewState.searcher}
                        registerElementRef={(id, ref) => this.viewState.resultElementMap.set(id, ref)}
                      />)
                  }
                </div>
              )}
            </Measure>
          </div>
        }
      </div>
    );
  }
}


export class SearchViewState {
  input: HTMLInputElement | undefined;
  refInput = (elm: HTMLInputElement) => (this.input = elm);

  scrollDivRef: RefObject<HTMLDivElement> = React.createRef();
  resultElementMap: Map<string, RefObject<HTMLDivElement>> = new Map();

  searcher: ISearcher;

  @observable
  value = "";

  timeout: NodeJS.Timeout | undefined;

  constructor(
    private ctx: any,
    private onCloseClick: () => void) {
    this.searcher = getSearcher(this.ctx);
    this.searcher.clear();
  }

  @action.bound
  onKeyDown(event: any) {
    if (event.key === "Escape") {
      this.onCloseClick();
      return;
    }
    if (event.key === "ArrowDown") {
      event.preventDefault();
      this.searcher.selectNextResult();
      this.scrollToCell();
      return;
    }
    if (event.key === "ArrowUp") {
      event.preventDefault();
      this.searcher.selectPreviousResult();
      this.scrollToCell();
      return;
    }
    if (event.key === "Enter") {
      if (this.searcher.selectedResult) {
        this.searcher.selectedResult.onClick();
        this.onResultItemClick(this.searcher.selectedResult);
      } else {
        this.searcher.searchOnServer();
      }
      return;
    }
  }

  onResultItemClick(result: ISearchResult) {
    this.onCloseClick();
    getMainMenuState(this.ctx).highlightItem(result.id);
  }

  scrollToCell() {
    if (!this.searcher.selectedResult) {
      return;
    }
    const scrollElement = this.scrollDivRef.current;
    const selectedElement = this.resultElementMap.get(this.searcher.selectedResult.id)?.current;
    if (!selectedElement || !scrollElement) {
      return;
    }
    const selectedElementRectangle = selectedElement.getBoundingClientRect();
    const scrollRectangle = scrollElement.getBoundingClientRect();
    const distanceOverTop = scrollRectangle.top - selectedElementRectangle.top;

    const scrollBarHeight = scrollRectangle.height - scrollElement.clientHeight;
    const distanceUnderBottom = selectedElementRectangle.bottom - scrollRectangle.bottom + scrollBarHeight;

    if (distanceOverTop > 0) {
      scrollElement.scrollTop -= distanceOverTop;
    } else if (distanceUnderBottom > 0) {
      scrollElement.scrollTop += distanceUnderBottom;
    } else if (distanceOverTop === 0) {
      const resultIndices = this.searcher.getSelectedResultIndices();
      const topItemSelected = resultIndices.groupIndex === 0 && resultIndices.indexInGroup === 0;
      if (topItemSelected) {
        scrollElement.scrollTop = 0;
      }
    }
  }

  onInputChange(event: React.ChangeEvent<HTMLInputElement>): void {
    this.value = event.target.value;
    this.searcher.onSearchFieldChange(this.value);

    if (this.timeout) {
      clearTimeout(this.timeout);
    }
    this.timeout = setTimeout(() => {
      this.timeout = undefined;
      this.searcher.searchOnServer();
    }, DELAY_BEFORE_SERVER_SEARCH_MS)
  }

  clear() {
    this.searcher.clear();
  }
}
