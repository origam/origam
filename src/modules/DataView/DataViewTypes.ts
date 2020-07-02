import {IRenderable} from "modules/CommonTypes";

export const SectionViewSwitchers = "SectionViewSwitchers";

export interface IDataViewToolbarContribItem extends IRenderable {
  section: string;
}