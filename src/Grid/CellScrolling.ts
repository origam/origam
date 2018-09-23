import {
  action,
  reaction,
  trace,
  when,
  computed,
  IReactionDisposer
} from "mobx";
import { reactionRuntimeInfo } from "../utils/reaction";
import {
  IGridSelectors,
  IGridActions,
  IGridTopology,
  IGridSetup,
  IGridInteractionSelectors
} from "./types";

export class CellScrolling {
  constructor(
    public gridViewSelectors: IGridSelectors,
    public gridViewActions: IGridActions,
    public gridTopology: IGridTopology,
    public gridSetup: IGridSetup,
    public gridInteractionSelectors: IGridInteractionSelectors
  ) {}

  @computed
  private get isCellSelected() {
    return this.gridInteractionSelectors.isCellSelected;
  }

  private isFixedColumn(columnId: string) {
    const columnIndex = this.gridTopology.getColumnIndexById(columnId);
    const isFixedColumn = this.gridSetup.isFixedColumn(columnIndex);
    return isFixedColumn;
  }

  private getColumnLeft(columnId: string) {
    return this.gridSetup.getColumnLeft(
      this.gridTopology.getColumnIndexById(columnId)
    );
  }

  private getColumnRight(columnId: string) {
    return this.gridSetup.getColumnRight(
      this.gridTopology.getColumnIndexById(columnId)
    );
  }

  private getRowTop(rowId: string) {
    return this.gridSetup.getRowTop(this.gridTopology.getRowIndexById(rowId));
  }

  private getRowBottom(rowId: string) {
    return this.gridSetup.getRowBottom(
      this.gridTopology.getRowIndexById(rowId)
    );
  }

  @computed
  private get fixedColumnsTotalWidth() {
    return this.gridViewSelectors.fixedColumnsTotalWidth;
  }

  @computed
  private get gridVisibleWidth() {
    return this.gridViewSelectors.innerWidth;
  }

  @computed
  private get gridVisibleHeight() {
    return this.gridViewSelectors.innerHeight;
  }

  @computed
  private get scrollTop() {
    return this.gridViewSelectors.scrollTop;
  }

  @computed
  private get scrollLeft() {
    return this.gridViewSelectors.scrollLeft;
  }

  @computed
  private get selectedColumnId() {
    return this.gridInteractionSelectors.selectedColumnId;
  }

  @computed
  private get selectedRowId() {
    return this.gridInteractionSelectors.selectedRowId;
  }

  private reScrollToCell: IReactionDisposer | undefined;

  private postponedReactionScope = new Set();
  @action.bound
  private startScrollerReaction() {
    if (this.reScrollToCell) {
      return;
    }
    this.reScrollToCell = reaction(
      () => {
        return [this.selectedRowId, this.selectedColumnId, this.isCellSelected];
      },
      () => {
        /*console.log(
          "REACTION SCOPE IS:",
          Array.from(this.postponedReactionScope.values())
        );*/
        if (this.isCellSelected && this.postponedReactionScope.has("UI")) {
          const top = this.getRowTop(this.selectedRowId!);
          const bottom = this.getRowBottom(this.selectedRowId!);
          if (!this.isFixedColumn(this.selectedColumnId!)) {
            const left = this.getColumnLeft(this.selectedColumnId!);
            const right = this.getColumnRight(this.selectedColumnId!);

            if (left - this.scrollLeft < this.fixedColumnsTotalWidth) {
              this.gridViewActions.performScrollTo({
                scrollLeft: left - this.fixedColumnsTotalWidth
              });
            }
            if (right - this.scrollLeft > this.gridVisibleWidth) {
              this.gridViewActions.performScrollTo({
                scrollLeft: right - this.gridVisibleWidth
              });
            }
          }
          if (top - this.scrollTop < 0) {
            this.gridViewActions.performScrollTo({ scrollTop: top });
          }
          if (bottom - this.scrollTop > this.gridVisibleHeight) {
            this.gridViewActions.performScrollTo({
              scrollTop: bottom - this.gridVisibleHeight
            });
          }
        }
        if (
          this.isCellSelected &&
          this.postponedReactionScope.has("LOAD_FRESH_AROUND")
        ) {
          const top = this.getRowTop(this.selectedRowId!);
          this.gridViewActions.performScrollTo({
            scrollTop: top - this.gridVisibleHeight / 2
          });
        }
      },
      {
        fireImmediately: true,
        scheduler: fn => {
          this.postponedReactionScope = new Set(reactionRuntimeInfo.info);
          if (this.postponedReactionScope.has("UI")) {
            fn();
          } else {
            setTimeout(fn, 10);
          }
        }
      }
    );
  }

  private reScrollable: IReactionDisposer | undefined;

  @action.bound
  private startScrollableReaction() {
    if (this.reScrollable) {
      return;
    }
    this.reScrollable = reaction(
      () => this.gridVisibleWidth > 20 && this.gridVisibleHeight > 20,
      isScrollable =>
        isScrollable
          ? this.startScrollerReaction()
          : this.stopScrollerReaction(),
      {
        fireImmediately: true
      }
    );
  }

  @action.bound
  private stopScrollerReaction() {
    this.reScrollToCell && this.reScrollToCell();
    this.reScrollToCell = undefined;
  }

  @action.bound
  private stopScrollableReaction() {
    this.reScrollable && this.reScrollable();
    this.reScrollable = undefined;
  }

  @action.bound
  public start() {
    this.startScrollableReaction();
  }

  @action.bound
  public stop() {
    this.stopScrollableReaction();
    this.startScrollerReaction();
  }
}
