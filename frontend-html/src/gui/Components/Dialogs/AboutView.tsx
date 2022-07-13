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

import React from "react";
import S from "gui/Components/Dialogs/AboutView.module.scss";
import { IAboutInfo } from "model/entities/types/IAboutInfo";
import { T } from "utils/translation";

export class AboutView extends React.Component<{ aboutInfo: IAboutInfo }> {
  render() {
    const customClientBuildVersion = (window as any).ORIGAM_CUSTOM_CLIENT_BUILD as string;
    const uiPluginVersions = (window as any).ORIGAM_UI_PLUGINS as string;
    const uiPluginVersionList = uiPluginVersions ? uiPluginVersions.split(";") : [];
    const serverPluginVersions = (window as any).ORIGAM_SERVER_PLUGINS as string;
    const serverPluginVersionList = serverPluginVersions ? serverPluginVersions.split(";") : [];
    const pluginVersionList = [...uiPluginVersionList, ...serverPluginVersionList]

    return (
      <div className={S.root}>
        <div>
          {T("Origam image version: {0}","origam_image_version", this.props.aboutInfo.serverVersion)}
        </div>
        <br/>
        {customClientBuildVersion &&
          <>
            <div>
              {T("Custom client build version: {0}","custom_client_version", customClientBuildVersion)}
            </div>
            <br/>
          </>
        }
        {pluginVersionList.length > 0 &&
          <>
            <div>{T("Used Origam plugins:","used_origam_plugins")}</div>
            {pluginVersionList.map(x => <div className={S.version}>{x}</div>)}
            <br/>
          </>
        }
        <div>
          <a
            href={"/Attributions.txt"}
            target="_blank"
            rel="noreferrer"
          >
            {T("Copyright attributions","copyright_attributions")}
          </a>
        </div>
        <br/>
        <div>&copy; 2004 - 2022 Advantage Solutions, s. r. o.</div>
        <br/>
      </div>
    );
  }
}



