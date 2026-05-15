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

const VIRTUAL_OVERSCAN = 5;
const DEFAULT_ITEM_HEIGHT = 26;

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
  const [itemHeight, setItemHeight] = useState(DEFAULT_ITEM_HEIGHT);
  const [scrollTop, setScrollTop] = useState(0);
  const [viewportHeight, setViewportHeight] = useState(0);
  const [menuPos, setMenuPos] = useState<{
    top?: number;
    bottom?: number;
    left: number;
    width: number;
    maxHeight: number;
    openUp: boolean;
  } | null>(null);

  const wrapperRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLUListElement>(null);
  const highlightSourceRef = useRef<'keyboard' | 'mouse'>('keyboard');

  const filteredOptions = useMemo(() => {
    if (filter == null || filter === '') return options;
    const lowerCaseFilter = filter.toLowerCase();
    return options.filter(option => option.name.toLowerCase().includes(lowerCaseFilter));
  }, [options, filter]);

  const commit = useCallback(
    (value: any, options?: { keepFocus?: boolean }) => {
      onChange(value);
      setOpen(false);
      setFilter(null);
      if (!options?.keepFocus) {
        inputRef.current?.blur();
      }
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
      if (openUp) {
        setMenuPos({
          bottom: window.innerHeight - wrapperRect.top + margin,
          left: wrapperRect.left,
          width: wrapperRect.width,
          maxHeight,
          openUp,
        });
      } else {
        setMenuPos({
          top: wrapperRect.bottom + margin,
          left: wrapperRect.left,
          width: wrapperRect.width,
          maxHeight,
          openUp,
        });
      }
    };
    update();
    window.addEventListener('scroll', update, true);
    window.addEventListener('resize', update);
    return () => {
      window.removeEventListener('scroll', update, true);
      window.removeEventListener('resize', update);
    };
  }, [open]);

  useLayoutEffect(() => {
    if (!open || !listRef.current) return;
    const list = listRef.current;
    const probe = list.querySelector<HTMLElement>(`.${S.dropdownItem}`);
    if (probe) {
      const probeHeight = probe.getBoundingClientRect().height;
      if (probeHeight > 0 && Math.abs(probeHeight - itemHeight) > 0.5) {
        setItemHeight(probeHeight);
      }
    }
    setViewportHeight(list.clientHeight);
  }, [open, filteredOptions.length, itemHeight]);

  useEffect(() => {
    if (!open || !listRef.current) return;
    if (highlightSourceRef.current === 'mouse') return;
    const list = listRef.current;
    const itemTop = highlight * itemHeight;
    const itemBottom = itemTop + itemHeight;
    if (itemTop < list.scrollTop) {
      list.scrollTop = itemTop;
    } else if (itemBottom > list.scrollTop + list.clientHeight) {
      list.scrollTop = itemBottom - list.clientHeight;
    }
  }, [highlight, open, itemHeight]);

  const totalCount = filteredOptions.length;
  const effectiveViewport = viewportHeight > 0 ? viewportHeight : (menuPos?.maxHeight ?? 240);
  const startIndex = Math.max(0, Math.floor(scrollTop / itemHeight) - VIRTUAL_OVERSCAN);
  const endIndex = Math.min(
    totalCount,
    Math.ceil((scrollTop + effectiveViewport) / itemHeight) + VIRTUAL_OVERSCAN,
  );
  const topSpacerHeight = startIndex * itemHeight;
  const bottomSpacerHeight = Math.max(0, (totalCount - endIndex) * itemHeight);
  const visibleOptions = filteredOptions.slice(startIndex, endIndex);

  const openDropdown = () => {
    if (disabled) return;
    setOpen(true);
    highlightSourceRef.current = 'keyboard';
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
      else {
        highlightSourceRef.current = 'keyboard';
        setHighlight(current => Math.min(filteredOptions.length - 1, current + 1));
      }
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      highlightSourceRef.current = 'keyboard';
      setHighlight(current => Math.max(0, current - 1));
    } else if (event.key === 'Enter') {
      if (open && filteredOptions[highlight]) {
        event.preventDefault();
        commit(filteredOptions[highlight].value, { keepFocus: true });
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
          highlightSourceRef.current = 'keyboard';
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
            onScroll={event => setScrollTop((event.target as HTMLUListElement).scrollTop)}
            style={{
              top: menuPos.top,
              bottom: menuPos.bottom,
              left: menuPos.left,
              width: menuPos.width,
              maxHeight: menuPos.maxHeight,
            }}
          >
            {totalCount === 0 && <li className={S.dropdownEmpty}>No matches</li>}
            {topSpacerHeight > 0 && (
              <li aria-hidden style={{ height: topSpacerHeight, padding: 0, cursor: 'default' }} />
            )}
            {visibleOptions.map((option, i) => {
              const index = startIndex + i;
              return (
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
                  onMouseEnter={() => {
                    highlightSourceRef.current = 'mouse';
                    setHighlight(index);
                  }}
                >
                  {option.name}
                </li>
              );
            })}
            {bottomSpacerHeight > 0 && (
              <li
                aria-hidden
                style={{ height: bottomSpacerHeight, padding: 0, cursor: 'default' }}
              />
            )}
          </ul>,
          document.body,
        )}
    </div>
  );
});
