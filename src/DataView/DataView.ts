import { Editing } from "./Editing";
import { Form } from "./Form";
import { RecCursor } from "./RecCursor";
import { ASwitchView } from "./ASwitchView";
import { DataTable, Properties, Records } from "./DataTable";
import { AStartEditing } from "./AStartEditing";
import { AFinishEditing } from "./AFinishEditing";
import { ACancelEditing } from "./ACancelEditing";
import { AInitForm } from "./AInitForm";
import { ASubmitForm } from "./ASubmitForm";
import { AvailViews } from "./AvailViews";
import { TableView } from "./TableView/TableView";
import { FormView } from "./FormView/FormView";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { IDataView } from "./types/IDataView";
import { IViewType } from "./types/IViewType";
import { ITableView } from "./TableView/ITableView";
import { IFormView } from "./FormView/types";
import { AStartView } from "./AStartView";
import { DataViewMachine } from "./DataViewMachine";
import { AStopView } from "./AStopView";
import { IApi } from "../Api/IApi";
import { IProperties } from "./types/IProperties";
import { IDataSource } from "../Screens/types";
import { ASelCell } from "./ASelCell";
import { DataViewMediator } from "./DataViewMediator";
import { IDataViewMediator } from "./types/IDataViewMediator";
import { IDataViewMachine } from "./types/IDataViewMachine";
import { AReloadChildren } from "./AReloadChildren";
import { IAvailViews } from "./types/IAvailViews";
import { IRecCursor } from "./types/IRecCursor";
import { IForm } from "./types/IForm";
import { IEditing } from "./types/IEditing";
import { IDataTable } from "./types/IDataTable";
import { IRecords } from "./types/IRecords";
import { IAStartView } from "./types/IAStartView";
import { IAStopView } from "./types/IAStopView";
import { IAStartEditing } from "./types/IAStartEditing";
import { IAFinishEditing } from "./types/IAFinishEditing";
import { IACancelEditing } from "./types/IACancelEditing";
import { IASwitchView } from "./types/IASwitchView";
import { IAInitForm } from "./types/IAInitForm";
import { IASubmitForm } from "./types/IASubmitForm";
import { IAReloadChildren } from "./types/IAReloadChildren";

export class DataView implements IDataView {
  constructor(
    public P: {
      id: ML<string>;
      menuItemId: ML<string>;
      dataStructureEntityId: ML<string>;
      initialDataView: ML<IViewType>;
      isHeadless: ML<boolean>;
      specificDataViews: ML<Array<FormView | TableView>>;
      properties: ML<IProperties>;
      dataSource: ML<IDataSource>;
      mediator: IDataViewMediator;
      api: ML<IApi>;
    }
  ) {
    this.mediator = P.mediator;

    this.availViews = new AvailViews({
      items: () => this.specificDataViews,
      initialActiveViewType: () => this.initialDataView
    });

    this.recCursor = new RecCursor({});

    this.form = new Form();
    this.editing = new Editing({});

    this.dataTable = new DataTable({
      records: () => this.records,
      properties: () => this.props
    });

    this.records = new Records();

    this.aStartView = new AStartView({ machine: () => this.machine });

    this.aStopView = new AStopView({ machine: () => this.machine });

    this.aStartEditing = new AStartEditing({
      editing: () => this.editing,
      aInitForm: () => this.aInitForm,
      mediator: this.mediator
    });
    this.aFinishEditing = new AFinishEditing({
      editing: () => this.editing,
      aSubmitForm: () => this.aSubmitForm,
      mediator: () => this.mediator
    });
    this.aCancelEditing = new ACancelEditing({
      editing: () => this.editing,
      form: () => this.form
    });

    this.aSwitchView = new ASwitchView({
      availViews: () => this.availViews
    });

    this.aInitForm = new AInitForm({
      recCursor: () => this.recCursor,
      dataTable: () => this.dataTable,
      form: () => this.form
    });
    this.aSubmitForm = new ASubmitForm({
      recCursor: () => this.recCursor,
      dataTable: () => this.dataTable,
      form: () => this.form
    });
    this.aReloadChildren = new AReloadChildren({
      dataViewMachine: () => this.machine
    });

    this.machine = new DataViewMachine({
      api: this.P.api,
      menuItemId: this.P.menuItemId,
      dataStructureEntityId: this.P.dataStructureEntityId,
      propertyIdsToLoad: () => this.props.ids,
      dataTable: () => this.dataTable,
      dataSource: this.P.dataSource,
      mediator: this.mediator,
      selectedIdGetter: () => {
        // console.log("selIdGetter:", this.recCursor.selId);
        return this.recCursor.selId;
      }
    });
  }

  machine: IDataViewMachine;
  mediator: IDataViewMediator;
  availViews: IAvailViews;
  recCursor: IRecCursor;
  form: IForm;
  editing: IEditing;
  dataTable: IDataTable;
  records: IRecords;
  aStartView: IAStartView;
  aStopView: IAStopView;
  aStartEditing: IAStartEditing;
  aFinishEditing: IAFinishEditing;
  aCancelEditing: IACancelEditing;
  aSwitchView: IASwitchView;
  aInitForm: IAInitForm;
  aSubmitForm: IASubmitForm;
  aReloadChildren: IAReloadChildren;

  get isHeadless() {
    return unpack(this.P.isHeadless);
  }

  get props() {
    return unpack(this.P.properties);
  }

  get specificDataViews() {
    return unpack(this.P.specificDataViews);
  }

  get initialDataView() {
    return unpack(this.P.initialDataView);
  }

  get id() {
    return unpack(this.P.id);
  }
}
