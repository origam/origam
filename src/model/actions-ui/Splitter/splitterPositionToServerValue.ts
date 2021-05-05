/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/


export function panelSizeRatioToServerValue(sizeRatio: number){
    return Math.round(sizeRatio * 1000_000);
}

export function serverValueToPanelSizeRatio(serverStoredSizeRatio: number | undefined){

    const panelPositionRatio =  serverStoredSizeRatio
        ? serverStoredSizeRatio / 1000_000
        : 0.5;

    if (panelPositionRatio > 1) return 0.5;
    if (panelPositionRatio < 0.1) return 0.1;
    if (panelPositionRatio > 0.9) return 0.9;
    return panelPositionRatio;
}