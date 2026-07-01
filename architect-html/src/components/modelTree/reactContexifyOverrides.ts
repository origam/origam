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

const VIEWPORT_PADDING = 8;

function shiftSubmenu(item: HTMLElement) {
  const submenu = item.querySelector<HTMLElement>(':scope > .contexify_submenu');
  if (!submenu) return;

  const submenuHeight = submenu.scrollHeight;
  if (!submenuHeight) return;

  const itemRect = item.getBoundingClientRect();
  const viewportHeight = window.innerHeight;

  let submenuTopViewport = itemRect.top;
  if (submenuTopViewport + submenuHeight > viewportHeight - VIEWPORT_PADDING) {
    submenuTopViewport = viewportHeight - VIEWPORT_PADDING - submenuHeight;
  }
  if (submenuTopViewport < VIEWPORT_PADDING) {
    submenuTopViewport = VIEWPORT_PADDING;
  }

  const relativeTop = submenuTopViewport - itemRect.top;
  submenu.style.setProperty('top', `${relativeTop}px`, 'important');
  submenu.style.setProperty('bottom', 'auto', 'important');
  submenu.classList.remove('contexify_submenu-bottom');
}

function shiftAllSubmenus(menu: HTMLElement) {
  const submenuItems = menu.querySelectorAll<HTMLElement>(
    ':scope > .contexify_item[aria-haspopup="true"]',
  );
  submenuItems.forEach(shiftSubmenu);
}

function watchMenu(menu: HTMLElement) {
  let scheduled = false;

  const schedule = () => {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(() =>
      requestAnimationFrame(() => {
        scheduled = false;
        shiftAllSubmenus(menu);
      }),
    );
  };

  schedule();

  const contentObserver = new MutationObserver(schedule);
  contentObserver.observe(menu, { childList: true, subtree: true });

  const parent = menu.parentNode;
  if (!parent) return;

  const removalObserver = new MutationObserver(() => {
    if (!parent.contains(menu)) {
      contentObserver.disconnect();
      removalObserver.disconnect();
    }
  });
  removalObserver.observe(parent, { childList: true });
}

let installed = false;

export function installContexifyMenuShift() {
  if (installed || typeof document === 'undefined') return;
  installed = true;

  const rootObserver = new MutationObserver(mutations => {
    for (const mutation of mutations) {
      mutation.addedNodes.forEach(node => {
        if (!(node instanceof HTMLElement)) return;
        if (node.classList.contains('contexify') && node.getAttribute('role') === 'menu') {
          watchMenu(node);
        }
      });
    }
  });

  rootObserver.observe(document.body, { childList: true, subtree: true });
}
