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

export class AboutView extends React.Component<{aboutInfo: IAboutInfo}> {
  render() {
    const commitId = (window as any).ORIGAM_CLIENT_REVISION_HASH;
    const commitDate = (window as any).ORIGAM_CLIENT_REVISION_DATE;
    return (
      <div className={S.root}>
        <div>Server version:</div>
        <div className={S.version}>{this.props.aboutInfo.serverVersion}</div>
        <br/>
        <div>Client version:</div>
        <div className={S.version}>
          <div>{"Commit ID: "}
            <a href={"https://github.com/origam/origam/commit/" + commitId}>{commitId}</a>
          </div>
          <div>Commit Date: {commitDate}</div>
        </div>
        <br/>
        <div>
          <a href={"/Attributions.txt"} target="_blank" rel="noreferrer">Copyright attributions</a>
        </div>
        <br/>
        <div>Copyright 2020 Advantage Solutions, s. r. o.</div>
        <br/>
      </div>
    );
  }
}



