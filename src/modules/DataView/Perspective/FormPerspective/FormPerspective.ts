import {TypeSymbol} from "dic/Container";
import {observable, flow} from "mobx";
import {getIdent, IIId} from "utils/common";
import {IPerspective, IPerspectiveContrib} from "../Perspective";
import bind from "bind-decorator";
import {IViewConfiguration} from "modules/DataView/ViewConfiguration";
import {IPanelViewType} from "model/entities/types/IPanelViewType";

export class FormPerspective implements IIId, IPerspectiveContrib {
  $iid = getIdent();

  constructor(
    public perspective = IPerspective(),
    public viewConfiguration = IViewConfiguration()
  ) {}

  @observable isActive = false;

  @bind
  handleClick(args: {saveNewState: boolean}) {
    const self = this;
    return flow(function* (){
      if (self.isActive) return;
      yield* self.perspective.deactivate();
      self.isActive = true;
      if(args.saveNewState){
        yield* self.viewConfiguration.anounceActivePerspective(IPanelViewType.Form);
      }
    })();
  }

  @bind
  *deactivate() {
    this.isActive = false;
  }

  @bind
  *activateDefault() {
    if (this.viewConfiguration.activePerspective === IPanelViewType.Form) this.isActive = true;
  }
}

export const IFormPerspective = TypeSymbol<FormPerspective>("IFormPerspective");
