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
import { ASelCell } from "../ASelCell";
import { AStartEditing } from "../AStartEditing";
import { AFinishEditing } from "../AFinishEditing";
import { AInitForm } from "../AInitForm";
import { ASubmitForm } from "../ASubmitForm";
import { ADeactivateView } from "./ADeactivateView";

export class FormView implements IFormView {
  constructor(
    public P: {
      uiStructure: ML<any>;
      dataView: ML<IDataView>;
      propIds?: string[];
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
  aDeactivateView = new ADeactivateView({
    editing: () => this.editing,
    aFinishEditing: () => this.aFinishEdit
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
    aSelCell: () => this.aSelCell
  });
  aSelRec = new ASelRec({
    aSelCell: () => this.aSelCell
  })

  aSelCell = new ASelCell({
    recCursor: () => this.recCursor,
    propCursor: () => this.propCursor,
    propReorder: () => this.propReorder,
    dataTable: () => this.dataView.dataTable,
    editing: () => this.editing,
    aStartEditing: () => this.aStartEdit,
    aFinishEditing: () => this.aFinishEdit,
    form: () => this.form
  });

  aStartEdit = new AStartEditing({
    editing: () => this.editing,
    aInitForm: () => this.aInitForm,
  })

  aFinishEdit = new AFinishEditing({
    editing: () => this.editing,
    aSubmitForm: () => this.aSubmitForm
  });

  aInitForm = new AInitForm({
    recCursor: () => this.recCursor,
    dataTable: () => this.dataView.dataTable,
    form: () => this.form
  });

  aSubmitForm = new ASubmitForm({
    recCursor: () => this.recCursor,
    dataTable: () => this.dataView.dataTable,
    form: () => this.form
  })
  
  propReorder = new PropReorder({
    props: () => this.props,
    initPropIds: this.P.propIds
  });

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

  get form() {
    return this.dataView.form;
  }

  @computed get uiStructure() {
    return unpack(this.P.uiStructure);
  }
}
