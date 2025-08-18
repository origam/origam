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

import React from "react";
import S from "gui/Components/Search/SearchResults.module.scss";
import { ISearchResult, isIMenuSearchResult } from "model/entities/types/ISearchResult";
import { observer } from "mobx-react";
import { ISearchResultGroup } from "model/entities/types/ISearchResultGroup";
import { observable } from "mobx";
import { Icon } from "gui/Components/Icon/Icon";
import { Dropdown } from "../Dropdown/Dropdown";
import { DropdownItem } from "../Dropdown/DropdownItem";
import { T } from "../../../utils/translation";
import { Dropdowner } from "../Dropdowner/Dropdowner";
import { getFavorites } from "../../../model/selectors/MainMenu/getFavorites";
import { onAddToFavoritesClicked } from "../../connections/CMainMenu";

export class SearchResults extends React.Component<{
  groups: ISearchResultGroup[];
  ctx: any;
}> {

  render() {
    return (
      <div className={S.root}>
        {this.props.groups.map(group =>
          <ResultGroup
            ctx={this.props.ctx}
            key={group.name}
            name={group.name}
            results={group.results}/>)}
      </div>
    );
  }
}

@observer
export class ResultGroup extends React.Component<{
  name: string;
  ctx: any;
  results: ISearchResult[];
}> {
  @observable
  isExpanded = true;

  onGroupClick() {
    this.isExpanded = !this.isExpanded;
  }

  get favorites() {
    return getFavorites(this.props.ctx);
  }

  render() {
    return (
      <>
        <div className={S.resultGroupRow} onClick={() => this.onGroupClick()}>
          {this.isExpanded ? (
            <i className={"fas fa-angle-up " + S.arrow}/>
          ) : (
            <i className={"fas fa-angle-down " + S.arrow}/>
          )}
          <div className={S.groupName}>
            {this.props.name}
          </div>
        </div>
        <div className={S.dropDownParent}>
          {this.isExpanded && this.props.results.map(result =>
            <Dropdowner
              key={result.id}
              trigger={({refTrigger, setDropped}) => (
                <SearchResultItem
                  refDom={refTrigger}
                  result={result}
                  onContextMenu={(event) => {
                    setDropped(true, event);
                    event.preventDefault();
                    event.stopPropagation();
                  }}
                />
              )}
              content={({setDropped}) => (
                <Dropdown>
                  {isIMenuSearchResult(result) && !this.favorites.isInAnyFavoriteFolder(result.id) && (
                    <DropdownItem
                      onClick={(event: any) => {
                        setDropped(false);
                        onAddToFavoritesClicked(this.props.ctx, result.id);
                      }}
                    >
                      {T("Put to favourites", "put_to_favourites")}
                    </DropdownItem>
                  )}
                </Dropdown>

              )}
            />
          )}
        </div>
      </>
    );
  }
}

function SearchResultItem(props: {
  result: ISearchResult;
  onContextMenu?(event: any): void;
  refDom?: any;
}) {
  return (
    <div className={S.resultItem}
         onContextMenu={props.onContextMenu}
         ref={props.refDom}
         onClick={() => props.result.onClick()}>
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
