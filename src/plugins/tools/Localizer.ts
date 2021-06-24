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

import {getLocaleFromCookie} from "../../utils/cookies";
import MessageFormat from '@messageformat/core';

export class Localizer {

  private messageFormat: MessageFormat;
  private defaultMessageFormat: MessageFormat;
  private activeLocalization: ILocalization;
  private defaultLocalization: ILocalization;
  public locale;

  constructor(private localizations: ILocalization[], defaultLocale: string) {
    this.locale = getLocaleFromCookie()
    this.activeLocalization = this.getLocalization(this.locale);
    this.defaultLocalization = this.getLocalization(defaultLocale);
    this.messageFormat = new MessageFormat(this.locale);
    this.defaultMessageFormat = this.locale === defaultLocale
      ? this.messageFormat
      : new MessageFormat(this.locale);
  }

  private getLocalization(locale: string){
    let localization = this.localizations.find(localization => localization.locale === locale);
    if(!localization){
      localization = this.localizations.find(localization => localization.locale === locale.split("-")[0]);
    }
    if(!localization){
      throw new Error(`Plugin localization for locale: "${locale}" was not found`)
    }
    return localization
  }

  public translate(key: string, parameters?: {[key: string]: any}) {
    let translation = this.activeLocalization.translations[key]
      ?? this.defaultLocalization.translations[key];
    if(!translation){
        throw new Error(`No translation was found for: "${key}" `)
    }

    const message = this.messageFormat.compile(translation);
    return parameters
      ? message(parameters)
      : message();
  }
}

export interface ILocalization{
  locale: string;
  translations: {[key:string]: string};
}
