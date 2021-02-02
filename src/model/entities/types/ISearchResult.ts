import { IMenuItemIcon } from "gui/Workbench/MainMenu/MainMenu";

export interface ISearchResult
{
  id: string;
  label: string,
  description: string,
  icon: IMenuItemIcon;
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