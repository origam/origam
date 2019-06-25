import {
  DataViewMediator02,
  IDataViewMediator02,
  IParentMediator
} from "./DataViewMediator02";
import { Editing } from "./Editing";
import { DataTable, Records, Properties } from "./DataTable";
import { AvailViews } from "./AvailViews";
import { RecCursor } from "./RecCursor";
import { Form } from "./Form";
import { DataViewMachine } from "./DataViewMachine";
import { IViewType } from "./types/IViewType";
import { IDataTable } from "./types/IDataTable";
import { IDataSource } from "../Screens/types";
import { IProperty } from "./types/IProperty";
import { ACancelEditing } from "./ACancelEditing";
import { ADeleteRow } from "./ADeleteRow";
import { AFinishEditing } from "./AFinishEditing";
import { AInitForm } from "./AInitForm";
import { AStartEditing } from "./AStartEditing";
import { AReloadChildren } from "./AReloadChildren";
import { AStartView } from "./AStartView";
import { AStopView } from "./AStopView";
import { ASubmitForm } from "./ASubmitForm";
import { ASwitchView } from "./ASwitchView";
import { IFormViewMediator } from "./FormView/FormViewMediator";
import { ITableViewMediator } from "./TableView/TableViewMediator";
import { IASelNextRec } from "./types/IASelNextRec";
import { IASelPrevRec } from "./types/IASelPrevRec";
import { ASelNextRec } from "./ASelNextRec";
import { ASelPrevRec } from "./ASelPrevRec";
import { activateView } from "./DataViewActions";
import { IFormScreen } from "../Screens/FormScreen/types";
import { AFocusEditor } from "./AFocusEditor";

export function buildDataView(
  id: string,
  modelInstanceId: string,
  label: string,
  isHeadless: boolean,
  dataStructureEntityId: string,
  dataSource: IDataSource,
  availViewItems: () => (IFormViewMediator | ITableViewMediator)[],
  propertyItems: () => IProperty[],
  initialActiveViewType: IViewType,
  parentMediator: IFormScreen
) {
  const mediator: IDataViewMediator02 = new DataViewMediator02({
    id,
    modelInstanceId,
    label,
    isHeadless,
    parentMediator,
    dataStructureEntityId,
    dataSource,
    availViewItems,
    propertyItems,
    initialActiveViewType,
    editing: () => editing,
    dataTable: () => dataTable,
    availViews: () => availViews,
    recCursor: () => recCursor,
    form: () => form,
    records: () => records,
    properties: () => properties,
    machine: () => machine,

    aCancelEditing: () => aCancelEditing,
    aDeleteRow: () => aDeleteRow,
    aFinishEditing: () => aFinishEditing,
    aInitForm: () => aInitForm,
    aReloadChildren: () => aReloadChildren,
    aStartEditing: () => aStartEditing,
    aStartView: () => aStartView,
    aStopView: () => aStopView,
    aSubmitForm: () => aSubmitForm,
    aSwitchView: () => aSwitchView,
    aFocusEditor: () => aFocusEditor
  });

  const editing = new Editing({});
  const dataTable: IDataTable = new DataTable(mediator);
  const availViews = new AvailViews(mediator);
  const recCursor = new RecCursor({});
  const form = new Form();
  const records = new Records();
  const properties = new Properties(mediator);
  const machine = new DataViewMachine(mediator);

  const aCancelEditing = new ACancelEditing(mediator);
  const aDeleteRow = new ADeleteRow(mediator);
  const aFinishEditing = new AFinishEditing(mediator);
  const aInitForm = new AInitForm(mediator);
  const aReloadChildren = new AReloadChildren(mediator);
  const aStartEditing = new AStartEditing(mediator);
  const aStartView = new AStartView(mediator);
  const aStopView = new AStopView(mediator);
  const aSubmitForm = new ASubmitForm(mediator);
  const aSwitchView = new ASwitchView(mediator);
  const aFocusEditor = new AFocusEditor(mediator);

  return mediator;
}
