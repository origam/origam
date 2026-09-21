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

import { EditorProperty } from '@editors/gridEditor/EditorProperty.ts';
import { observer } from 'mobx-react-lite';
import { useState } from 'react';

// The server parses the text by the data type of the value and would reject
// a partially typed one, so the text is sent only when the editing ends.
export const UntypedPropertyInput = observer(
  (props: { property: EditorProperty; onChange: (value: string) => void }) => {
    const { property, onChange } = props;
    const [draft, setDraft] = useState<string | null>(null);
    const storedText = property.value != null ? String(property.value) : '';

    const commit = () => {
      if (draft === null) {
        return;
      }
      setDraft(null);
      if (draft !== storedText) {
        onChange(draft);
      }
    };

    return (
      <input
        type="text"
        disabled={property.readOnly}
        data-test-id={`property-input-${property.name}`}
        value={draft ?? storedText}
        onChange={e => setDraft(e.target.value)}
        onBlur={commit}
        onKeyDown={e => {
          if (e.key === 'Enter') {
            commit();
          }
        }}
        title={storedText}
      />
    );
  },
);
