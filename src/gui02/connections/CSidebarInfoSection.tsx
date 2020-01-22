import React from "react";
import { IInfoSubsection } from "./types";
import { SidebarRecordInfo } from "gui02/components/SidebarInfoSection/SidebarRecordInfo";
import { SidebarRecordAudit } from "gui02/components/SidebarInfoSection/SidebarRecordAudit";
import { useContext } from "react";
import { MobXProviderContext, observer } from "mobx-react";
import { getRecordInfo } from "model/selectors/RecordInfo/getRecordInfo";

export const CSidebarInfoSection: React.FC<{
  activeSubsection: IInfoSubsection;
}> = observer(props => {
  const workbench = useContext(MobXProviderContext).workbench;
  const recordInfo = getRecordInfo(workbench);

  return (
    <>
      {props.activeSubsection === IInfoSubsection.Info &&
        recordInfo.info.length > 0 && (
          <SidebarRecordInfo lines={recordInfo.info} />
        )}
      {props.activeSubsection === IInfoSubsection.Audit && recordInfo.audit && (
        <SidebarRecordAudit />
      )}
    </>
  );
});
