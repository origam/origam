/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
import { IEditorState } from '@components/editorTabView/IEditorState';
import { T } from '@/main';
import { observable } from 'mobx';

export class SearchResultsEditorState implements IEditorState {
  @observable accessor results: ISearchResult[] = [];
  @observable accessor isActive = false;
  @observable accessor label = '';
  @observable accessor query = '';

  isDirty = false;

  constructor(
    public editorId: string,
    query: string,
    results: ISearchResult[],
  ) {
    this.results = results;
    this.query = query;
    this.label = T('Search: {0}', 'editor_search_results_title', query);
  }

  save(): Generator<Promise<any>, void, any> {
    throw new Error('Method not implemented');
  }
}
