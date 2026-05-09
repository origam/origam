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

import { IDropDownValue } from '@api/IArchitectApi';
import S from '@editors/propertyEditor/SinglePropertyEditor.module.scss';
import { observer } from 'mobx-react-lite';
import { useEffect, useLayoutEffect, useMemo, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { VscChevronDown } from 'react-icons/vsc';

interface FilterableSelectProps {
  options: IDropDownValue[];
  selectedValue: any;
  disabled?: boolean;
  onChange: (value: any) => void;
}

export const FilterableSelect = observer((props: FilterableSelectProps) => {
  const { options, selectedValue, disabled, onChange } = props;

  const selectedOption = useMemo(
    () => options.find(o => String(o.value) === String(selectedValue)),
    [options, selectedValue],
  );
  const selectedLabel = selectedOption?.name ?? '';

  const [open, setOpen] = useState(false);
  const [filter, setFilter] = useState<string | null>(null);
  const [highlight, setHighlight] = useState(0);
  const [menuPos, setMenuPos] = useState<{ top: number; left: number; width: number } | null>(
    null,
  );

  const wrapperRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLUListElement>(null);

  const filteredOptions = useMemo(() => {
    if (filter == null || filter === '') return options;
    const f = filter.toLowerCase();
    return options.filter(o => o.name.toLowerCase().includes(f));
  }, [options, filter]);

  useEffect(() => {
    if (!open) return;
    const handler = (e: MouseEvent) => {
      const target = e.target as Node;
      if (
        wrapperRef.current && !wrapperRef.current.contains(target) &&
        listRef.current && !listRef.current.contains(target)
      ) {
        if (filter != null && filteredOptions[highlight]) {
          commit(filteredOptions[highlight].value);
        } else {
          setOpen(false);
          setFilter(null);
        }
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [open, filter, filteredOptions, highlight]);

  useLayoutEffect(() => {
    if (!open || !wrapperRef.current) return;
    const update = () => {
      if (!wrapperRef.current) return;
      const r = wrapperRef.current.getBoundingClientRect();
      setMenuPos({ top: r.bottom + 2, left: r.left, width: r.width });
    };
    update();
    window.addEventListener('scroll', update, true);
    window.addEventListener('resize', update);
    return () => {
      window.removeEventListener('scroll', update, true);
      window.removeEventListener('resize', update);
    };
  }, [open]);

  useEffect(() => {
    if (!open || !listRef.current) return;
    const list = listRef.current;
    const el = list.children[highlight] as HTMLElement | undefined;
    if (!el) return;
    const itemTop = el.offsetTop;
    const itemBottom = itemTop + el.offsetHeight;
    if (itemTop < list.scrollTop) {
      list.scrollTop = itemTop;
    } else if (itemBottom > list.scrollTop + list.clientHeight) {
      list.scrollTop = itemBottom - list.clientHeight;
    }
  }, [highlight, open]);

  const openDropdown = () => {
    if (disabled) return;
    setOpen(true);
    setHighlight(
      Math.max(
        0,
        filteredOptions.findIndex(o => String(o.value) === String(selectedValue)),
      ),
    );
  };

  const commit = (value: any) => {
    onChange(value);
    setOpen(false);
    setFilter(null);
    inputRef.current?.blur();
  };

  const onKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      if (!open) openDropdown();
      else setHighlight(h => Math.min(filteredOptions.length - 1, h + 1));
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setHighlight(h => Math.max(0, h - 1));
    } else if (e.key === 'Enter') {
      if (open && filteredOptions[highlight]) {
        e.preventDefault();
        commit(filteredOptions[highlight].value);
      }
    } else if (e.key === 'Escape') {
      setOpen(false);
      setFilter(null);
    }
  };

  return (
    <div className={S.selectWrapper} ref={wrapperRef}>
      <input
        ref={inputRef}
        type="text"
        disabled={disabled}
        className={S.filterableInput}
        value={filter ?? selectedLabel}
        onChange={e => {
          setFilter(e.target.value);
          setOpen(true);
          setHighlight(0);
        }}
        onFocus={openDropdown}
        onClick={openDropdown}
        onKeyDown={onKeyDown}
      />
      <VscChevronDown className={S.selectIcon} />
      {open && menuPos &&
        createPortal(
          <ul
            className={S.dropdownList}
            ref={listRef}
            style={{ top: menuPos.top, left: menuPos.left, width: menuPos.width }}
          >
            {filteredOptions.length === 0 && (
              <li className={S.dropdownEmpty}>No matches</li>
            )}
            {filteredOptions.map((o, i) => (
              <li
                key={String(o.value) + o.name}
                className={
                  S.dropdownItem +
                  (i === highlight ? ' ' + S.dropdownItemHighlight : '') +
                  (String(o.value) === String(selectedValue)
                    ? ' ' + S.dropdownItemSelected
                    : '')
                }
                onMouseDown={e => {
                  e.preventDefault();
                  commit(o.value);
                }}
                onMouseEnter={() => setHighlight(i)}
              >
                {o.name}
              </li>
            ))}
          </ul>,
          document.body,
        )}
    </div>
  );
});
