// import {IOrderByDirection} from "../../../gui/Components/ScreenElements/Table/types";
// import {IOrderByDirection} from "../../selectors/TablePanelView/types";
import {IOrdering} from "./IOrderingConfiguration";

export interface IPanelConfiguration {
  position: number | undefined;
  defaultOrdering: IOrdering[] | undefined;
}