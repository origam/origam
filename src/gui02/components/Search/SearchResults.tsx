import React, {useContext} from "react";
import S from "gui02/components/Search/SearchResults.module.scss";
import { ISearchResult } from "model/entities/types/ISearchResult";
import {MobXProviderContext, Observer} from "mobx-react";
import {DropdownLayout} from "modules/Editors/DropdownEditor/Dropdown/DropdownLayout";
import {DropdownEditorControl} from "modules/Editors/DropdownEditor/DropdownEditorControl";
import {DropdownLayoutBody} from "modules/Editors/DropdownEditor/Dropdown/DropdownLayoutBody";
import {DropdownEditorBody} from "modules/Editors/DropdownEditor/DropdownEditorBody";
import {CtxDropdownEditor} from "modules/Editors/DropdownEditor/DropdownEditor";
import {IApplication} from "model/entities/types/IApplication";
import {getWorkbenchLifecycle} from "model/selectors/getWorkbenchLifecycle";
import { getApi } from "model/selectors/getApi";
import { flow } from "mobx";
import {selectNextRow} from "model/actions/DataView/selectNextRow";
import {handleError} from "model/actions/handleError";

export class SearchResults extends React.Component<{
  results: ISearchResult[];
}> {
  render(){
    return(
      <div className={S.root}>
        {Array.isArray(this.props.results)
          ? this.props.results.map(result => <SearchResultItem result={result}/>)
          : null}
      </div>
    );
  }
}

function SearchResultItem(props: {
  result: ISearchResult
}) {
  const application = useContext(MobXProviderContext)
    .application as IApplication;

  async function onClick(event: any){
    flow(function* () {
        try {
          const api = getApi(application);
          const menuId = yield api.getMenuId({
            LookupId: props.result.dataSourceLookupId,
            ReferenceId: props.result.referenceId
          })
          yield* getWorkbenchLifecycle(application)
            .onMainMenuItemIdClick({
              event: event,
              itemId: menuId,
              idParameter: props.result.referenceId
            });
        } catch (e) {
          yield* handleError(application)(e);
          throw e;
        }
      }
     )();
  }

  return (
    <div className={S.resultItem} onClick={onClick}>
      <div className={S.resultItemName}>
        {props.result.name}
      </div>
      <div className={S.resultItemDescription}>
        {props.result.description}
      </div>
    </div>
  );
}
