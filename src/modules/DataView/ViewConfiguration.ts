import {TypeSymbol} from "dic/Container";
import {IPanelViewType} from "model/entities/types/IPanelViewType";

export class ViewConfiguration {
  constructor(
    public saveConfiguration: (activePerspectiveTag: string) => Generator,
    public getActivePerspectiveTag: () => IPanelViewType
  ) {}

  *anounceActivePerspective(tag: string) {
    yield* this.saveConfiguration(tag);
  }

  get activePerspective(): IPanelViewType {
    return this.getActivePerspectiveTag();
  }
}

export const IViewConfiguration = TypeSymbol<ViewConfiguration>("IViewConfiguration");
