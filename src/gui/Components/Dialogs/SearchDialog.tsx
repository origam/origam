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

import { Icon } from "gui/Components/Icon/Icon";
import { observer } from "mobx-react";
import React, { RefObject } from "react";
import { T } from "utils/translation";
import { ModalWindow } from "../Dialog/Dialog";
import S from "gui/Components/Dialogs/SearchDialog.module.scss";
import { action, observable } from "mobx";
import { ISearchResult } from "model/entities/types/ISearchResult";
import { getSearcher } from "model/selectors/getSearcher";
import { ISearchResultGroup } from "model/entities/types/ISearchResultGroup";
import { getMainMenuState } from "model/selectors/MainMenu/getMainMenuState";

const DELAY_BEFORE_SERVER_SEARCH_MS = 1000;
export const SEARCH_DIALOG_KEY = "Search Dialog";

@observer
export class SearchDialog extends React.Component<{
  ctx: any;
  onCloseClick: () => void;
}> {

  input: HTMLInputElement | undefined;
  refInput = (elm: HTMLInputElement) => (this.input = elm);

  scrollDivRef: RefObject<HTMLDivElement> = React.createRef();
  resultElementMap: Map<string, RefObject<HTMLDivElement>> = new Map();

  searcher = getSearcher(this.props.ctx);

  @observable
  value = "";

  timeout: NodeJS.Timeout | undefined;

  componentDidMount(){
    this.input?.focus();
  }

  @action.bound
  onKeyDown(event: any){
    if(event.key === "Escape"){
      this.props.onCloseClick();
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
      if(this.searcher.selectedResult){
        this.searcher.selectedResult.onClick();
        this.onResultItemClick(this.searcher.selectedResult);
      }
      else
      {
        this.searcher.searchOnServer();
      }
      return;
    }
  }

  onResultItemClick(result: ISearchResult){
    this.props.onCloseClick();
    getMainMenuState(this.props.ctx).highlightItem(result.id);
  }

  scrollToCell(){
    if(!this.searcher.selectedResult){
      return;
    }
    const scrollElement =this.scrollDivRef.current;
    const selectedElement = this.resultElementMap.get(this.searcher.selectedResult.id)?.current;
    if(!selectedElement || !scrollElement){
      return;
    }
    const selectedElementRectangle = selectedElement.getBoundingClientRect();
    const scrollRectangle = scrollElement.getBoundingClientRect();
    const distanceOverTop = scrollRectangle.top - selectedElementRectangle.top;

    const scrollBarHeight = scrollRectangle.height - scrollElement.clientHeight;
    const distanceUnderBottom = selectedElementRectangle.bottom - scrollRectangle.bottom + scrollBarHeight;

    if(distanceOverTop > 0){
      scrollElement.scrollTop -= distanceOverTop;
    }
    else if(distanceUnderBottom > 0){
      scrollElement.scrollTop += distanceUnderBottom;
    }
    else if(distanceOverTop === 0){
      const resultIndices = this.searcher.getSelectedResultIndices();
      const topItemSelected = resultIndices.groupIndex === 0 && resultIndices.indexInGroup === 0;
      if(topItemSelected){
        scrollElement.scrollTop = 0;
      }
    }
  }

  async onInputKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
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
        topPosiotionProc={30}
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
            <div className={S.resultArea} ref={this.scrollDivRef}>
              <div className={S.resultsContainer}>
                {this.searcher.resultGroups
                  .map(group=>
                    <ResultGroup
                      key={group.name}
                      name={group.name}
                      group={group}
                      onResultItemClick={result => this.onResultItemClick(result)}
                      selectedResult={this.searcher.selectedResult}
                      registerElementRef={(id, ref)=> this.resultElementMap.set(id, ref)}
                      />)
                }
              </div>
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
            <i className={"fas fa-angle-up " + S.arrow} />
          ) : (
            <i className={"fas fa-angle-down " + S.arrow} />
          )}
          <div className={S.groupName}>
            {this.props.name}
          </div>
        </div>
        <div>
        {this.props.group.isExpanded && this.props.group.results.map(result =>
            <ResultItem
              key={result.label + result.description + result.iconUrl}
              result={result}
              onResultItemClick={()=> this.props.onResultItemClick(result)}
              selected={this.props.selectedResult?.id === result.id}
              registerElementRef={this.props.registerElementRef}
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
  selected: boolean;
  registerElementRef: (id: string, ref: RefObject<HTMLDivElement>) => void;
}> {

  @observable
  mouseOver = false;

  divRef: RefObject<HTMLDivElement> = React.createRef();

  componentDidMount(){
    this.props.registerElementRef(this.props.result.id, this.divRef);
  }

  componentDidUpdate(){
    this.props.registerElementRef(this.props.result.id, this.divRef);
  }

  @action.bound
  onClick(){
    this.props.onResultItemClick();
    this.props.result.onClick();
  }

  render() {
    return (
      <div
        className={ S.resultIemRow + " "+(this.mouseOver ? S.resultIemRowHovered : "") + " " +
                   (this.props.selected ? S.resultIemRowSelected : "")}
        ref={this.divRef}
        onClick={() => this.onClick()}
        onMouseOver={evt => this.mouseOver = true}
        onMouseOut={evt=> this.mouseOver = false}>
        <div className={S.itemIcon}>
          <Icon src= {this.props.result.iconUrl} />
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

