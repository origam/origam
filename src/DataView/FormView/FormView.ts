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
import { IFormView, IFormViewMachine } from "./types";
import { ASelCell } from "../ASelCell";
import { AStartEditing } from "../AStartEditing";
import { AFinishEditing } from "../AFinishEditing";
import { AInitForm } from "../AInitForm";
import { ASubmitForm } from "../ASubmitForm";
import { ADeactivateView } from "./ADeactivateView";
import { IUIFormRoot } from "../../presenter/view/Perspectives/FormView/types";
import { IPropReorder } from "../types/IPropReorder";
import { IPropCursor } from "../types/IPropCursor";
import { IForm } from "../types/IForm";
import { IASelNextProp } from "../types/IASelNextProp";
import { IASelPrevProp } from "../types/IASelPrevProp";
import { IADeactivateView } from "../types/IADeactivateView";
import { IASelProp } from "../types/IASelProp";
import { IAFinishEditing } from "../types/IAFinishEditing";
import { IAInitForm } from "../types/IAInitForm";
import { IASubmitForm } from "../types/IASubmitForm";
import { IASelRec } from "../types/IASelRec";
import { IAStartEditing } from "../types/IAStartEditing";
import { IASelCell } from "../types/IASelCell";
import { IASelNextRec } from "../types/IASelNextRec";
import { IASelPrevRec } from "../types/IASelPrevRec";
import { IAActivateView } from "../types/IAActivateView";
import { FormViewMachine } from "./FormViewMachine";
import { IDataTable } from "../types/IDataTable";

export class FormView implements IFormView {
  constructor(
    public P: {
      uiStructure: ML<any>;
      dataView: ML<IDataView>;
      propIds?: string[];
    }
  ) {

    this.aOnEditorClick = new AOnEditorClick({});
    this.aOnNoEditorClick = new AOnNoEditorClick({});
    this.aOnOutsideFormClick = new AOnOutsideFormClick({});
    this.aOnEditorKeyDown = new AOnEditorKeyDown({});
  
    this.aActivateView = new AActivateView({
      recCursor: () => this.recCursor,
      aSelProp: () => this.aSelProp,
      aStartEditing: () => this.aStartEditing,
      availViews: () => this.availViews,
      machine: () => this.machine
    });
    this.aDeactivateView = new ADeactivateView({
      editing: () => this.editing,
      aFinishEditing: () => this.aFinishEdit,
      machine: () => this.machine
    });
  
    this.aSelNextProp = new ASelNextProp({
      props: () => this.propReorder,
      propCursor: () => this.propCursor,
      aSelProp: () => this.aSelProp
    });
    this.aSelPrevProp = new ASelPrevProp({
      props: () => this.propReorder,
      propCursor: () => this.propCursor,
      aSelProp: () => this.aSelProp
    });
    this.aSelNextRec = new ASelNextRec({
      records: () => this.records,
      recCursor: () => this.recCursor,
      aSelRec: () => this.aSelRec
    });
    this.aSelPrevRec = new ASelPrevRec({
      records: () => this.records,
      recCursor: () => this.recCursor,
      aSelRec: () => this.aSelRec
    });
  
    this.aSelProp = new ASelProp({
      aSelCell: () => this.aSelCell
    });
    this.aSelRec = new ASelRec({
      aSelCell: () => this.aSelCell
    });
  
    this.aSelCell = new ASelCell({
      recCursor: () => this.recCursor,
      propCursor: () => this.propCursor,
      propReorder: () => this.propReorder,
      dataTable: () => this.dataView.dataTable,
      editing: () => this.editing,
      aStartEditing: () => this.aStartEdit,
      aFinishEditing: () => this.aFinishEdit,
      form: () => this.form,
      mediator: this.mediator
    });
  
    this.aStartEdit = new AStartEditing({
      editing: () => this.editing,
      aInitForm: () => this.aInitForm,
      mediator: this.mediator
    });
  
    this.aFinishEdit = new AFinishEditing({
      editing: () => this.editing,
      aSubmitForm: () => this.aSubmitForm,
      mediator: () => this.mediator
    });
  
    this.aInitForm = new AInitForm({
      recCursor: () => this.recCursor,
      dataTable: () => this.dataView.dataTable,
      form: () => this.form
    });
  
    this.aSubmitForm = new ASubmitForm({
      recCursor: () => this.recCursor,
      dataTable: () => this.dataView.dataTable,
      form: () => this.form
    });
  
    this.propReorder = new PropReorder({
      properties: () => this.props,
      initPropIds: this.P.propIds
    });
  
    this.propCursor = new PropCursor({});
    this.machine = new FormViewMachine({
      mediator: () => this.mediator,
      dataTable: () => this.dataTable,
      recCursor: () => this.recCursor,
    });
  }

  type: IViewType.Form = IViewType.Form;

  // TODO: Change to abstractions.
  aOnEditorClick: AOnEditorClick;
  aOnNoEditorClick: AOnNoEditorClick;
  aOnOutsideFormClick: AOnOutsideFormClick;
  aOnEditorKeyDown: AOnEditorKeyDown;
  propReorder: IPropReorder;
  propCursor: IPropCursor;
  aSelNextProp: IASelNextProp;
  aSelPrevProp: IASelPrevProp;
  aDeactivateView: IADeactivateView;
  aSelProp: IASelProp;
  aFinishEdit : IAFinishEditing;
  aInitForm: IAInitForm;
  aSubmitForm: IASubmitForm;
  aSelRec: IASelRec;
  aStartEdit: IAStartEditing;
  aSelCell: IASelCell;
  aSelNextRec: IASelNextRec;
  aSelPrevRec: IASelPrevRec;
  aActivateView: IAActivateView;
  machine: IFormViewMachine;

  init(): void {
    return;
  }


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

  get mediator() {
    return this.dataView.mediator;
  }

  get dataTable() {
    return this.dataView.dataTable;
  }

  @computed get uiStructure() {
    return unpack(this.P.uiStructure);
  }
}

