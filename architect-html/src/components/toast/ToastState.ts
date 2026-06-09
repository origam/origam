/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.
This file is part of ORIGAM (http://www.origam.org).
*/

import { ISearchResult } from '@api/IArchitectApi';
import { action, observable } from 'mobx';

export type ToastKind = 'success' | 'info';

export interface IActionResultToast {
  id: number;
  kind: ToastKind;
  title: string;
  results: ISearchResult[];
  /** Called when the user clicks the action button (e.g. "Show result"). */
  onShowResult?: () => void;
  /** Total auto-dismiss delay in ms. */
  durationMs: number;
}

const DEFAULT_DURATION_MS = 6000;

export class ToastState {
  @observable.shallow accessor toasts: IActionResultToast[] = [];

  private nextId = 1;
  private timers = new Map<number, ReturnType<typeof setTimeout>>();
  /** Remaining ms when the timer is paused (mouse hover). */
  private remaining = new Map<number, number>();
  /** Wall-clock timestamp when the active timer was last started. */
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
    const idx = this.toasts.findIndex(t => t.id === id);
    if (idx === -1) return;
    this.toasts.splice(idx, 1);
    this.clearTimer(id);
    this.remaining.delete(id);
    this.startedAt.delete(id);
  }

  /** Pause the auto-dismiss countdown for the given toast (mouse enter). */
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

  /** Resume the auto-dismiss countdown (mouse leave). */
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
