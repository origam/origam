import { unpack } from "../../utils/objects";
import { ML } from "../../utils/types";
import { ASelNextProp } from "../ASelNextProp";
import { ASelNextRec } from "../ASelNextRec";
import { ASelPrevProp } from "../ASelPrevProp";
import { ASelPrevRec } from "../ASelPrevRec";
import { ASelProp } from "../ASelProp";
import { ASelRec } from "../ASelRec";
import { DataView } from "../DataView";
import { PropCursor } from "../PropCursor";
import { PropReorder } from "../PropReorder";
import { IDataView } from "../types/IDataView";
import { IViewType } from "../types/IViewType";
import { AActivateView } from "./AActivateView";
import { AOnEditorClick } from "./AOnEditorClick";
import { AOnEditorKeyDown } from "./AOnEditorKeyDown";
import { AOnNoEditorClick } from "./AOnNoEditorClick";
import { AOnOutsideFormClick } from "./AOnOutsideFormClick";
import { computed } from "mobx";
import { IFormView } from "./types";

export class FormView implements IFormView {
  constructor(
    public P: {
      uiStructure: ML<any>;
      dataView: ML<IDataView>;
    }
  ) {}

  type: IViewType.Form = IViewType.Form;

  init(): void {
    return;
  }

  aOnEditorClick = new AOnEditorClick({});
  aOnNoEditorClick = new AOnNoEditorClick({});
  aOnOutsideFormClick = new AOnOutsideFormClick({});
  aOnEditorKeyDown = new AOnEditorKeyDown({});

  aActivateView = new AActivateView({
    recCursor: () => this.recCursor,
    aSelProp: () => this.aSelProp,
    aStartEditing: () => this.aStartEditing,
    availViews: () => this.availViews
  });

  aSelNextProp = new ASelNextProp({
    props: () => this.propReorder,
    propCursor: () => this.propCursor,
    aSelProp: () => this.aSelProp
  });
  aSelPrevProp = new ASelPrevProp({
    props: () => this.propReorder,
    propCursor: () => this.propCursor,
    aSelProp: () => this.aSelProp
  });
  aSelNextRec = new ASelNextRec({
    records: () => this.records,
    recCursor: () => this.recCursor,
    aSelRec: () => this.aSelRec
  });
  aSelPrevRec = new ASelPrevRec({
    records: () => this.records,
    recCursor: () => this.recCursor,
    aSelRec: () => this.aSelRec
  });

  aSelProp = new ASelProp({
    propCursor: () => this.propCursor,
    properties: () => this.propReorder,
    aReloadChildren: () => this.dataView.aReloadChildren
  });
  aSelRec = new ASelRec({
    aFinishEditing: () => this.aFinishEditing,
    aStartEditing: () => this.aStartEditing,
    editing: () => this.editing,
    propCursor: () => this.propCursor,
    recCursor: () => this.recCursor,
    records: () => this.records
  });

  propReorder = new PropReorder({ props: () => this.props, initPropIds: [] });

  propCursor = new PropCursor({});

  get props() {
    return this.dataView.props;
  }

  get records() {
    return this.dataView.records;
  }

  get recCursor() {
    return this.dataView.recCursor;
  }
  get editing() {
    return this.dataView.editing;
  }

  get aStartEditing() {
    return this.dataView.aStartEditing;
  }

  get aFinishEditing() {
    return this.dataView.aFinishEditing;
  }

  get availViews() {
    return this.dataView.availViews;
  }

  get dataView() {
    return unpack(this.P.dataView);
  }

  @computed get uiStructure() {
    return unpack(this.P.uiStructure);
  }
}
