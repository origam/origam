
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