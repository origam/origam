import S from "gui/Components/ScreenElements/Table/Table.module.scss";
import * as React from "react";

export function formatTooltipText(content: string | string[] | undefined) {
  if (!content) {
    return "";
  }
  const lines = Array.isArray(content)
    ? content.flatMap((line) => splitToLines(line))
    : splitToLines(content);
  return formatToolTipLines(lines);
}

function splitToLines(value: string) {
  return value.split(/\\r\\n|\\n|<br\/>|<BR\/>/);
}

function formatToolTipLines(content: string[]) {
  const equalLengthLines = content;
  const linesToShow =
    equalLengthLines.length > 10 ? equalLengthLines.slice(0, 9).concat(["..."]) : equalLengthLines;
  return (
    <div className={S.tooltipContent}>
      {linesToShow.map((line) => (
        <div className={S.toolTipLine}>{line}</div>
      ))}
    </div>
  );
}

export function formatTooltipPlaintext(content: string | string[] | undefined) {
  if (!content) return;
  const lines = Array.isArray(content)
    ? content.flatMap((line) => splitToLines(line))
    : splitToLines(content);
  return formatToolTipLinesPlaintext(lines);
}

function formatToolTipLinesPlaintext(content: string[]) {
  const equalLengthLines = content;
  const linesToShow =
    equalLengthLines.length > 10 ? equalLengthLines.slice(0, 9).concat(["..."]) : equalLengthLines;
  return linesToShow.join("\n");
}
