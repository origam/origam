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

export class ModelCheckState {
  @observable accessor isRunning = false;

  constructor(private rootStore: RootStore) {}

  *run(): Generator<Promise<any>, void, any> {
    if (this.isRunning) {
      return;
    }
    this.isRunning = true;
    this.rootStore.progressBarState.isWorking = true;
    try {
      const result = (yield this.rootStore.architectApi.runModelCheck()) as IModelCheckResult;
      this.rootStore.editorTabViewState.openModelCheckResults(result);
    } finally {
      this.isRunning = false;
      this.rootStore.progressBarState.isWorking = false;
    }
  }
}
