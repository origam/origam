import { TypeSymbol } from "dic/Container";
import { IPanelViewType } from "model/entities/types/IPanelViewType";

export class ViewConfiguration {
  constructor(
    public saveConfiguration: () => Generator,
    public getActivePerspectiveTag: () => IPanelViewType
  ) {}

  *anounceActivePerspective(tag: string) {
    yield* this.saveConfiguration();
  }

  get activePerspective(): IPanelViewType {
    return this.getActivePerspectiveTag();
  }
}

export const IViewConfiguration = TypeSymbol<ViewConfiguration>("IViewConfiguration");
