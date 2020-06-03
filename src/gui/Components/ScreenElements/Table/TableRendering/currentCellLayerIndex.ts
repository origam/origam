import {isCurrentGroupRow} from "./rowCells/groupRowCells";

export function cellLayerCount(){
    return  isCurrentGroupRow() ? 2 : 1;
}

let  _currentCellLayerIndex = 0;

export function setLayerIndex(layerIndex: number){
    _currentCellLayerIndex = layerIndex;
}

export function currentCellLayerIndex(){
    return _currentCellLayerIndex;
}