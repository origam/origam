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
  if (!value) {
    return [];
  }
  if (typeof value.split !== 'function') {
    return [value.toString()];
  }
  return value.split(/\\r\\n|\\n|<br\/>|<BR\/>/);
}

function formatToolTipLines(content: string[]) {
  const equalLengthLines = content;
  const linesToShow =
    equalLengthLines.length > 10 ? equalLengthLines.slice(0, 9).concat(["..."]) : equalLengthLines;
  return (
    <div className={S.tooltipContent}>
      {linesToShow.map((line) => (
        <div className={S.tooltipLine}>{line}</div>
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
