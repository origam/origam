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
import { Observer } from "mobx-react";

const debugShowTranslations = !!window.localStorage.getItem(
  "debugShowTranslations"
);

let translations = {} as { [k: string]: string };

export async function translationsInit() {
  const locale = getLocaleFromCookie();
  translations = (await axios.get(`locale/localization_${locale}.json`, {}))
    .data;
}

export function T(defaultContent: any, translKey: string, ...p: any[]) {
  return (
    <Observer>
      {() => {
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
            console.error(
              `Could not find translation for: "${translKey}", showing default: "${result}"`
            );
            result = (
              <span
                title={translKey}
                style={{ backgroundColor: "red", color: "white" }}
              >
                {result}
              </span>
            );
          } else {
            result = (
              <span
                title={translKey}
                style={{ backgroundColor: "green", color: "white" }}
              >
                {result}
              </span>
            );
          }
        }
        return <>{result}</>;
      }}
    </Observer>
  );
}

export function TR(defaultContent: any, translKey: string, ...p: any[]) {
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
      console.error(
        `Could not find translation for: "${translKey}", showing default: "${result}"`
      );
      result = `T> !DEFAULT:${result} <T`;
    } else {
      result = `T> ${result} <T`;
    }
  }
  return result;
}
