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

The header names the a SidebarSection.

```jsx
import {Icon} from "../Icon";
import {MainMenuLI} from "src/gui/Components/MainMenu/MainMenuLI";
import {MainMenuUL} from "src/gui/Components/MainMenu/MainMenuUL";
import {MainMenuItem} from "src/gui/Components/MainMenu/MainMenuItem";
import {SidebarSection} from "src/gui/Components/Sidebar/SidebarSection";
import {SidebarSectionDivider} from "src/gui/Components/Sidebar/SidebarSectionDivider";

<>
  <SidebarSection isActive={false}>
    <SidebarSectionHeader
      isActive={false}
      icon={<Icon src="./icons/work-queue.svg"/>}
      label={"Work queue"}
    />
  </SidebarSection>
  <SidebarSection isActive={false}>
    <SidebarSectionDivider/>
    <SidebarSectionHeader
      isActive={false}
      icon={<Icon src="./icons/info.svg"/>}
      label={"Information"}
    />
  </SidebarSection>
  <SidebarSection isActive={true}>
    <SidebarSectionDivider/>
    <SidebarSectionHeader
      isActive={true}
      icon={<Icon src="./icons/menu.svg"/>}
      label={"Menu"}
    />
    <MainMenuUL>
      <MainMenuLI>
        <MainMenuItem
          level={1}
          isActive={false}
          icon={<Icon src="./icons/folder-closed.svg"/>}
          label={"Menu item 1"}
        />
      </MainMenuLI>
    </MainMenuUL>
  </SidebarSection>
</>;
```
