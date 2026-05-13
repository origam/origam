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
import { useCallback, useEffect, useLayoutEffect, useMemo, useRef, useState } from 'react';
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
    () => options.find(option => String(option.value) === String(selectedValue)),
    [options, selectedValue],
  );
  const selectedLabel = selectedOption?.name ?? '';

  const [open, setOpen] = useState(false);
  const [filter, setFilter] = useState<string | null>(null);
  const [highlight, setHighlight] = useState(0);
  const [menuPos, setMenuPos] = useState<{
    top: number;
    left: number;
    width: number;
    maxHeight: number;
    openUp: boolean;
  } | null>(null);

  const wrapperRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLUListElement>(null);

  const filteredOptions = useMemo(() => {
    if (filter == null || filter === '') return options;
    const lowerCaseFilter = filter.toLowerCase();
    return options.filter(option => option.name.toLowerCase().includes(lowerCaseFilter));
  }, [options, filter]);

  const commit = useCallback(
    (value: any) => {
      onChange(value);
      setOpen(false);
      setFilter(null);
      inputRef.current?.blur();
    },
    [onChange],
  );

  useEffect(() => {
    if (!open) return;
    const handler = (event: MouseEvent) => {
      const target = event.target as Node;
      if (
        wrapperRef.current &&
        !wrapperRef.current.contains(target) &&
        listRef.current &&
        !listRef.current.contains(target)
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
  }, [open, filter, filteredOptions, highlight, commit]);

  useLayoutEffect(() => {
    if (!open || !wrapperRef.current) return;
    const update = () => {
      if (!wrapperRef.current) return;
      const wrapperRect = wrapperRef.current.getBoundingClientRect();
      const margin = 2;
      const desired = 240;
      const spaceBelow = window.innerHeight - wrapperRect.bottom - margin;
      const spaceAbove = wrapperRect.top - margin;
      const openUp = spaceBelow < Math.min(desired, 160) && spaceAbove > spaceBelow;
      const maxHeight = Math.max(80, Math.min(desired, openUp ? spaceAbove : spaceBelow));
      const top = openUp
        ? Math.max(margin, wrapperRect.top - margin - maxHeight)
        : wrapperRect.bottom + margin;
      setMenuPos({ top, left: wrapperRect.left, width: wrapperRect.width, maxHeight, openUp });
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
    const element = list.children[highlight] as HTMLElement | undefined;
    if (!element) return;
    const itemTop = element.offsetTop;
    const itemBottom = itemTop + element.offsetHeight;
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
        filteredOptions.findIndex(option => String(option.value) === String(selectedValue)),
      ),
    );
  };

  const onKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      if (!open) openDropdown();
      else setHighlight(current => Math.min(filteredOptions.length - 1, current + 1));
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      setHighlight(current => Math.max(0, current - 1));
    } else if (event.key === 'Enter') {
      if (open && filteredOptions[highlight]) {
        event.preventDefault();
        commit(filteredOptions[highlight].value);
      }
    } else if (event.key === 'Escape') {
      setOpen(false);
      setFilter(null);
    } else if (event.key === 'Tab') {
      if (open && filter != null && filteredOptions[highlight]) {
        commit(filteredOptions[highlight].value);
      } else {
        setOpen(false);
        setFilter(null);
      }
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
        onChange={event => {
          setFilter(event.target.value);
          setOpen(true);
          setHighlight(0);
        }}
        onFocus={openDropdown}
        onClick={openDropdown}
        onKeyDown={onKeyDown}
      />
      <VscChevronDown className={S.selectIcon} />
      {open &&
        menuPos &&
        createPortal(
          <ul
            className={S.dropdownList}
            ref={listRef}
            style={{
              top: menuPos.top,
              left: menuPos.left,
              width: menuPos.width,
              maxHeight: menuPos.maxHeight,
            }}
          >
            {filteredOptions.length === 0 && <li className={S.dropdownEmpty}>No matches</li>}
            {filteredOptions.map((option, index) => (
              <li
                key={String(option.value) + option.name}
                className={
                  S.dropdownItem +
                  (index === highlight ? ' ' + S.dropdownItemHighlight : '') +
                  (String(option.value) === String(selectedValue)
                    ? ' ' + S.dropdownItemSelected
                    : '')
                }
                onMouseDown={event => {
                  event.preventDefault();
                  commit(option.value);
                }}
                onMouseEnter={() => setHighlight(index)}
              >
                {option.name}
              </li>
            ))}
          </ul>,
          document.body,
        )}
    </div>
  );
});
