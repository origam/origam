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
