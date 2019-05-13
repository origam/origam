import { IPropCursor } from "../types/IPropCursor";
import { IPropReorder } from "../types/IPropReorder";
import { IProperties } from "../types/IProperties";


export interface IParentMediator {
  properties: IProperties;
}

export interface ITableViewMediator {
  propCursor: IPropCursor;
  propReorder: IPropReorder;
  properties: IProperties;
  initPropIds: string[];
}

export class TableViewMediator implements ITableViewMediator {
  constructor(public P: {
    initPropIds: string[];
    parentMediator: IParentMediator;
    propCursor: () => IPropCursor;
    propReorder: () => IPropReorder;
  }) {
  }

  get propCursor(): IPropCursor {
    return this.P.propCursor();
  }

  get propReorder(): IPropReorder {
    return this.P.propReorder();
  }

  get initPropIds(): string[] {
    return this.P.initPropIds;
  }

  get properties() {
    return this.P.parentMediator.properties;
  }
}
