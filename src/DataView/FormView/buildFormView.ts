import {
  FormViewMediator,
  IFormViewMediator,
  IParentMediator
} from "./FormViewMediator";
import { PropReorder } from "../PropReorder";
import { PropCursor } from "../PropCursor";
import { FormViewMachine } from "./FormViewMachine";
import { IPropReorder } from "../types/IPropReorder";

export function buildFormView(
  parentMediator: IParentMediator,
  initPropIds: string[]
) {
  const mediator: IFormViewMediator = new FormViewMediator({
    initPropIds,
    parentMediator,
    propReorder: () => propReorder,
    propCursor: () => propCursor,
    machine: () => machine
  });

  const propReorder: IPropReorder = new PropReorder(mediator);
  const propCursor = new PropCursor(mediator);
  const machine = new FormViewMachine(mediator);
}
