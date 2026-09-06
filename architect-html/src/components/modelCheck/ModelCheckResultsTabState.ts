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
import { ITabState } from '@/components/editorTabView/ITabState';
import { T } from '@/main';
import { observable } from 'mobx';

export class ModelCheckResultsTabState implements ITabState {
  @observable accessor result: IModelCheckResult;
  @observable accessor isActive = false;
  @observable accessor label = '';

  isDirty = false;

  constructor(
    public tabId: string,
    result: IModelCheckResult,
  ) {
    this.result = result;
    this.label = T('Model validator', 'modelcheck_results_title');
  }

  save(): Generator<Promise<any>, void, any> {
    throw new Error('Method not implemented');
  }
}
