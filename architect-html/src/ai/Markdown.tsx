/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

import S from '@/ai/Markdown.module.scss';
import { ReactNode } from 'react';

const INLINE_PATTERN =
  /(`[^`]+`)|(\*\*[^*]+\*\*)|(__[^_]+__)|(\[[^\]]+\]\([^)]+\))|(\*[^*\n]+\*)|(_[^_\n]+_)/g;

const LINK_PATTERN = /^\[([^\]]+)\]\(([^)]+)\)$/;

function isSafeUrl(url: string): boolean {
  return /^(https?:\/\/|mailto:)/i.test(url);
}

function renderInline(text: string, keyPrefix: string): ReactNode[] {
  const nodes: ReactNode[] = [];
  let lastIndex = 0;
  let tokenIndex = 0;
  let match: RegExpExecArray | null;
  INLINE_PATTERN.lastIndex = 0;

  while ((match = INLINE_PATTERN.exec(text)) !== null) {
    if (match.index > lastIndex) {
      nodes.push(text.slice(lastIndex, match.index));
    }
    const token = match[0];
    const key = `${keyPrefix}-${tokenIndex++}`;

    if (token.startsWith('`')) {
      nodes.push(
        <code key={key} className={S.code}>
          {token.slice(1, -1)}
        </code>,
      );
    } else if (token.startsWith('**') || token.startsWith('__')) {
      nodes.push(<strong key={key}>{token.slice(2, -2)}</strong>);
    } else if (token.startsWith('[')) {
      const linkMatch = LINK_PATTERN.exec(token);
      if (linkMatch && isSafeUrl(linkMatch[2])) {
        nodes.push(
          <a key={key} href={linkMatch[2]} target="_blank" rel="noreferrer noopener">
            {linkMatch[1]}
          </a>,
        );
      } else {
        nodes.push(token);
      }
    } else {
      nodes.push(<em key={key}>{token.slice(1, -1)}</em>);
    }
    lastIndex = INLINE_PATTERN.lastIndex;
  }

  if (lastIndex < text.length) {
    nodes.push(text.slice(lastIndex));
  }
  return nodes;
}

function renderLines(lines: string[], keyPrefix: string): ReactNode[] {
  return lines.flatMap((line, lineIndex) => {
    const rendered = renderInline(line, `${keyPrefix}-${lineIndex}`);
    return lineIndex < lines.length - 1
      ? [...rendered, <br key={`${keyPrefix}-br-${lineIndex}`} />]
      : rendered;
  });
}

type CellAlignment = 'left' | 'center' | 'right' | undefined;

function parseTableRow(line: string): string[] {
  let trimmed = line.trim();
  if (trimmed.startsWith('|')) {
    trimmed = trimmed.slice(1);
  }
  if (trimmed.endsWith('|')) {
    trimmed = trimmed.slice(0, -1);
  }
  return trimmed.split('|').map(cell => cell.trim());
}

function isTableSeparator(line: string): boolean {
  const trimmed = line.trim();
  if (!trimmed.includes('-')) {
    return false;
  }
  const cells = parseTableRow(trimmed);
  return cells.length > 0 && cells.every(cell => /^:?-+:?$/.test(cell));
}

function isTableStart(lines: string[], index: number): boolean {
  const line = lines[index];
  return line.includes('|') && index + 1 < lines.length && isTableSeparator(lines[index + 1]);
}

function parseAlignment(cell: string): CellAlignment {
  const alignLeft = cell.startsWith(':');
  const alignRight = cell.endsWith(':');
  if (alignLeft && alignRight) {
    return 'center';
  }
  if (alignRight) {
    return 'right';
  }
  if (alignLeft) {
    return 'left';
  }
  return undefined;
}

function alignStyle(alignment: CellAlignment) {
  return alignment ? { textAlign: alignment } : undefined;
}

export function Markdown({ text }: { text: string }) {
  const lines = text.replace(/\r\n/g, '\n').split('\n');
  const blocks: ReactNode[] = [];
  let index = 0;
  let blockKey = 0;

  while (index < lines.length) {
    const line = lines[index];

    if (line.trim().length === 0) {
      index += 1;
      continue;
    }

    const fenceMatch = /^```(.*)$/.exec(line.trim());
    if (fenceMatch) {
      const codeLines: string[] = [];
      index += 1;
      while (index < lines.length && !/^```/.test(lines[index].trim())) {
        codeLines.push(lines[index]);
        index += 1;
      }
      index += 1;
      blocks.push(
        <pre key={`block-${blockKey++}`} className={S.pre}>
          <code>{codeLines.join('\n')}</code>
        </pre>,
      );
      continue;
    }

    const headingMatch = /^(#{1,6})\s+(.*)$/.exec(line);
    if (headingMatch) {
      const level = Math.min(headingMatch[1].length, 4);
      const HeadingTag = `h${level}` as 'h1' | 'h2' | 'h3' | 'h4';
      blocks.push(
        <HeadingTag key={`block-${blockKey++}`} className={S.heading}>
          {renderInline(headingMatch[2], `h-${blockKey}`)}
        </HeadingTag>,
      );
      index += 1;
      continue;
    }

    if (/^\s*([-*+])\s+/.test(line)) {
      const items: string[] = [];
      while (index < lines.length && /^\s*([-*+])\s+/.test(lines[index])) {
        items.push(lines[index].replace(/^\s*([-*+])\s+/, ''));
        index += 1;
      }
      blocks.push(
        <ul key={`block-${blockKey++}`} className={S.list}>
          {items.map((item, itemIndex) => (
            <li key={itemIndex}>{renderInline(item, `ul-${blockKey}-${itemIndex}`)}</li>
          ))}
        </ul>,
      );
      continue;
    }

    if (/^\s*\d+\.\s+/.test(line)) {
      const items: string[] = [];
      while (index < lines.length && /^\s*\d+\.\s+/.test(lines[index])) {
        items.push(lines[index].replace(/^\s*\d+\.\s+/, ''));
        index += 1;
      }
      blocks.push(
        <ol key={`block-${blockKey++}`} className={S.list}>
          {items.map((item, itemIndex) => (
            <li key={itemIndex}>{renderInline(item, `ol-${blockKey}-${itemIndex}`)}</li>
          ))}
        </ol>,
      );
      continue;
    }

    if (/^\s*>\s?/.test(line)) {
      const quoteLines: string[] = [];
      while (index < lines.length && /^\s*>\s?/.test(lines[index])) {
        quoteLines.push(lines[index].replace(/^\s*>\s?/, ''));
        index += 1;
      }
      blocks.push(
        <blockquote key={`block-${blockKey++}`} className={S.quote}>
          {renderLines(quoteLines, `quote-${blockKey}`)}
        </blockquote>,
      );
      continue;
    }

    if (/^\s*([-*_])\1{2,}\s*$/.test(line)) {
      blocks.push(<hr key={`block-${blockKey++}`} className={S.rule} />);
      index += 1;
      continue;
    }

    if (isTableStart(lines, index)) {
      const headerCells = parseTableRow(lines[index]);
      const alignments = parseTableRow(lines[index + 1]).map(parseAlignment);
      index += 2;
      const bodyRows: string[][] = [];
      while (index < lines.length && lines[index].trim().length > 0 && lines[index].includes('|')) {
        bodyRows.push(parseTableRow(lines[index]));
        index += 1;
      }
      const tableKey = blockKey++;
      blocks.push(
        <table key={`block-${tableKey}`} className={S.table}>
          <thead>
            <tr>
              {headerCells.map((cell, cellIndex) => (
                <th key={cellIndex} style={alignStyle(alignments[cellIndex])}>
                  {renderInline(cell, `th-${tableKey}-${cellIndex}`)}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {bodyRows.map((row, rowIndex) => (
              <tr key={rowIndex}>
                {headerCells.map((_, cellIndex) => (
                  <td key={cellIndex} style={alignStyle(alignments[cellIndex])}>
                    {renderInline(row[cellIndex] ?? '', `td-${tableKey}-${rowIndex}-${cellIndex}`)}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>,
      );
      continue;
    }

    const paragraphLines: string[] = [];
    while (
      index < lines.length &&
      lines[index].trim().length > 0 &&
      !/^```/.test(lines[index].trim()) &&
      !/^(#{1,6})\s+/.test(lines[index]) &&
      !/^\s*([-*+])\s+/.test(lines[index]) &&
      !/^\s*\d+\.\s+/.test(lines[index]) &&
      !/^\s*>\s?/.test(lines[index]) &&
      !isTableStart(lines, index)
    ) {
      paragraphLines.push(lines[index]);
      index += 1;
    }
    blocks.push(
      <p key={`block-${blockKey++}`} className={S.paragraph}>
        {renderLines(paragraphLines, `p-${blockKey}`)}
      </p>,
    );
  }

  return <div className={S.markdown}>{blocks}</div>;
}
