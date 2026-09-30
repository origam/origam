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

import { IModelCheckResult } from '@api/IArchitectApi';
import { RootStore } from '@stores/RootStore';
import { observable } from 'mobx';

export function formatRunTime(lastRunAt: string) {
  return new Date(lastRunAt).toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  });
}

export class ModelCheckState {
  @observable accessor isRunning = false;
  @observable accessor lastResult: IModelCheckResult | null = null;

  constructor(private rootStore: RootStore) {}

  get problemCount() {
    if (!this.lastResult) {
      return 0;
    }
    const fileProblemCount = this.lastResult.fileErrorSections.reduce(
      (count, section) => count + section.errors.length,
      0,
    );
    return this.lastResult.ruleErrors.length + fileProblemCount;
  }

  *loadLastResult(): Generator<Promise<any>, void, any> {
    const result = (yield this.rootStore.architectApi.getModelCheckResult()) as IModelCheckResult;
    // The server returns an empty result until the first run.
    this.lastResult = result.lastRunAt ? result : null;
  }

  *run(): Generator<Promise<any>, void, any> {
    if (this.isRunning) {
      return;
    }
    this.isRunning = true;
    try {
      const result = (yield this.rootStore.architectApi.runModelCheck()) as IModelCheckResult;
      this.lastResult = result;
      this.rootStore.editorTabViewState.openModelCheckResults(result);
    } finally {
      this.isRunning = false;
    }
  }
}
