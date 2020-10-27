// import {IOrderByDirection} from "../../../gui/Components/ScreenElements/Table/types";
// import {IOrderByDirection} from "../../selectors/TablePanelView/types";
import {IOrdering} from "./IOrderingConfiguration";
import {IFilterGroup} from "model/entities/types/IFilterGroup";

export interface IPanelConfiguration {
  position: number | undefined;
  defaultOrdering: IOrdering[] | undefined;
  filterGroups: IFilterGroup[];
  defaultFilter: IFilterGroup | undefined;
}