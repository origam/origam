import S from "gui/Components/ScreenElements/Table/Table.module.scss";
import * as React from "react";


export function formatTooltipText(content: string | string[] | undefined) {
  if(!content){
    return "";
  }
  let lines: string[]=[];
  if (Array.isArray(content)) {
    lines = content;
  } else {
    lines = content.split("\\r\\n");
    if (lines.length === 1){
      lines = lines[0].split("\\n");
    }
  }
  return formatTooltipLines(lines);
}

function formatTooltipLines(content: string[]) {
  const equalLengthLines = content.flatMap(line => line.match(/.{1,72}/g));
  const linesToShow = equalLengthLines.length > 10
    ? equalLengthLines.slice(0, 9).concat(["..."])
    : equalLengthLines;
  return (
    <div className={S.tooltipContent}>
      {linesToShow.map(line => <div className={S.toolTipLine}>{line}</div>)}
    </div>)
}
