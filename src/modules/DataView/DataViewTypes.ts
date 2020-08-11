import { IRenderable } from "modules/CommonTypes";
import { TypeSymbol } from "dic/Container";
import {DataView} from 'model/entities/DataView';

export const SectionViewSwitchers = "SectionViewSwitchers";

export interface IDataViewToolbarContribItem extends IRenderable {
  section: string;
}

export const IDataView = TypeSymbol<DataView>("IDataView");