The header names the a SidebarSection.

```jsx
import { Icon } from "../Icon";
import { MainMenuLI } from "../MainMenu/MainMenuLI";
import { MainMenuUL } from "../MainMenu/MainMenuUL";
import { MainMenuItem } from "../MainMenu/MainMenuItem";
import { SidebarSection } from "./SidebarSection";
import { SidebarSectionDivider } from "./SidebarSectionDivider";

<>
  <SidebarSection isActive={false}>
    <SidebarSectionHeader
      isActive={false}
      icon={<Icon src="./icons/work-queue.svg" />}
      label={"Work queue"}
    />
  </SidebarSection>
  <SidebarSection isActive={false}>
    <SidebarSectionDivider />
    <SidebarSectionHeader
      isActive={false}
      icon={<Icon src="./icons/info.svg" />}
      label={"Information"}
    />
  </SidebarSection>
  <SidebarSection isActive={true}>
    <SidebarSectionDivider />
    <SidebarSectionHeader
      isActive={true}
      icon={<Icon src="./icons/menu.svg" />}
      label={"Menu"}
    />
    <MainMenuUL>
      <MainMenuLI>
        <MainMenuItem
          level={1}
          isActive={false}
          icon={<Icon src="./icons/folder-closed.svg" />}
          label={"Menu item 1"}
        />
      </MainMenuLI>
    </MainMenuUL>
  </SidebarSection>
</>;
```
