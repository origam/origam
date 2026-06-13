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

import { ISearchResult } from '@api/IArchitectApi';
import { action, observable } from 'mobx';

export type ToastKind = 'success' | 'info';

export interface IActionResultToast {
  id: number;
  kind: ToastKind;
  title: string;
  results: ISearchResult[];
  onShowResult?: () => void;
  durationMs: number;
}

const DEFAULT_DURATION_MS = 15000;

export class ToastState {
  @observable.shallow accessor toasts: IActionResultToast[] = [];

  private nextId = 1;
  private timers = new Map<number, ReturnType<typeof setTimeout>>();
  private remaining = new Map<number, number>();
  private startedAt = new Map<number, number>();

  @action.bound
  pushActionResult(input: {
    title: string;
    results: ISearchResult[];
    onShowResult?: () => void;
    kind?: ToastKind;
    durationMs?: number;
  }): number {
    const id = this.nextId++;
    const duration = input.durationMs ?? DEFAULT_DURATION_MS;
    const toast: IActionResultToast = {
      id,
      kind: input.kind ?? 'success',
      title: input.title,
      results: input.results,
      onShowResult: input.onShowResult,
      durationMs: duration,
    };
    this.toasts.unshift(toast);
    this.armTimer(id, duration);
    return id;
  }

  @action.bound
  dismiss(id: number) {
    const index = this.toasts.findIndex(toast => toast.id === id);
    if (index === -1) return;
    this.toasts.splice(index, 1);
    this.clearTimer(id);
    this.remaining.delete(id);
    this.startedAt.delete(id);
  }

  pause(id: number) {
    const startedAt = this.startedAt.get(id);
    const remaining = this.remaining.get(id);
    if (startedAt === undefined || remaining === undefined) return;
    this.clearTimer(id);
    const elapsed = Date.now() - startedAt;
    const left = Math.max(0, remaining - elapsed);
    this.remaining.set(id, left);
    this.startedAt.delete(id);
  }

  resume(id: number) {
    const remaining = this.remaining.get(id);
    if (remaining === undefined) return;
    if (this.timers.has(id)) return;
    this.armTimer(id, remaining);
  }

  private armTimer(id: number, durationMs: number) {
    this.remaining.set(id, durationMs);
    this.startedAt.set(id, Date.now());
    const handle = setTimeout(() => this.dismiss(id), durationMs);
    this.timers.set(id, handle);
  }

  private clearTimer(id: number) {
    const handle = this.timers.get(id);
    if (handle !== undefined) {
      clearTimeout(handle);
      this.timers.delete(id);
    }
  }
}
