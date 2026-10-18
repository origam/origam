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
import S from '@editors/propertyEditor/SinglePropertyEditor.module.scss';
import { UntypedPropertyInput } from '@editors/propertyEditor/UntypedPropertyInput.tsx';
import { observer } from 'mobx-react-lite';
import { useEffect, useEffectEvent, useRef, useState } from 'react';

const HEX_COLOR = /^#[0-9a-f]{6}$/i;
const NO_COLOR = '#000000';

export const ColorPropertyInput = observer(
  (props: { property: EditorProperty; onChange: (value: string) => void }) => {
    const { property } = props;
    const pickerRef = useRef<HTMLInputElement>(null);
    const storedText = property.value != null ? String(property.value) : '';
    const [shownColor, setShownColor] = useState(storedText);
    const [syncedText, setSyncedText] = useState(storedText);
    // A pending or rejected typed name keeps the last color the server knows.
    if (syncedText !== storedText) {
      setSyncedText(storedText);
      if (storedText === '' || HEX_COLOR.test(storedText)) {
        setShownColor(storedText);
      }
    }
    const hasColor = HEX_COLOR.test(shownColor);
    const onPicked = useEffectEvent((color: string) => props.onChange(color));

    // The native change fires once a color is picked, React onChange on every move.
    useEffect(() => {
      const picker = pickerRef.current;
      if (!picker) {
        return;
      }
      const handleChange = () => onPicked(picker.value);
      picker.addEventListener('change', handleChange);
      return () => picker.removeEventListener('change', handleChange);
    }, []);

    return (
      <>
        <input
          ref={pickerRef}
          type="color"
          className={hasColor ? S.colorPicker : `${S.colorPicker} ${S.colorPickerEmpty}`}
          disabled={property.readOnly}
          aria-label={property.name}
          data-test-id={`property-color-${property.name}`}
          value={hasColor ? shownColor : NO_COLOR}
          onChange={e => setShownColor(e.target.value)}
        />
        <UntypedPropertyInput property={property} onChange={props.onChange} />
      </>
    );
  },
);
