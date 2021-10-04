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
import S from "RootError.module.scss";
import cx from "classnames";
import {T} from "utils/translation";

export class RootError extends React.Component<{
  error: any
}> {

  getHelpUrl(){
    if(window.navigator?.platform?.toLowerCase()?.includes("win")){ // platform is deprecated but there seems to be no alternative
      return "https://support.microsoft.com/en-us/windows/how-to-set-your-time-and-time-zone-dfaa7122-479f-5b98-2a7b-fa0b6e01b261"
    }
    if(window.navigator?.platform?.toLowerCase()?.includes("mac")){
      return "https://support.apple.com/en-gb/guide/mac-help/mchlp2996/mac"
    }
    return ""
  }

  render(){
    if(this.props.error.message.includes("iat is in the future")){
      const errorMessage = T("Login is not possible due to discrepancy between time on the server and on your machine. Please make sure that time on your machine is correct.", "login_error_due_to_time");
      const helpUrl = this.getHelpUrl();

      return(
        <div className={cx(S.alert, S.alertDanger)}>
          <p>{errorMessage}</p>
          {helpUrl && <a href={helpUrl} target="_blank" rel="noopener noreferrer">{T("Help", "switch_time_help")}</a>}
        </div>);
    }else{
      return <div className={cx(S.alert, S.alertDanger)}>{this.props.error.message}</div>
    }
  }
}