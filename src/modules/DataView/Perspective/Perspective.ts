import {TypeSymbol} from "dic/Container";
import {ContribArray} from "utils/common";
import bind from "bind-decorator";

export interface IPerspectiveContrib {
  deactivate(): Generator;
  activateDefault(): Generator;
}

export class Perspective {
  contrib = new ContribArray<IPerspectiveContrib>();

  @bind
  *deactivate() {
    for(let c of this.contrib) {
      yield* c.deactivate();
    }
  }

  @bind
  *activateDefault() {
    for(let c of this.contrib) {
      yield* c.activateDefault();
    }
  }

}

export const IPerspective  = TypeSymbol<Perspective>("IPerspective");