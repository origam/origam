import S from "gui02/components/Search/SearchBox.module.scss";
import {Icon} from "gui02/components/Icon/Icon";
import React from "react";
import {observable} from "mobx";
import { getApi } from "model/selectors/getApi";
import { observer } from "mobx-react";
import {ISearchResult} from "model/entities/types/ISearchResult";


@observer
export class SearchBox extends React.Component<{
  ctx: any;
  onSearchResultsChange: (results: ISearchResult[]) => void;
}> {
  @observable
  value = "";

  async onKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key == "Enter" && this.value.trim()) {
      const api = getApi(this.props.ctx);
      const searchResults = await api.search(this.value);
      this.props.onSearchResultsChange(searchResults);
    }
  }

  onChange(event: React.ChangeEvent<HTMLInputElement>): void {
    this.value = event.target.value;
  }

  render() {
    return (
      <div className={S.searchBox}>
        <input
          className={S.input}
          value={this.value}
          onKeyDown={(event) => this.onKeyDown(event)}
          onChange={(event) => this.onChange(event)}
        />
        <div className={S.icon}>
          <Icon src="./icons/search.svg"/>
        </div>
      </div>
    );
  }
}