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
import axios from "axios";
import { getLocaleFromCookie } from "./cookies";

const debugShowTranslations = window.localStorage.getItem("debugShowTranslations") === "true";

let translations = {} as { [k: string]: string };

export async function translationsInit(ctx: any) {
  const locale = getLocaleFromCookie();
  try {
    const result = await axios.get(`locale/localization_${locale}.json`, {});
    translations = result.data
  } catch(error) {
      if(error.response.status === 404){
        const localeParent = locale.split("-")[0]
        axios.get(`locale/localization_${localeParent}.json`, {})
          .then(result => translations = result.data)
          .catch(error2 => console.error(error2)) /* eslint-disable-line no-console */
      }
      console.error(error) /* eslint-disable-line no-console */
  }
}

export function T(defaultContent: any, translKey: string, ...p: any[]) {
  let result;
  let showingDefault = false;
  if (translations.hasOwnProperty(translKey)) {
    result = translations[translKey];
  } else {
    result = defaultContent;
    showingDefault = true;
  }
  for (let i = 0; i < p.length; i++) {
    result = result.replace(`{${i}}`, p[i]);
  }
  if (debugShowTranslations) {
    if (showingDefault) {
      console.error(`Could not find translation for: "${translKey}", showing default: "${result}"`); // eslint-disable-line no-console
      return (
        <span title={translKey} style={{ backgroundColor: "red" }}>
          {result}
        </span>
      );
    } else {
      return (
        <span title={translKey} style={{ backgroundColor: "green" }}>
          {result}
        </span>
      );
    }
  } else {
    return result;
  }
}
