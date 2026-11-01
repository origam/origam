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

export interface Bounds {
  left: number;
  top: number;
  right: number;
  bottom: number;
}

export interface SnapLine {
  orientation: 'vertical' | 'horizontal';
  position: number;
  from: number;
  to: number;
}

export const allEdges: Edge[] = ['left', 'right', 'top', 'bottom'];

const snapDistance = 8;

export function findSnapOffset(bounds: Bounds, targets: Bounds[], edges: Edge[]) {
  const verticalEdges = edges.filter(isVertical);
  const horizontalEdges = edges.filter(edge => !isVertical(edge));
  return {
    x: findNearestOffset(bounds, targets, verticalEdges),
    y: findNearestOffset(bounds, targets, horizontalEdges),
  };
}

export function findSnapLines(bounds: Bounds, targets: Bounds[], edges: Edge[]): SnapLine[] {
  return targets.flatMap(target =>
    edges
      .filter(edge => target[edge] === bounds[edge])
      .map(edge =>
        isVertical(edge)
          ? {
              orientation: 'vertical' as const,
              position: bounds[edge],
              from: Math.min(bounds.top, target.top),
              to: Math.max(bounds.bottom, target.bottom),
            }
          : {
              orientation: 'horizontal' as const,
              position: bounds[edge],
              from: Math.min(bounds.left, target.left),
              to: Math.max(bounds.right, target.right),
            },
      ),
  );
}

function findNearestOffset(bounds: Bounds, targets: Bounds[], edges: Edge[]): number {
  const offsets = targets
    .flatMap(target => edges.map(edge => target[edge] - bounds[edge]))
    .filter(offset => Math.abs(offset) <= snapDistance)
    .sort((offsetA, offsetB) => Math.abs(offsetA) - Math.abs(offsetB));
  return offsets[0] ?? 0;
}

function isVertical(edge: Edge) {
  return edge === 'left' || edge === 'right';
}
