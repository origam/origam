import { TableViewMediator, ITableViewMediator } from "./TableViewMediator";
import { PropCursor } from "../PropCursor";
import { IPropReorder } from "../types/IPropReorder";
import { PropReorder } from "../PropReorder";
import { IPropCursor } from "../types/IPropCursor";
import { IDataViewMediator02 } from "../DataViewMediator02";
import { IAActivateView } from "../types/IAActivateView";
import { IADeactivateView } from "../types/IADeactivateView";
import { IASelProp } from "../types/IASelProp";
import { ASelProp } from "../ASelProp";
import { AActivateView } from "./AActivateView";
import { ADeactivateView } from "./ADeactivateView";
import { ASelCell } from "../ASelCell";
import { IASelCell } from "../types/IASelCell";
import { ASelNextProp } from "../ASelNextProp";
import { ASelPrevProp } from "../ASelPrevProp";
import { IASelPrevProp } from "../types/IASelPrevProp";
import { IASelNextProp } from "../types/IASelNextProp";
import { ASelNextRec } from "../ASelNextRec";
import { ASelPrevRec } from "../ASelPrevRec";
import { IASelNextRec } from "../types/IASelNextRec";
import { IASelPrevRec } from "../types/IASelPrevRec";
import { ASelRec } from "../ASelRec";
import { IASelRec } from "../types/IASelRec";
import { TableViewMachine } from "./TableViewMachine";
import { ITableViewMachine } from "./types/ITableViewMachine";
import { ISelection } from "../Selection";
import { Selection } from "../Selection";

export function buildTableView(
  initPropIds: string[] | undefined,
  parentMediator: IDataViewMediator02
) {
  const mediator: ITableViewMediator = new TableViewMediator({
    initPropIds,
    parentMediator,
    machine: () => machine,
    propCursor: () => propCursor,
    propReorder: () => propReorder,
    selection: () => selection,
    aActivateView: () => aActivateView,
    aDeactivateView: () => aDeactivateView,
    aSelProp: () => aSelProp,
    aSelCell: () => aSelCell,
    aSelRec: () => aSelRec,
    aSelNextProp: () => aSelNextProp,
    aSelPrevProp: () => aSelPrevProp,
    aSelNextRec: () => aSelNextRec,
    aSelPrevRec: () => aSelPrevRec
  });
  const propCursor: IPropCursor = new PropCursor(mediator);
  const propReorder: IPropReorder = new PropReorder(mediator);
  const machine: ITableViewMachine = new TableViewMachine(mediator);
  const selection: ISelection = new Selection(mediator);
  const aActivateView: IAActivateView = new AActivateView(mediator);
  const aDeactivateView: IADeactivateView = new ADeactivateView();
  const aSelProp: IASelProp = new ASelProp(mediator);
  const aSelCell: IASelCell = new ASelCell(mediator);
  const aSelNextProp: IASelNextProp = new ASelNextProp(mediator);
  const aSelPrevProp: IASelPrevProp = new ASelPrevProp(mediator);
  const aSelNextRec: IASelNextRec = new ASelNextRec(mediator);
  const aSelPrevRec: IASelPrevRec = new ASelPrevRec(mediator);
  const aSelRec: IASelRec = new ASelRec(mediator);
  return mediator;
}
