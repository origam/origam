import {
  FormViewMediator,
  IFormViewMediator,
  IParentMediator
} from "./FormViewMediator";
import { PropReorder } from "../PropReorder";
import { PropCursor } from "../PropCursor";
import { FormViewMachine } from "./FormViewMachine";
import { Selection } from "../Selection";
import { IPropReorder } from "../types/IPropReorder";
import { IDataViewMediator02 } from "../DataViewMediator02";
import { AActivateView } from "./AActivateView";
import { ADeactivateView } from "./ADeactivateView";
import { ASelNextProp } from "../ASelNextProp";
import { ASelPrevProp } from "../ASelPrevProp";
import { ASelProp } from "../ASelProp";
import { ASelCell } from "../ASelCell";
import { ASelNextRec } from "../ASelNextRec";
import { ASelPrevRec } from "../ASelPrevRec";
import { ASelRec } from "../ASelRec";

export function buildFormView(
  initPropIds: string[] | undefined,
  uiStructure: any[],
  parentMediator: IDataViewMediator02
) {
  const mediator: IFormViewMediator = new FormViewMediator({
    initPropIds,
    parentMediator,
    propReorder: () => propReorder,
    propCursor: () => propCursor,
    machine: () => machine,
    selection: () => selection,
    aActivateView: () => aActivateView,
    aDeactivateView: () => aDeactivateView,
    aSelNextProp: () => aSelNextProp,
    aSelPrevProp: () => aSelPrevProp,
    aSelNextRec: () => aSelNextRec,
    aSelPrevRec: () => aSelPrevRec,
    aSelProp: () => aSelProp,
    aSelRec: () => aSelRec,
    aSelCell: () => aSelCell,
    uiStructure
  });

  const propReorder: IPropReorder = new PropReorder(mediator);
  const propCursor = new PropCursor(mediator);
  const machine = new FormViewMachine(mediator);
  const selection = new Selection(mediator);
  const aActivateView = new AActivateView(mediator);
  const aDeactivateView = new ADeactivateView(mediator);
  const aSelNextProp = new ASelNextProp(mediator);
  const aSelPrevProp = new ASelPrevProp(mediator);
  const aSelNextRec = new ASelNextRec(mediator);
  const aSelPrevRec = new ASelPrevRec(mediator);
  const aSelProp = new ASelProp(mediator);
  const aSelRec = new ASelRec(mediator);
  const aSelCell = new ASelCell(mediator);
  return mediator;
}
