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
import { Icon } from "../Icon";
import { MainMenuLI } from "./MainMenuLI";
import { MainMenuUL } from "./MainMenuUL";

<MainMenuUL>
  <MainMenuLI>
    <MainMenuItem
      level={1}
      isActive={false}
      icon={<Icon src="./icons/folder-closed.svg" />}
      label={"Menu item 1"}
    />
  </MainMenuLI>
  <MainMenuLI>
    <MainMenuItem
      level={1}
      isActive={false}
      icon={<Icon src="./icons/folder-closed.svg" />}
      label={"Menu item 2"}
    />
  </MainMenuLI>
  <MainMenuLI>
    <MainMenuItem
      level={1}
      isActive={true}
      icon={<Icon src="./icons/folder-open.svg" />}
      label={"Menu item 3"}
    />
  </MainMenuLI>
  <MainMenuLI>
    <MainMenuUL>
      <MainMenuLI>
        <MainMenuItem
          level={2}
          isActive={false}
          icon={<Icon src="./icons/folder-closed.svg" />}
          label={"Menu item 4"}
        />
      </MainMenuLI>
      <MainMenuLI>
        <MainMenuItem
          level={2}
          isActive={false}
          icon={<Icon src="./icons/document.svg" />}
          label={"Menu item 5"}
        />
      </MainMenuLI>
      <MainMenuLI>
        <MainMenuItem
          level={2}
          isActive={false}
          icon={<Icon src="./icons/settings.svg" />}
          label={"Menu item 6"}
        />
      </MainMenuLI>
    </MainMenuUL>
  </MainMenuLI>
</MainMenuUL>;
```
