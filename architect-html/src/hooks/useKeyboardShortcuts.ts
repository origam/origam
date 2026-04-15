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

import { useEffect, useRef } from 'react';

type ShortcutDef = {
  predicate: (e: KeyboardEvent) => boolean;
  handler: () => void;
};

export function useKeyboardShortcuts(shortcuts: ShortcutDef[]): void {
  const ref = useRef(shortcuts);
  ref.current = shortcuts;

  useEffect(() => {
    const onKeyDown = (e: KeyboardEvent) => {
      for (const { predicate, handler } of ref.current) {
        if (predicate(e)) {
          e.preventDefault();
          handler();
          break;
        }
      }
    };
    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, []);
}
