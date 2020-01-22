import { Icon } from "gui02/components/Icon/Icon";
import { SidebarAlertCounter } from "gui02/components/Sidebar/AlertCounter";
import { LogoSection } from "gui02/components/Sidebar/LogoSection";
import { Sidebar } from "gui02/components/Sidebar/Sidebar";
import { SidebarSection } from "gui02/components/Sidebar/SidebarSection";
import { SidebarSectionDivider } from "gui02/components/Sidebar/SidebarSectionDivider";
import { SidebarSectionHeader } from "gui02/components/Sidebar/SidebarSectionHeader";
import React from "react";
import { CMainMenu } from "./CMainMenu";
import { observable, action } from "mobx";
import { SidebarSectionBody } from "gui02/components/Sidebar/SidebarSectionBody";
import { observer, MobXProviderContext } from "mobx-react";
import { getWorkQueuesTotalItemsCount } from "model/selectors/WorkQueues/getWorkQueuesTotalItemCount";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { CWorkQueues } from "./CWorkQueues";
import { ISidebarSection, IInfoSubsection } from "./types";
import { CSidebarInfoSection } from "./CSidebarInfoSection";
import { addRecordInfoExpandRequestHandler } from "model/actions-ui/RecordInfo/addRecordInfoExpandRequestHandler";
import { addRecordAuditExpandRequestHandler } from "model/actions-ui/RecordInfo/addRecordAuditExpandRequestHandler";
import { onSidebarInfoSectionCollapsed } from "model/actions-ui/RecordInfo/onSidebarInfoSectionCollapsed";
import { onSidebarAuditSectionExpanded } from "model/actions-ui/RecordInfo/onSidebarAuditSectionExpanded";
import { onSidebarInfoSectionExpanded } from "model/actions-ui/RecordInfo/onSidebarInfoSectionExpanded";

@observer
export class CSidebar extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  @observable _activeSection = ISidebarSection.Menu;
  @observable activeInfoSubsection = IInfoSubsection.Info;

  get activeSection() {
    return this._activeSection;
  }

  set activeSection(value) {
    if (
      this._activeSection === ISidebarSection.Info &&
      value !== this._activeSection
    ) {
      onSidebarInfoSectionCollapsed(this.workbench)();
    }
    if (
      this._activeSection !== ISidebarSection.Info &&
      value === ISidebarSection.Info
    ) {
      if (this.activeInfoSubsection === IInfoSubsection.Info) {
        onSidebarInfoSectionExpanded(this.workbench)();
      }
      if (this.activeInfoSubsection === IInfoSubsection.Audit) {
        onSidebarAuditSectionExpanded(this.workbench)();
      }
    }
    this._activeSection = value;
  }

  @action.bound handleExpandRecordAuditLog() {
    this.activeInfoSubsection = IInfoSubsection.Audit;
    this.activeSection = ISidebarSection.Info;
  }

  @action.bound handleExpandRecordInfo() {
    this.activeInfoSubsection = IInfoSubsection.Info;
    this.activeSection = ISidebarSection.Info;
  }

  disposers: any[] = [];

  componentDidMount() {
    this.disposers.push(
      addRecordInfoExpandRequestHandler(this.workbench)(
        this.handleExpandRecordInfo
      ),
      addRecordAuditExpandRequestHandler(this.workbench)(
        this.handleExpandRecordAuditLog
      )
    );
  }

  componentWillUnmount() {
    this.disposers.forEach(disposer => disposer());
  }

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
            <CSidebarInfoSection activeSubsection={this.activeInfoSubsection} />
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
