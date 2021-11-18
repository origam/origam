import {IMenuItemIcon} from "gui/Workbench/MainMenu/IMenuItemIcon";

export interface ISearchResult
{
  id: string;
  label: string,
  description: string,
  iconUrl: string;
  onClick: ()=> void;
}

export interface IServerSearchResult extends ISearchResult
{
  group: string,
  dataSourceId: string,
  dataSourceLookupId: string,
  referenceId: string
}

export interface IMenuSearchResult  extends ISearchResult {
  type: string;
}