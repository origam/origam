import { Mediator02, IMediator02, IParentMediator } from "./Mediator02";
import { Editing } from "./Editing";
import { DataTable, Records, Properties } from "./DataTable";
import { AvailViews } from "./AvailViews";
import { RecCursor } from "./RecCursor";
import { Form } from "./Form";
import { DataViewMachine } from "./DataViewMachine";
import { IViewType } from "./types/IViewType";
import { IDataTable } from "./types/IDataTable";
import { IDataSource } from "../Screens/types";

export function buildDataView(
  dataStructureEntityId: string,
  dataSource: IDataSource,
  parentMediator: IParentMediator
) {
  const mediator: IMediator02 = new Mediator02({
    parentMediator,
    dataStructureEntityId,
    dataSource,
    editing: () => editing,
    dataTable: () => dataTable,
    availViews: () => availViews,
    recCursor: () => recCursor,
    form: () => form,
    records: () => records,
    properties: () => properties,
    machine: () => machine
  });

  const editing = new Editing({});
  const dataTable: IDataTable = new DataTable(mediator);
  const availViews = new AvailViews({
    items: [],
    initialActiveViewType: IViewType.Table
  });
  const recCursor = new RecCursor({});
  const form = new Form();
  const records = new Records();
  const properties = new Properties({
    items: []
  });
  const machine = new DataViewMachine(mediator);
}
