import React, {useContext} from "react";
import S from "gui02/components/Search/SearchResults.module.scss";
import { ISearchResult } from "model/entities/types/ISearchResult";
import {Observer} from "mobx-react";
import {DropdownLayout} from "modules/Editors/DropdownEditor/Dropdown/DropdownLayout";
import {DropdownEditorControl} from "modules/Editors/DropdownEditor/DropdownEditorControl";
import {DropdownLayoutBody} from "modules/Editors/DropdownEditor/Dropdown/DropdownLayoutBody";
import {DropdownEditorBody} from "modules/Editors/DropdownEditor/DropdownEditorBody";
import {CtxDropdownEditor} from "modules/Editors/DropdownEditor/DropdownEditor";

export class SearchResults extends React.Component<{
  results: ISearchResult[];
}> {
  render(){
    return(
      <div className={S.root}>
        {this.props.results.map(result => <SearchResultItem result={result}/>)}
      </div>
    );
  }
}

function SearchResultItem(props: {
  result: ISearchResult
}) {
  return (
    <div className={S.resultItem}>
      <div className={S.resultItemName}>
        {props.result.name}
      </div>
      <div className={S.resultItemDescription}>
        {props.result.description}
      </div>
    </div>
  );
}
