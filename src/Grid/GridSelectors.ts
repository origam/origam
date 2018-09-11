import { decorate } from "mobx";
import { IGridState, IGridSelectors } from "./types";

export class GridSelectors implements IGridSelectors {
  constructor(public state: IGridState) {}

  get width(): number {
    return this.state.width;
  }

  get height(): number {
    return this.state.height;
  }

  get innerWidth(): number {
    return this.width - 16;
  }

  get innerHeight(): number {
    return this.height - 16;
  }
}

decorate(GridSelectors, {});
