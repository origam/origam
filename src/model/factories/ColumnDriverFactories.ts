import {
  IColumnDriverFactories,
  IColumnDriver,
} from "gui/Components/ScreenElements/Table/CellDrivers/types";

export class ColumnDriverFactories implements IColumnDriverFactories {
  newCheckboxColumnDriver(): IColumnDriver {
    throw new Error("Method not implemented.");
  }

  newDataColumnDriver(columnId: string): IColumnDriver {
    throw new Error("Method not implemented.");
  }

  newNoopColumnDriver(): IColumnDriver {
    throw new Error("Method not implemented.");
  }
  
  newGroupHeaderColumnDriver(columnId: string): IColumnDriver {
    throw new Error("Method not implemented.");
  }
}
