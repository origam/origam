import {MobXProviderContext} from "mobx-react";
import {observer} from "mobx-react-lite";
import {IApplication} from "model/entities/types/IApplication";
import React, {useContext} from "react";
import {ISearchResultItem} from "../../model/entities/types/IClientFulltextSearch";
import {getClientFulltextSearch} from "../../model/selectors/getClientFulltextSearch";
import {IMenuItemIcon} from "./MainMenu/MainMenu";
import S from "./SearchResults.module.css";
import {getWorkbenchLifecycle} from "model/selectors/getWorkbenchLifecycle";

export const SearchResultItem: React.FC<{
  icon: React.ReactNode;
  label: React.ReactNode;
  description: React.ReactNode;
  onClick?: (event: any) => void;
}> = props => (
  <div className={S.searchResultItem} onClick={props.onClick}>
    <div className={S.searchResultItemIcon}>{props.icon}</div>
    <div className={S.searchResultItemText}>
      <div className={S.searchResultItemLabel}>{props.label}</div>
      <div className={S.searchResultItemDescription}>{props.description}</div>
    </div>
  </div>
);

function itemIcon(item: ISearchResultItem) {
  switch (item.icon) {
    case IMenuItemIcon.Folder:
      return <i className="fa fa-folder icon" />;
    case IMenuItemIcon.Form:
      return <i className="fas fa-file-alt icon" />;
    case IMenuItemIcon.Workflow:
      return <i className="fa fa-magic icon" />;
    case IMenuItemIcon.Parameter:
      return <i className="fas fa-asterisk icon" />;
    default:
      return null;
  }
}

export const SearchResultsPanel: React.FC<{}> = observer(props => {
  const application = useContext(MobXProviderContext)
    .application as IApplication;
  const clientFulltextSearch = getClientFulltextSearch(application);
  const foundItems = clientFulltextSearch.foundItems;
  const handleMainMenuItemClick = (event: any, item: any) =>
    getWorkbenchLifecycle(application).onMainMenuItemClick({
      event: event, item: item, idParameter: undefined });
  return (
    <div className={S.searchResultsPanel}>
      {foundItems.map(searchResultSection => (
        <>
          <div className={S.searchResultsSectionLabel}>
            {searchResultSection.label} ({searchResultSection.itemCount})
          </div>
          {searchResultSection.items.map(item => (
            <SearchResultItem
              icon={itemIcon(item)}
              label={item.label}
              description={item.description}
              onClick={event => handleMainMenuItemClick(event, item.node)}
            />
          ))}
        </>
      ))}
    </div>
  );
});
