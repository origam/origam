import { observable, computed, runInAction } from "mobx";

export default 0;

class ValueBox<T> {
  constructor(private value: T) {}

  get() {
    return this.value;
  }
  set(value: T) {
    this.value = value;
  }
}

function computedNoCache<T>(fn: () => T): { get(): T } {
  return {
    get: () => fn(),
  };
}

interface IProperty {
  id: string;
  type: string;
  dataIndex: number;
}

interface IGroupRow {
  level: number;
  columnLabel: string;
  columnValue: string;
  isExpanded: boolean;
}

type ITableRow = any[] | IGroupRow;

interface ICell {
  draw(): void;
  isFixed(): boolean;
  getWidth(): number;
  getText(): string;
}

/*
const tableRows: ITableRow[] = observable(
  [
    [1, 2, 3, "A", "B", "C"],
    [4, 5, 6, "D", "E", "F"],
    [7, 8, 9, "G", "H", "I"],
    [10, 11, 12, "J", "K", "L"],
    { level: 0, childGroups: [], childRows: [] },
    [1, 2, 3, "A", "B", "C"],
    [4, 5, 6, "D", "E", "F"],
    [7, 8, 9, "G", "H", "I"],
    [10, 11, 12, "J", "K", "L"],
    { level: 0, childGroups: [], childRows: [] },
    { level: 1, childGroups: [], childRows: [] },
    { level: 2, childGroups: [], childRows: [] },
    [1, 2, 3, "A", "B", "C"],
    [4, 5, 6, "D", "E", "F"],
    [7, 8, 9, "G", "H", "I"],
    [10, 11, 12, "J", "K", "L"],
  ],
  { deep: false }
);*/

class GroupItem {
  constructor(
    public childGroups: GroupItem[],
    public childRows: any[][],
    public columnLabel: string,
    public groupLabel: string
  ) {}

  @observable isExpanded = false;
}

class TableGroupRow implements IGroupRow {
  constructor(public level: number, public sourceGroup: GroupItem) {}
  get columnLabel(): string {
    return this.sourceGroup.columnLabel;
  }
  get columnValue(): string {
    return this.sourceGroup.groupLabel;
  }
  get isExpanded(): boolean {
    return this.sourceGroup.isExpanded;
  }
}

const rootGroups: GroupItem[] = observable([
  new GroupItem([], [], "Column 1", "Value 1"),
  new GroupItem(
    [
      new GroupItem(
        [],
        [
          [1, 2, 3, "A", "B", "C"],
          [4, 5, 6, "D", "E", "F"],
          [7, 8, 9, "G", "H", "I"],
          [10, 11, 12, "J", "K", "L"],
        ],
        "Column 2",
        "Value 1"
      ),
      new GroupItem([], [], "Column 2", "Value 2"),
      new GroupItem([], [], "Column 2", "Value 3"),
      new GroupItem([], [], "Column 2", "Value 4"),
      new GroupItem([], [], "Column 2", "Value 5"),
    ],
    [],
    "Column 1",
    "Value 2"
  ),
  new GroupItem([], [], "Column 1", "Value 3"),
  new GroupItem([], [], "Column 1", "Value 4"),
  new GroupItem([], [], "Column 1", "Value 5"),
]);

const tableRows = computed<ITableRow[]>(() => {
  const result: ITableRow[] = [];
  let level = 0;
  function recursive(group: GroupItem) {
    result.push(new TableGroupRow(level, group));
    if (!group.isExpanded) return;
    for (let g of group.childGroups) {
      level++;
      recursive(g);
      level--;
    }
    result.push(...group.childRows);
  }
  for (let group of rootGroups) {
    recursive(group);
  }
  console.log(result);
  return result;
});

const fixedColumnCount = observable.box(0);

const properties = observable<IProperty>([
  {
    id: "k",
    type: "Number",
    dataIndex: 0,
  },
  {
    id: "l",
    type: "Number",
    dataIndex: 1,
  },
  {
    id: "m",
    type: "Number",
    dataIndex: 2,
  },
  {
    id: "a",
    type: "Text",
    dataIndex: 3,
  },
  {
    id: "b",
    type: "Text",
    dataIndex: 4,
  },
  {
    id: "c",
    type: "Text",
    dataIndex: 5,
  },
]);

const dataColumnWidths = observable(
  new Map<string, number>([
    ["k", 100],
    ["l", 100],
    ["m", 100],
    ["a", 100],
    ["b", 100],
    ["c", 100],
  ])
);

const tableDataColumnIds = observable<string>(["m", "a", "l", "k", "c", "b"]);

const tableGroupingColumnIds = observable<string>(["k", "l", "m"]);
const isCheckboxedTable = observable.box(false);

const isGroupedView = computed(() => tableGroupingColumnIds.length > 0);

const propertyById = computed(() => new Map(properties.map((property) => [property.id, property])));
const propertyIds = computed(() => properties.map((property) => property.id));

const rowHeight = observable.box(20);

const scrollLeft = observable.box(0);
const scrollTop = observable.box(0);

const viewportWidth = observable.box(500);
const viewportHeight = observable.box(300);

const realFixedColumnCount = computed(() =>
  isCheckboxedTable.get() ? fixedColumnCount.get() + 1 : fixedColumnCount.get()
);

const viewportLeft = computed(() => {
  return scrollLeft.get();
});

const viewportRight = computed(() => {
  return viewportLeft.get() + viewportWidth.get();
});

const viewportTop = computed(() => {
  return scrollTop.get();
});

const viewportBottom = computed(() => {
  return viewportTop.get() + viewportHeight.get();
});

const firstDrawableRowIndex = computed(() => {
  const wpTop = viewportTop.get();
  return Math.floor(wpTop / rowHeight.get());
});

const lastDrawableRowIndex = computed(() => {
  const wpBottom = viewportBottom.get();
  return Math.ceil(wpBottom / rowHeight.get());
});

const leadRowCells = computed<ICell[]>(() => {
  const globIdx = {
    value: 0,
    inc: () => globIdx.value++,
  };
  return [
    ...(isCheckboxedTable.get() ? [createCheckboxCell(globIdx.inc())] : []),
    ...tableGroupingColumnIds.map((id, idx) => createEmptyGroupCell(id, idx, globIdx.inc())),
    ...tableDataColumnIds.map((id, idx) => createDataCell(id, idx, globIdx.inc())),
  ];
});

const leadRowCellsCoords = computed(() => {
  const cells = leadRowCells.get();
  let acc = 0;
  const result = [];
  for (let i = 0; i < cells.length; i++) {
    const width = cells[i].getWidth();
    const left = acc;
    acc = acc + width;
    const right = acc;
    result.push({ width, left, right });
  }
  return result;
});

function isDataRow(row: any): row is any[] {
  return !isGroupRow(row);
}

function isGroupRow(row: any): row is IGroupRow {
  return row.level !== undefined;
}

function getCurrentRow() {
  const idx = currentRowIndexBox.get();
  if (idx > tableRows.get().length - 1) return;
  return tableRows.get()[idx];
}

function isCurrentDataRow() {
  return isDataRow(getCurrentRow());
}

function isCurrentGroupRow() {
  return isGroupRow(getCurrentRow());
}

function getCurrentColumnLeft() {
  return getCurrentRowCellDimensions()[currentColumnIndexBox.get()].left;
}

function getCurrentColumnWidth() {
  return getCurrentRowCellDimensions()[currentColumnIndexBox.get()].width;
}

function getCurrentColumnRight() {
  return getCurrentRowCellDimensions()[currentColumnIndexBox.get()].right;
}

function getRowHeight(rowIndex: number) {
  return rowHeight.get();
}

function getRowTop(rowIndex: number) {
  return getRowHeight(rowIndex) * rowIndex;
}

function getRowBottom(rowIndex: number) {
  return getRowTop(rowIndex) + getRowHeight(rowIndex);
}

function getCurrentRowTop() {
  return getRowTop(currentRowIndexBox.get());
}

function getCurrentRowHeight() {
  return rowHeight.get();
}

function getCurrentRowBottom() {
  return getRowBottom(currentRowIndexBox.get());
}

function getCurrentCellText() {
  return;
}

function applyScrollTranslation(isFixed: boolean) {
  const ctx2d = ctx2dBox.get();
  ctx2d.translate(!isFixed ? -scrollLeft.get() : 0, -scrollTop.get());
}

function clipCell() {
  const ctx2d = ctx2dBox.get();
  ctx2d.beginPath();
  ctx2d.rect(
    getCurrentColumnLeft(),
    getCurrentRowTop(),
    getCurrentColumnWidth(),
    getCurrentRowHeight()
  );
  ctx2d.clip();
}

function drawCheckboxBackground() {
  const ctx2d = ctx2dBox.get();
  ctx2d.fillStyle = "#ffffff";
  ctx2d.fillRect(
    getCurrentColumnLeft(),
    getCurrentRowTop(),
    getCurrentColumnWidth(),
    getCurrentRowHeight()
  );
}

function drawEmptyGroupCellBackground() {
  const ctx2d = ctx2dBox.get();
  ctx2d.fillStyle = "#ffffff";
  ctx2d.fillRect(
    getCurrentColumnLeft(),
    getCurrentRowTop(),
    getCurrentColumnWidth(),
    getCurrentRowHeight()
  );
}

function drawDataCellBackground() {
  const ctx2d = ctx2dBox.get();
  ctx2d.fillStyle = "#ffffff";
  ctx2d.fillRect(
    getCurrentColumnLeft(),
    getCurrentRowTop(),
    getCurrentColumnWidth(),
    getCurrentRowHeight()
  );
}

function drawGroupCellBackground() {
  const ctx2d = ctx2dBox.get();
  ctx2d.fillStyle = "#ffffff";
  ctx2d.fillRect(
    getCurrentColumnLeft(),
    getCurrentRowTop(),
    getCurrentColumnWidth(),
    getCurrentRowHeight()
  );
}

function createCheckboxCell(globIdx: number) {
  return {
    draw() {
      const ctx2d = ctx2dBox.get();
      applyScrollTranslation(true);
      drawCheckboxBackground();
      ctx2d.fillStyle = "black";
      ctx2d.font = '15px "Font Awesome 5 Free"';
      const state = true;
      ctx2d.fillText(
        state ? "\uf14a" : "\uf0c8",
        getCurrentColumnLeft() + 2,
        getCurrentRowTop() + 17
      );
    },
    getText() {
      return "";
    },
    getWidth() {
      return 20;
    },
    isFixed() {
      return true;
    },
  };
}

function createEmptyGroupCell(columnId: string, localIdx: number, globalIdx: number) {
  const isFixed = globalIdx < realFixedColumnCount.get();
  return {
    draw() {
      applyScrollTranslation(isFixed);
      drawEmptyGroupCellBackground();
    },
    getText() {
      return "";
    },
    getWidth() {
      return 20;
    },
    isFixed() {
      return isFixed;
    },
  };
}

function createDataCell(columnId: string, localIdx: number, globalIdx: number) {
  const isFixed = globalIdx < realFixedColumnCount.get();
  const self = {
    draw() {
      const ctx2d = ctx2dBox.get();
      applyScrollTranslation(isFixed);
      clipCell();
      drawDataCellBackground();
      ctx2d.fillStyle = "black";
      ctx2d.font = "12px Arial, sans-serif";
      ctx2d.fillText(
        self.getText() + "dfdsafa dsf dsa f dsa f dsaf  fa dsfas",
        getCurrentColumnLeft() + 2,
        getCurrentRowTop() + 17
      );
    },
    getText() {
      return (getCurrentRow() as any[])[propertyById.get().get(columnId)!.dataIndex];
    },
    getWidth() {
      return dataColumnWidths.get(columnId)!;
    },
    isFixed() {
      return isFixed;
    },
  };
  return self;
}

function createGroupCell(columnId: string, localIdx: number, globalIdx: number): ICell {
  const isFixed = globalIdx < realFixedColumnCount.get();
  const self = {
    draw() {
      const ctx2d = ctx2dBox.get();
      applyScrollTranslation(isFixed);
      clipCell();
      drawGroupCellBackground();
      const row = getCurrentRow() as IGroupRow;
      ctx2d.fillStyle = "black";
      ctx2d.font = '15px "Font Awesome 5 Free"';
      const state = true;
      ctx2d.fillText(
        state ? "\uf146" : "\uf0fe",
        getCurrentColumnLeft() + 2,
        getCurrentRowTop() + 17
      );
      ctx2d.font = "12px Arial, sans-serif";
      ctx2d.fillText(
        `${row.columnLabel} : ${row.columnValue}`,
        getCurrentColumnLeft() + 2 + 20,
        getCurrentRowTop() + 17
      );
    },
    getWidth() {
      const leadCells = leadRowCellsCoords.get();
      return leadCells.slice(-1)[0].right - leadCells[globalIdx].left;
    },
    isFixed() {
      return isFixed;
    },
    getText() {
      return "";
    },
  };
  return self;
}

function createEmptyCheckboxCell(globalIdx: number) {
  const isFixed = globalIdx < realFixedColumnCount.get();
  return {
    draw() {
      applyScrollTranslation(isFixed);
      drawEmptyGroupCellBackground();
    },
    getText() {
      return "";
    },
    getWidth() {
      return 20;
    },
    isFixed() {
      return isFixed;
    },
  };
}

function createCurrentRowCells(): ICell[] {
  const globIdx = {
    value: 0,
    inc: () => globIdx.value++,
  };
  if (isCurrentDataRow()) {
    return [
      ...(isCheckboxedTable.get() ? [createCheckboxCell(globIdx.inc())] : []),
      ...tableGroupingColumnIds.map((id, idx) => createEmptyGroupCell(id, idx, globIdx.inc())),
      ...tableDataColumnIds.map((id, idx) => createDataCell(id, idx, globIdx.inc())),
    ];
  }
  if (isCurrentGroupRow()) {
    return [
      ...(isCheckboxedTable.get() ? [createEmptyCheckboxCell(globIdx.inc())] : []),
      ...createGroupCells(globIdx),
    ];
  }
  return [];
}

function createGroupCells(globIdx: { inc(): number }): ICell[] {
  const row = getCurrentRow() as IGroupRow;
  const result: ICell[] = [];
  for (let idx = 0; idx < tableGroupingColumnIds.length; idx++) {
    const id = tableGroupingColumnIds[idx];
    if (idx < row.level) {
      result.push(createEmptyGroupCell(id, idx, globIdx.inc()));
    } else {
      result.push(createGroupCell(id, idx, globIdx.inc()));
      break;
    }
  }
  return result;
}

let _currentRowCells: ICell[] = null as any;
function getCurrentRowCells() {
  if (!_currentRowCells) {
    _currentRowCells = createCurrentRowCells();
  }
  return _currentRowCells;
}

function createCurrentRowCellDimensions() {
  const cells = getCurrentRowCells();
  let acc = 0;
  const result: { left: number; right: number; width: number }[] = [];
  for (let i = 0; i < cells?.length; i++) {
    const width = cells[i].getWidth();
    const left = acc;
    acc = acc + width;
    const right = acc;
    result.push({
      left,
      right,
      width,
    });
  }
  return result;
}

let _currentRowCellDimensions: { left: number; right: number; width: number }[] = null as any;
function getCurrentRowCellDimensions() {
  if (!_currentRowCellDimensions) {
    _currentRowCellDimensions = createCurrentRowCellDimensions();
  }
  return _currentRowCellDimensions;
}

function getCurrentCell() {
  return getCurrentRowCells()[currentColumnIndexBox.get()];
}

function isCurrentColumnFixed() {
  return getCurrentCell().isFixed();
}

const currentRowIndexBox = new ValueBox(0);
const currentColumnIndexBox = new ValueBox(0);
const ctx2dBox = new ValueBox(null as any);

export function drawTable(ctx2d: CanvasRenderingContext2D) {
  ctx2dBox.set(ctx2d);
  ctx2d.fillStyle = "white";
  ctx2d.fillRect(0, 0, viewportWidth.get(), viewportHeight.get());
  try {
    drawRows();
  } finally {
    ctx2dBox.set(undefined);
  }
}

function drawRows() {
  const rowIdx0 = firstDrawableRowIndex.get();
  const rowIdx1 = lastDrawableRowIndex.get();
  for (let i = rowIdx0; i <= rowIdx1; i++) {
    currentRowIndexBox.set(i);
    try {
      if (!getCurrentRow()) continue;
      drawRow();
    } finally {
      _currentRowCells = null as any;
      _currentRowCellDimensions = null as any;
    }
  }
}

function drawRow() {
  const rowCells = getCurrentRowCells();
  for (let i = rowCells.length - 1; i >= 0; i--) {
    currentColumnIndexBox.set(i);
    if (
      !isCurrentColumnFixed() &&
      !(
        getCurrentColumnLeft() <= viewportRight.get() &&
        getCurrentColumnRight() >= viewportLeft.get()
      )
    ) {
      continue;
    }
    drawCell();
  }
}

function drawCell() {
  const ctx2d = ctx2dBox.get();
  ctx2d.save();
  getCurrentCell().draw();
  ctx2d.restore();
}

export function setScroll(left: number, top: number) {
  runInAction(() => {
    scrollLeft.set(left);
    scrollTop.set(top);
  });
}
