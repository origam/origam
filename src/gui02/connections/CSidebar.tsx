import { Icon } from "gui02/components/Icon/Icon";
import { SidebarAlertCounter } from "gui02/components/Sidebar/AlertCounter";
import { LogoSection } from "gui02/components/Sidebar/LogoSection";
import { Sidebar } from "gui02/components/Sidebar/Sidebar";
import { SidebarSection } from "gui02/components/Sidebar/SidebarSection";
import { SidebarSectionDivider } from "gui02/components/Sidebar/SidebarSectionDivider";
import { SidebarSectionHeader } from "gui02/components/Sidebar/SidebarSectionHeader";
import React from "react";
import { CMainMenu } from "./CMainMenu";

export const CSidebar: React.FC = props => (
  <Sidebar>
    <LogoSection>
      <div style={{ width: 150 }}>
        <img src="./img/asap.png" />
      </div>
    </LogoSection>
    <SidebarSection isActive={false}>
      <SidebarSectionDivider />
      <SidebarSectionHeader
        isActive={false}
        icon={
          <>
            <Icon src="./icons/work-queue.svg" />
            <SidebarAlertCounter>50</SidebarAlertCounter>
          </>
        }
        label={<>Work Queues</>}
      />
    </SidebarSection>
    <SidebarSection isActive={false}>
      <SidebarSectionDivider />
      <SidebarSectionHeader
        isActive={false}
        icon={<Icon src="./icons/favorites.svg" />}
        label={"Favorites"}
      />
    </SidebarSection>
    <SidebarSection isActive={true}>
      <SidebarSectionDivider />
      <SidebarSectionHeader
        isActive={true}
        icon={<Icon src="./icons/menu.svg" />}
        label={"Menu"}
      />
      <CMainMenu />
    </SidebarSection>
    <SidebarSection isActive={false}>
      <SidebarSectionDivider />
      <SidebarSectionHeader
        isActive={false}
        icon={<Icon src="./icons/info.svg" />}
        label={"Info"}
      />
    </SidebarSection>
    <SidebarSection isActive={false}>
      <SidebarSectionDivider />
      <SidebarSectionHeader
        isActive={false}
        icon={<Icon src="./icons/search.svg" />}
        label={"Search"}
      />
      <SidebarSectionDivider />
    </SidebarSection>
  </Sidebar>
);
