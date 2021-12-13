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

const DELAY_BEFORE_SERVER_SEARCH_MS = 1000;
export const SEARCH_DIALOG_KEY = "Search Dialog";

@observer
export class SearchView extends React.Component<{
  state: SearchViewState
}> {

  get viewState() {
    return this.props.state;
  }

  componentDidMount() {
    this.viewState.input?.focus();
  }

  render() {
    return (
      <div className={S.root}>
        <div className={S.inputRow}>
          <Icon className={S.icon} src="./icons/search.svg"/>
          <input
            ref={this.viewState.refInput}
            className={S.input}
            placeholder={T("Search for anything here", "type_search_here")}
            onKeyDown={(event) => this.viewState.onInputKeyDown(event)}
            onChange={(event) => this.viewState.onInputChange(event)}
          />
        </div>
        {(this.viewState.searcher.resultGroups.length > 0) &&
          <div className={S.resultArea} ref={this.viewState.scrollDivRef}>
            <div className={S.resultsContainer}>
              {this.viewState.searcher.resultGroups
                .map(group =>
                  <ResultGroup
                    key={group.name}
                    name={group.name}
                    group={group}
                    onResultItemClick={result => this.viewState.onResultItemClick(result)}
                    selectedResult={this.viewState.searcher.selectedResult}
                    registerElementRef={(id, ref) => this.viewState.resultElementMap.set(id, ref)}
                  />)
              }
            </div>
          </div>
        }
      </div>
    );
  }
}


export class SearchViewState {

  constructor(
    private ctx: any,
    private onCloseClick: ()=>void)
  {

  }

  input: HTMLInputElement | undefined;
  refInput = (elm: HTMLInputElement) => (this.input = elm);

  scrollDivRef: RefObject<HTMLDivElement> = React.createRef();
  resultElementMap: Map<string, RefObject<HTMLDivElement>> = new Map();

  searcher = getSearcher(this.ctx);

  @observable
  value = "";

  timeout: NodeJS.Timeout | undefined;

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

  async onInputKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (this.timeout) {
      clearTimeout(this.timeout);
    }
    this.timeout = setTimeout(() => {
      this.timeout = undefined;
      this.searcher.searchOnServer();
    }, DELAY_BEFORE_SERVER_SEARCH_MS)
  }

  onInputChange(event: React.ChangeEvent<HTMLInputElement>): void {
    this.value = event.target.value;
    this.searcher.onSearchFieldChange(this.value);
  }
}
