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

export type Edge = 'left' | 'right' | 'top' | 'bottom' | 'baseline';

export interface Bounds {
  left: number;
  top: number;
  right: number;
  bottom: number;
  baseline: number | null;
}

export interface SnapTargets {
  container: Bounds | null;
  siblings: Bounds[];
}

export interface SnapLine {
  kind: 'edge' | 'margin' | 'baseline';
  orientation: 'vertical' | 'horizontal';
  position: number;
  from: number;
  to: number;
}

interface Guide {
  edge: Edge;
  offset: number;
  line: SnapLine;
}

export const allEdges: Edge[] = ['left', 'right', 'top', 'bottom', 'baseline'];
export const noSnapTargets: SnapTargets = { container: null, siblings: [] };

const snapDistance = 8;
const controlSpacing = 6;

export function findSnapOffset(bounds: Bounds, targets: SnapTargets, edges: Edge[]) {
  const guides = getGuides(bounds, targets, edges);
  return {
    x: findNearestOffset(guides.filter(guide => isVertical(guide.edge))),
    y: findNearestOffset(guides.filter(guide => !isVertical(guide.edge))),
  };
}

export function findSnapLines(bounds: Bounds, targets: SnapTargets, edges: Edge[]): SnapLine[] {
  return getGuides(bounds, targets, edges)
    .filter(guide => guide.offset === 0)
    .map(guide => guide.line);
}

function findNearestOffset(guides: Guide[]): number {
  const offsets = guides
    .map(guide => guide.offset)
    .filter(offset => Math.abs(offset) <= snapDistance)
    .sort((offsetA, offsetB) => Math.abs(offsetA) - Math.abs(offsetB));
  return offsets[0] ?? 0;
}

function getGuides(bounds: Bounds, targets: SnapTargets, edges: Edge[]): Guide[] {
  const edgeTargets = targets.container
    ? [targets.container, ...targets.siblings]
    : targets.siblings;
  return [
    ...edgeTargets.flatMap(target => getEdgeGuides(bounds, target)),
    ...targets.siblings.flatMap(sibling => getMarginGuides(bounds, sibling)),
    ...targets.siblings.flatMap(sibling => getBaselineGuides(bounds, sibling)),
  ].filter(guide => edges.includes(guide.edge));
}

function getEdgeGuides(bounds: Bounds, target: Bounds): Guide[] {
  const verticalLine = (position: number): SnapLine => ({
    kind: 'edge',
    orientation: 'vertical',
    position,
    from: Math.min(bounds.top, target.top),
    to: Math.max(bounds.bottom, target.bottom),
  });
  const horizontalLine = (position: number): SnapLine => ({
    kind: 'edge',
    orientation: 'horizontal',
    position,
    from: Math.min(bounds.left, target.left),
    to: Math.max(bounds.right, target.right),
  });
  return [
    { edge: 'left', offset: target.left - bounds.left, line: verticalLine(target.left) },
    { edge: 'right', offset: target.right - bounds.right, line: verticalLine(target.right) },
    { edge: 'top', offset: target.top - bounds.top, line: horizontalLine(target.top) },
    { edge: 'bottom', offset: target.bottom - bounds.bottom, line: horizontalLine(target.bottom) },
  ];
}

function getMarginGuides(bounds: Bounds, sibling: Bounds): Guide[] {
  const guides: Guide[] = [];
  const rowMiddle = getOverlapMiddle(bounds.top, bounds.bottom, sibling.top, sibling.bottom);
  if (rowMiddle !== null) {
    guides.push(
      {
        edge: 'left',
        offset: sibling.right + controlSpacing - bounds.left,
        line: marginLine('horizontal', rowMiddle, sibling.right, bounds.left),
      },
      {
        edge: 'right',
        offset: sibling.left - controlSpacing - bounds.right,
        line: marginLine('horizontal', rowMiddle, bounds.right, sibling.left),
      },
    );
  }
  const columnMiddle = getOverlapMiddle(bounds.left, bounds.right, sibling.left, sibling.right);
  if (columnMiddle !== null) {
    guides.push(
      {
        edge: 'top',
        offset: sibling.bottom + controlSpacing - bounds.top,
        line: marginLine('vertical', columnMiddle, sibling.bottom, bounds.top),
      },
      {
        edge: 'bottom',
        offset: sibling.top - controlSpacing - bounds.bottom,
        line: marginLine('vertical', columnMiddle, bounds.bottom, sibling.top),
      },
    );
  }
  return guides;
}

function marginLine(
  orientation: SnapLine['orientation'],
  position: number,
  from: number,
  to: number,
): SnapLine {
  return { kind: 'margin', orientation, position, from, to };
}

function getOverlapMiddle(startA: number, endA: number, startB: number, endB: number) {
  const start = Math.max(startA, startB);
  const end = Math.min(endA, endB);
  return start < end ? Math.round((start + end) / 2) : null;
}

function getBaselineGuides(bounds: Bounds, sibling: Bounds): Guide[] {
  if (bounds.baseline === null || sibling.baseline === null) {
    return [];
  }
  return [
    {
      edge: 'baseline',
      offset: sibling.baseline - bounds.baseline,
      line: {
        kind: 'baseline',
        orientation: 'horizontal',
        position: sibling.baseline,
        from: Math.min(bounds.left, sibling.left),
        to: Math.max(bounds.right, sibling.right),
      },
    },
  ];
}

function isVertical(edge: Edge) {
  return edge === 'left' || edge === 'right';
}
