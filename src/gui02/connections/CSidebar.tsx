import { Icon } from "gui02/components/Icon/Icon";
import { SidebarAlertCounter } from "gui02/components/Sidebar/AlertCounter";
import { LogoSection } from "gui02/components/Sidebar/LogoSection";
import { Sidebar } from "gui02/components/Sidebar/Sidebar";
import { SidebarSection } from "gui02/components/Sidebar/SidebarSection";
import { SidebarSectionDivider } from "gui02/components/Sidebar/SidebarSectionDivider";
import { SidebarSectionHeader } from "gui02/components/Sidebar/SidebarSectionHeader";
import React from "react";
import { CMainMenu } from "./CMainMenu";
import { observable } from "mobx";
import { SidebarSectionBody } from "gui02/components/Sidebar/SidebarSectionBody";
import { observer, MobXProviderContext } from "mobx-react";
import { getWorkQueuesTotalItemsCount } from "model/selectors/WorkQueues/getWorkQueuesTotalItemCount";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { CWorkQueues } from "./CWorkQueues";

enum ISidebarSection {
  WorkQueues,
  Favorites,
  Menu,
  Info,
  Search
}

@observer
export class CSidebar extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.application;
  }

  @observable activeSection = ISidebarSection.Menu;

  render() {
    const workQueuesItemsCount = getWorkQueuesTotalItemsCount(this.workbench);
    return (
      <Sidebar>
        <LogoSection>
          <div style={{ width: 150 }}>
            <img src="./img/logo-left.png" />
          </div>
        </LogoSection>
        <SidebarSection
          isActive={this.activeSection === ISidebarSection.WorkQueues}
        >
          <SidebarSectionDivider />
          <SidebarSectionHeader
            isActive={this.activeSection === ISidebarSection.WorkQueues}
            icon={
              <>
                <Icon src="./icons/work-queue.svg" />
                {workQueuesItemsCount > 0 && (
                  <SidebarAlertCounter>
                    {workQueuesItemsCount}
                  </SidebarAlertCounter>
                )}
              </>
            }
            label={<>Work Queues</>}
            onClick={() => (this.activeSection = ISidebarSection.WorkQueues)}
          />
          <SidebarSectionBody
            isActive={this.activeSection === ISidebarSection.WorkQueues}
          >
            <CWorkQueues />
          </SidebarSectionBody>
        </SidebarSection>
        <SidebarSection
          isActive={this.activeSection === ISidebarSection.Favorites}
        >
          <SidebarSectionDivider />
          <SidebarSectionHeader
            isActive={this.activeSection === ISidebarSection.Favorites}
            icon={<Icon src="./icons/favorites.svg" />}
            label={"Favorites"}
            onClick={() => (this.activeSection = ISidebarSection.Favorites)}
          />
          <SidebarSectionBody
            isActive={this.activeSection === ISidebarSection.Favorites}
          >
            &nbsp;
          </SidebarSectionBody>
        </SidebarSection>
        <SidebarSection isActive={this.activeSection === ISidebarSection.Menu}>
          <SidebarSectionDivider />
          <SidebarSectionHeader
            isActive={this.activeSection === ISidebarSection.Menu}
            icon={<Icon src="./icons/menu.svg" />}
            label={"Menu"}
            onClick={() => (this.activeSection = ISidebarSection.Menu)}
          />
          <SidebarSectionBody
            isActive={this.activeSection === ISidebarSection.Menu}
          >
            <CMainMenu />
          </SidebarSectionBody>
        </SidebarSection>
        <SidebarSection isActive={this.activeSection === ISidebarSection.Info}>
          <SidebarSectionDivider />
          <SidebarSectionHeader
            isActive={this.activeSection === ISidebarSection.Info}
            icon={<Icon src="./icons/info.svg" />}
            label={"Info"}
            onClick={() => (this.activeSection = ISidebarSection.Info)}
          />
          <SidebarSectionBody
            isActive={this.activeSection === ISidebarSection.Info}
          >
            &nbsp;
          </SidebarSectionBody>
        </SidebarSection>
        <SidebarSection
          isActive={this.activeSection === ISidebarSection.Search}
        >
          <SidebarSectionDivider />
          <SidebarSectionHeader
            isActive={this.activeSection === ISidebarSection.Search}
            icon={<Icon src="./icons/search.svg" />}
            label={"Search"}
            onClick={() => (this.activeSection = ISidebarSection.Search)}
          />
          <SidebarSectionBody
            isActive={this.activeSection === ISidebarSection.Search}
          >
            &nbsp;
          </SidebarSectionBody>
          <SidebarSectionDivider />
        </SidebarSection>
      </Sidebar>
    );
  }
}
