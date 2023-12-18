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

import { getLocaleFromCookie } from "utils/cookies";
import MessageFormat from '@messageformat/core';
import { formatNumber } from "model/entities/NumberFormating";
import { ILocalizer } from "plugins/interfaces/ILocalizer";
import { ILocalization } from "plugins/interfaces/ILocalization";
import { DataType } from "plugins/interfaces/DataType";

export class Localizer implements ILocalizer {

  private messageFormat: MessageFormat;
  private defaultMessageFormat: MessageFormat;
  private _activeLocalization: ILocalization | undefined;
  private _defaultLocalization: ILocalization | undefined;
  public locale;

  get defaultLocalization() {
    if (!this._defaultLocalization) {
      this._defaultLocalization = this.getLocalization(this.defaultLocale);
    }
    return this._defaultLocalization;
  }

  get activeLocalization() {
    if (!this._activeLocalization) {
      this._activeLocalization = this.getLocalization(this.locale);
    }
    return this._activeLocalization;
  }

  constructor(private localizations: ILocalization[], private defaultLocale: string) {
    this.locale = getLocaleFromCookie()
    this.messageFormat = new MessageFormat(this.locale);
    this.defaultMessageFormat = this.locale === defaultLocale
      ? this.messageFormat
      : new MessageFormat(this.locale);
  }

  formatNumber(value: number, dataType: DataType, customNumericFormat?: string): string {
    let dataTypeStr;
    if(dataType === DataType.Currency){
      dataTypeStr = "Currency"
    }
    else if(dataType === DataType.Float){
      dataTypeStr = "Float";
    }
    else{
      throw new Error("Data type not implemented: " + dataType)
    }
    return formatNumber(customNumericFormat, dataTypeStr, value);
  }

  private getLocalization(locale: string) {
    const localeLower = locale.toLowerCase();
    let localization = this.localizations.find(localization => localization.locale.toLowerCase() === localeLower);
    if (!localization) {
      localization = this.localizations.find(localization => localization.locale.toLowerCase() === localeLower.split("-")[0]);
    }
    if (!localization) {
      throw new Error(`Plugin localization for locale: "${locale}" was not found`)
    }
    return localization
  }

  public translate(key: string, parameters?: { [key: string]: any }) {
    let translation = this.activeLocalization.translations[key]
      ?? this.defaultLocalization.translations[key];
    if (!translation) {
      throw new Error(`No translation was found for: "${key}" `)
    }

    const message = this.messageFormat.compile(translation);
    return parameters
      ? message(parameters)
      : message();
  }
}


