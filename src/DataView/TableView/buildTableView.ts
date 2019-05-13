import { TableViewMediator, IParentMediator } from "./TableViewMediator";
import { PropCursor } from "../PropCursor";
import { IPropReorder } from "../types/IPropReorder";
import { PropReorder } from "../PropReorder";
import { IPropCursor } from "../types/IPropCursor";

export function buildTableView(
  initPropIds: string[],
  parentMediator: IParentMediator
) {
  const mediator = new TableViewMediator({
    initPropIds,
    parentMediator,
    propCursor: () => propCursor,
    propReorder: () => propReorder
  });
  const propCursor: IPropCursor = new PropCursor(mediator);
  const propReorder: IPropReorder = new PropReorder(mediator);
}
