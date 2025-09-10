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

import { EditorProperty } from '@editors/gridEditor/EditorProperty';
import React, { useEffect, useRef, useState } from 'react';

const debounceMs = 300;

export const NumericPropertyInput: React.FC<{
  property: EditorProperty;
  onChange: (value: number | null) => void;
  type: 'integer' | 'float';
}> = ({ property, onChange, type }) => {
  const [inputValue, setInputValue] = useState<string>(
    property.value != null ? String(property.value) : '',
  );
  const parseValue = (val: string): number | null => {
    if (val.trim() === '') {
      return null;
    }
    return type === 'integer' ? parseInt(val, 10) : parseFloat(val);
  };

  useEffect(() => {
    setInputValue(property.value != null ? String(property.value) : '');
  }, [property.value]);

  const debounceRef = useRef<number>(undefined);

  function onValueChange(value: string) {
    setInputValue(value);
    window.clearTimeout(debounceRef.current);
    debounceRef.current = window.setTimeout(() => {
      onChange(parseValue(value));
    }, debounceMs);
  }

  return (
    <input
      type="number"
      step={type === 'float' ? 'any' : '1'}
      disabled={property.readOnly}
      value={inputValue}
      onChange={e => onValueChange(e.target.value)}
      onBlur={() => {
        if (inputValue.trim() === '') {
          setInputValue(property.value != null ? String(property.value) : '');
        } else {
          onChange(parseValue(inputValue));
        }
      }}
    />
  );
};
