import {
    context2d,
    currentRow,
    recordId,
    tableColumnIds,
    tablePanelView
} from "../renderingValues";
import {applyScrollTranslation, numberCellPaddingLeft, clipCell, topTextOffset, fontSize} from "./cellsCommon";
import {getRowStateAllowRead} from "../../../../../../model/selectors/RowState/getRowStateAllowRead";
import {
    currentColumnId,
    currentColumnLeft,
    currentColumnWidth,
    currentProperty,
    currentRowTop
} from "../currentCell";
import {CPR} from "../../../../../../utils/canvas";
import {isGroupRow} from "../rowCells/groupRowCells";
import {IGroupRow} from "../types";
import {dataColumnsWidths} from "./dataCell";
import {aggregationToString} from "../../../../../../model/entities/types/IAggregation";



export function aggregationColumnsWidths() {
    return  dataColumnsWidths();
}

export function aggregationCellDraws() {
    const row = currentRow();
    if (isGroupRow(row)) {
        return tableColumnIds().map((id) => () => drawAggregationCell());
    } else return [];
}

export function drawAggregationCell() {
    applyScrollTranslation();
    clipCell();
    drawAggregationText();
}

function drawAggregationText(){
    const ctx2d = context2d();
    if(!currentColumnId()) return;
    const isHidden = !getRowStateAllowRead(tablePanelView(), recordId(), currentProperty().id)

    const row = currentRow();
    if (isHidden || !isGroupRow(row))
    {
        return;
    }
    const groupRow = row as IGroupRow;
    if(!groupRow.sourceGroup.aggregations)
    {
        return;
    }
    const aggregation = groupRow.sourceGroup.aggregations
        .find(aggregation => aggregation.columnId === currentProperty().id)
    if(!aggregation)
    {
        return;
    }

    ctx2d.font = `${fontSize * CPR}px "IBM Plex Sans", sans-serif`;
    ctx2d.fillStyle = "black";
    ctx2d.textAlign = "right";
    ctx2d.fillText(
      aggregationToString(aggregation) ,
        CPR * (currentColumnLeft() + currentColumnWidth() - numberCellPaddingLeft()),
        CPR * (currentRowTop() + topTextOffset));
}
