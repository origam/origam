SidebarSection indicates a part of a sidebar content.

```jsx
import {Icon} from "../Icon";
import {MainMenuLI} from "src/gui/Components/MainMenu/MainMenuLI";
import {MainMenuUL} from "src/gui/Components/MainMenu/MainMenuUL";
import {MainMenuItem} from "src/gui/Components/MainMenu/MainMenuItem";
import {SidebarSectionDivider} from "src/gui/Components/Sidebar/SidebarSectionDivider";

<>
  <SidebarSection isActive={false}>
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

  <SidebarSection isActive={true}>
    <SidebarSectionDivider/>
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
