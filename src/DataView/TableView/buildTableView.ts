import { TableViewMediator, ITableViewMediator} from "./TableViewMediator";
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

export function buildTableView(
  initPropIds: string[] | undefined,
  parentMediator: IDataViewMediator02
) {
  const mediator: ITableViewMediator = new TableViewMediator({
    initPropIds,
    parentMediator,
    propCursor: () => propCursor,
    propReorder: () => propReorder,
    aActivateView: () => aActivateView,
    aDeactivateView: () =>aDeactivateView,
    aSelProp: () => aSelProp,
    aSelCell: () => aSelCell
  });
  const propCursor: IPropCursor = new PropCursor(mediator);
  const propReorder: IPropReorder = new PropReorder(mediator);

  const aActivateView: IAActivateView = new AActivateView(mediator);
  const aDeactivateView: IADeactivateView = new ADeactivateView();
  const aSelProp: IASelProp = new ASelProp(mediator);
  const aSelCell: IASelCell = new ASelCell(mediator);
  return mediator;
}
