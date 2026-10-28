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

import { RootStoreContext, T } from '@/main.tsx';
import { PropertyValue } from '@api/IArchitectApi';
import S from '@editors/propertyEditor/OpenReferenceButton.module.scss';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler.ts';
import { useContext } from 'react';
import { VscLinkExternal } from 'react-icons/vsc';

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/iu;

export const isSchemaItemId = (value: PropertyValue | undefined): value is string =>
  typeof value === 'string' && GUID_PATTERN.test(value);

export const OpenReferenceButton = (props: { schemaItemId: string; dataTestId: string }) => {
  const rootStore = useContext(RootStoreContext);

  const handleClick = () => {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: function* () {
        yield* rootStore.editorTabViewState.openEditorByOrigamId(props.schemaItemId)();
        yield* rootStore.modelTreeState.revealSchemaItem(props.schemaItemId);
      },
    });
  };

  return (
    <button
      type="button"
      className={S.openButton}
      onClick={handleClick}
      title="Open in editor"
      data-test-id={props.dataTestId}
    >
      <VscLinkExternal />
      {T('Open', 'property_open_referenced_item')}
    </button>
  );
};
