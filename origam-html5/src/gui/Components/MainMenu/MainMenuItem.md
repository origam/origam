/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

This is an item of the main menu.

```jsx
import { Icon } from "../Icon";

<>
  <MainMenuItem
    level={1}
    isActive={false}
    icon={<Icon src="./icons/folder-closed.svg" />}
    label={"Menu item 1"}
  />

  <MainMenuItem
    level={1}
    isActive={false}
    icon={<Icon src="./icons/folder-open.svg" />}
    label={"Menu item 2"}
  />

  <MainMenuItem
    level={2}
    isActive={true}
    icon={<Icon src="./icons/settings.svg" />}
    label={"Menu item 3"}
  />

  <MainMenuItem
    level={3}
    isActive={false}
    icon={<Icon src="./icons/document.svg" />}
    label={"Menu item 4"}
  />
</>;
```

Main menu items are supposed to be rendered in a ul-li structure like this:

```jsx
import {Icon} from "../Icon";
import {MainMenuLI} from "src/gui/Components/MainMenu/MainMenuLI";
import {MainMenuUL} from "src/gui/Components/MainMenu/MainMenuUL";

<MainMenuUL>
  <MainMenuLI>
    <MainMenuItem
      level={1}
      isActive={false}
      icon={<Icon src="./icons/folder-closed.svg"/>}
      label={"Menu item 1"}
    />
  </MainMenuLI>
  <MainMenuLI>
    <MainMenuItem
      level={1}
      isActive={false}
      icon={<Icon src="./icons/folder-closed.svg"/>}
      label={"Menu item 2"}
    />
  </MainMenuLI>
  <MainMenuLI>
    <MainMenuItem
      level={1}
      isActive={true}
      icon={<Icon src="./icons/folder-open.svg"/>}
      label={"Menu item 3"}
    />
  </MainMenuLI>
  <MainMenuLI>
    <MainMenuUL>
      <MainMenuLI>
        <MainMenuItem
          level={2}
          isActive={false}
          icon={<Icon src="./icons/folder-closed.svg"/>}
          label={"Menu item 4"}
        />
      </MainMenuLI>
      <MainMenuLI>
        <MainMenuItem
          level={2}
          isActive={false}
          icon={<Icon src="./icons/document.svg"/>}
          label={"Menu item 5"}
        />
      </MainMenuLI>
      <MainMenuLI>
        <MainMenuItem
          level={2}
          isActive={false}
          icon={<Icon src="./icons/settings.svg"/>}
          label={"Menu item 6"}
        />
      </MainMenuLI>
    </MainMenuUL>
  </MainMenuLI>
</MainMenuUL>;
```
