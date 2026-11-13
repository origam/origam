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

export type Edge = 'left' | 'right' | 'top' | 'bottom';

type Orientation = 'vertical' | 'horizontal';

export interface Bounds {
  left: number;
  top: number;
  right: number;
  bottom: number;
}

export interface SnapTargets {
  container: Bounds | null;
  siblings: Bounds[];
}

export interface SnapLine {
  kind: 'alignment' | 'spacing' | 'size';
  orientation: Orientation;
  position: number;
  from: number;
  to: number;
}

interface MinSize {
  width: number;
  height: number;
}

interface Axis {
  start: Edge;
  end: Edge;
  crossStart: Edge;
  crossEnd: Edge;
  along: Orientation;
  across: Orientation;
  size: keyof MinSize;
}

interface Alignment {
  offset: number;
  position: number;
  distance: number;
  target: Bounds;
}

interface Spacing {
  offset: number;
  lines: SnapLine[];
}

interface SpacingCandidate {
  start: number;
  gap: number;
}

interface ResizeMatch {
  kind: 'alignment' | 'size';
  axis: Axis;
  edge: Edge;
  value: number;
  difference: number;
  target: Bounds;
}

export const noSnapTargets: SnapTargets = { container: null, siblings: [] };

const alignmentTolerance = 5;
const spacingTolerance = alignmentTolerance / 2;
const minSpacing = 4;

const horizontalAxis: Axis = {
  start: 'left',
  end: 'right',
  crossStart: 'top',
  crossEnd: 'bottom',
  along: 'horizontal',
  across: 'vertical',
  size: 'width',
};

const verticalAxis: Axis = {
  start: 'top',
  end: 'bottom',
  crossStart: 'left',
  crossEnd: 'right',
  along: 'vertical',
  across: 'horizontal',
  size: 'height',
};

const axes = [horizontalAxis, verticalAxis];

export function snapMove(bounds: Bounds, targets: SnapTargets) {
  const horizontalSpacing = findSpacing(bounds, targets.siblings, horizontalAxis);
  const verticalSpacing = findSpacing(bounds, targets.siblings, verticalAxis);
  const spaced = shift(
    shift(bounds, horizontalAxis, horizontalSpacing?.offset ?? 0),
    verticalAxis,
    verticalSpacing?.offset ?? 0,
  );
  const alignmentTargets = getAlignmentTargets(targets);
  const horizontalAlignment = horizontalSpacing
    ? findExactAlignment(spaced, alignmentTargets, horizontalAxis)
    : findAlignment(spaced, alignmentTargets, horizontalAxis);
  const verticalAlignment = verticalSpacing
    ? findExactAlignment(spaced, alignmentTargets, verticalAxis)
    : findAlignment(spaced, alignmentTargets, verticalAxis);
  const x = (horizontalSpacing?.offset ?? 0) + (horizontalAlignment?.offset ?? 0);
  const y = (verticalSpacing?.offset ?? 0) + (verticalAlignment?.offset ?? 0);
  const snapped = shift(shift(bounds, horizontalAxis, x), verticalAxis, y);
  const lines: SnapLine[] = [
    ...(horizontalSpacing?.lines ?? []),
    ...(verticalSpacing?.lines ?? []),
  ];
  if (horizontalAlignment) {
    lines.push(
      toAlignmentLine(
        horizontalAlignment.position,
        horizontalAlignment.target,
        snapped,
        horizontalAxis,
      ),
    );
  }
  if (verticalAlignment) {
    lines.push(
      toAlignmentLine(verticalAlignment.position, verticalAlignment.target, snapped, verticalAxis),
    );
  }
  return { x, y, lines };
}

export function snapResize(
  bounds: Bounds,
  movingEdges: Edge[],
  targets: SnapTargets,
  minSize: MinSize,
) {
  const matches = axes.flatMap(axis => {
    const edge = movingEdges.find(
      movingEdge => movingEdge === axis.start || movingEdge === axis.end,
    );
    const match = edge && findResizeMatch(bounds, edge, axis, targets, minSize[axis.size]);
    return match ? [match] : [];
  });
  const snapped = matches.reduce<Bounds>(
    (result, match) => ({ ...result, [match.edge]: match.value }),
    bounds,
  );
  return { bounds: snapped, lines: matches.flatMap(match => toResizeLines(match, snapped)) };
}

function getAlignmentTargets(targets: SnapTargets): Bounds[] {
  return targets.container ? [targets.container, ...targets.siblings] : targets.siblings;
}

function findExactAlignment(moved: Bounds, targets: Bounds[], axis: Axis): Alignment | null {
  const alignment = findAlignment(moved, targets, axis);
  return alignment?.offset === 0 ? alignment : null;
}

function findAlignment(moved: Bounds, targets: Bounds[], axis: Axis): Alignment | null {
  let best: Alignment | null = null;
  for (const target of targets) {
    for (const [targetValue, movedValue] of getAlignmentPairs(moved, target, axis)) {
      const distance = Math.abs(targetValue - movedValue);
      if (isBetterAlignment(distance, target, best, moved)) {
        best = {
          offset: Math.round(targetValue - movedValue),
          position: Math.round(targetValue),
          distance,
          target,
        };
      }
    }
  }
  return best;
}

function getAlignmentPairs(moved: Bounds, target: Bounds, axis: Axis): [number, number][] {
  return [
    [centerOf(target, axis), centerOf(moved, axis)],
    [target[axis.start], moved[axis.start]],
    [target[axis.start], moved[axis.end]],
    [target[axis.end], moved[axis.start]],
    [target[axis.end], moved[axis.end]],
  ];
}

function isBetterAlignment(
  distance: number,
  target: Bounds,
  best: Alignment | null,
  moved: Bounds,
) {
  if (!best) {
    return distance < alignmentTolerance;
  }
  return (
    distance < best.distance ||
    (distance === best.distance &&
      getCenterDistance(target, moved) < getCenterDistance(best.target, moved))
  );
}

function toAlignmentLine(position: number, target: Bounds, snapped: Bounds, axis: Axis): SnapLine {
  return {
    kind: 'alignment',
    orientation: axis.across,
    position,
    from: Math.min(snapped[axis.crossStart], target[axis.crossStart]),
    to: Math.max(snapped[axis.crossEnd], target[axis.crossEnd]),
  };
}

function findSpacing(moved: Bounds, siblings: Bounds[], axis: Axis): Spacing | null {
  const row = siblings.filter(sibling => isInRow(moved, sibling, axis));
  const before = row.filter(sibling => sibling[axis.end] < moved[axis.start]);
  const after = row.filter(sibling => sibling[axis.start] > moved[axis.end]);
  const candidate = getSpacingCandidates(moved, before, after, axis)
    .filter(
      spacingCandidate => Math.abs(spacingCandidate.start - moved[axis.start]) <= spacingTolerance,
    )
    .sort(
      (first, second) =>
        Math.abs(first.start - moved[axis.start]) - Math.abs(second.start - moved[axis.start]),
    )[0];
  if (!candidate) {
    return null;
  }
  const offset = Math.round(candidate.start - moved[axis.start]);
  const snapped = shift(moved, axis, offset);
  const chain = getEqualGapChain(snapped, before, after, candidate.gap, axis);
  const firstFixed = chain[0] === snapped ? chain[1] : chain[0];
  return {
    offset,
    lines: chain.slice(1).map((cell, index) => ({
      kind: 'spacing',
      orientation: axis.along,
      position: firstFixed[axis.crossEnd],
      from: chain[index][axis.end],
      to: cell[axis.start],
    })),
  };
}

function isInRow(moved: Bounds, sibling: Bounds, axis: Axis) {
  const overlapsAcross =
    moved[axis.crossStart] <= sibling[axis.crossEnd] &&
    sibling[axis.crossStart] <= moved[axis.crossEnd];
  const isApart =
    moved[axis.start] > sibling[axis.end] + minSpacing ||
    moved[axis.end] + minSpacing < sibling[axis.start];
  return overlapsAcross && isApart;
}

function getSpacingCandidates(
  moved: Bounds,
  before: Bounds[],
  after: Bounds[],
  axis: Axis,
): SpacingCandidate[] {
  const length = lengthOf(moved, axis);
  const previous = findNearestBefore(before, moved, axis);
  const next = findNearestAfter(after, moved, axis);
  const candidates: SpacingCandidate[] = [];
  if (previous && next) {
    const gap = (next[axis.start] - previous[axis.end] - length) / 2;
    candidates.push({ start: previous[axis.end] + gap, gap });
  }
  const earlier = previous && findNearestBefore(before, previous, axis);
  if (previous && earlier) {
    const gap = previous[axis.start] - earlier[axis.end];
    candidates.push({ start: previous[axis.end] + gap, gap });
  }
  const later = next && findNearestAfter(after, next, axis);
  if (next && later) {
    const gap = later[axis.start] - next[axis.end];
    candidates.push({ start: next[axis.start] - gap - length, gap });
  }
  return candidates;
}

function getEqualGapChain(
  snapped: Bounds,
  before: Bounds[],
  after: Bounds[],
  gap: number,
  axis: Axis,
): Bounds[] {
  const chain = [snapped];
  let previous = findNearestBefore(before, snapped, axis);
  while (previous && isSameGap(chain[0][axis.start] - previous[axis.end], gap)) {
    chain.unshift(previous);
    previous = findNearestBefore(before, previous, axis);
  }
  let next = findNearestAfter(after, snapped, axis);
  while (next && isSameGap(next[axis.start] - chain[chain.length - 1][axis.end], gap)) {
    chain.push(next);
    next = findNearestAfter(after, next, axis);
  }
  return chain;
}

function isSameGap(gap: number, expectedGap: number) {
  return Math.abs(gap - expectedGap) < 1;
}

function findNearestBefore(cells: Bounds[], reference: Bounds, axis: Axis): Bounds | undefined {
  return cells
    .filter(cell => cell !== reference && cell[axis.end] <= reference[axis.start])
    .sort(
      (first, second) =>
        second[axis.end] - first[axis.end] || second[axis.start] - first[axis.start],
    )[0];
}

function findNearestAfter(cells: Bounds[], reference: Bounds, axis: Axis): Bounds | undefined {
  return cells
    .filter(cell => cell !== reference && cell[axis.start] >= reference[axis.end])
    .sort(
      (first, second) =>
        first[axis.start] - second[axis.start] || first[axis.end] - second[axis.end],
    )[0];
}

function findResizeMatch(
  bounds: Bounds,
  edge: Edge,
  axis: Axis,
  targets: SnapTargets,
  minLength: number,
): ResizeMatch | null {
  return (
    findSizeMatch(bounds, edge, axis, sortNearestFirst(targets.siblings, bounds), minLength) ??
    findEdgeMatch(
      bounds,
      edge,
      axis,
      sortNearestFirst(getAlignmentTargets(targets), bounds),
      minLength,
    )
  );
}

function findSizeMatch(
  bounds: Bounds,
  edge: Edge,
  axis: Axis,
  targets: Bounds[],
  minLength: number,
): ResizeMatch | null {
  const length = lengthOf(bounds, axis);
  let best: ResizeMatch | null = null;
  for (const target of targets) {
    const targetLength = lengthOf(target, axis);
    const difference = Math.abs(targetLength - length);
    if (targetLength >= minLength && isBetterResizeMatch(difference, best)) {
      best = {
        kind: 'size',
        axis,
        edge,
        value:
          edge === axis.start ? bounds[axis.end] - targetLength : bounds[axis.start] + targetLength,
        difference,
        target,
      };
    }
  }
  return best;
}

function findEdgeMatch(
  bounds: Bounds,
  edge: Edge,
  axis: Axis,
  targets: Bounds[],
  minLength: number,
): ResizeMatch | null {
  let best: ResizeMatch | null = null;
  for (const target of targets) {
    for (const targetValue of [target[axis.start], centerOf(target, axis), target[axis.end]]) {
      const value = Math.round(targetValue);
      const length = edge === axis.start ? bounds[axis.end] - value : value - bounds[axis.start];
      const difference = Math.abs(targetValue - bounds[edge]);
      if (length >= minLength && isBetterResizeMatch(difference, best)) {
        best = { kind: 'alignment', axis, edge, value, difference, target };
      }
    }
  }
  return best;
}

function isBetterResizeMatch(difference: number, best: ResizeMatch | null) {
  return difference <= alignmentTolerance && (!best || difference < best.difference);
}

function toResizeLines(match: ResizeMatch, snapped: Bounds): SnapLine[] {
  const { axis, target } = match;
  if (match.kind === 'alignment') {
    return [toAlignmentLine(match.value, target, snapped, axis)];
  }
  return [snapped, target].map(measured => ({
    kind: 'size',
    orientation: axis.along,
    position: Math.round((measured[axis.crossStart] + measured[axis.crossEnd]) / 2),
    from: measured[axis.start],
    to: measured[axis.end],
  }));
}

function sortNearestFirst(targets: Bounds[], bounds: Bounds): Bounds[] {
  return [...targets].sort(
    (first, second) => getCenterDistance(first, bounds) - getCenterDistance(second, bounds),
  );
}

function getCenterDistance(first: Bounds, second: Bounds) {
  return Math.hypot(
    centerOf(first, horizontalAxis) - centerOf(second, horizontalAxis),
    centerOf(first, verticalAxis) - centerOf(second, verticalAxis),
  );
}

function shift(bounds: Bounds, axis: Axis, offset: number): Bounds {
  return {
    ...bounds,
    [axis.start]: bounds[axis.start] + offset,
    [axis.end]: bounds[axis.end] + offset,
  };
}

function lengthOf(bounds: Bounds, axis: Axis) {
  return bounds[axis.end] - bounds[axis.start];
}

function centerOf(bounds: Bounds, axis: Axis) {
  return (bounds[axis.start] + bounds[axis.end]) / 2;
}
