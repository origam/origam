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

import { getLocaleFromCookie } from "../../utils/cookies";
import _ from "lodash";

const clearCacheLater = _.debounce(() => {
  formatCache.clear();
}, 10000);
const formatCache: any = new Map<any, any>();

function getOrSetCached(fnGetResult: () => any, ...simpleKeys: any[]) {
  let item = formatCache;
  let keyIndex = 0;
  for (let key of simpleKeys) {
    let nextItem = item.get(key);
    if (nextItem === undefined) {
      if (keyIndex === simpleKeys.length - 1) {
        item.set(key, (nextItem = fnGetResult()));
      } else {
        item.set(key, (nextItem = new Map()));
      }
    }
    item = nextItem;
    keyIndex++;
  }
  clearCacheLater();
  return item;
}

export function formatNumberWithLocale(
  customNumericFormat: string | undefined,
  value: number,
  locale: string
) {
  return getOrSetCached(
    () => {
      if (customNumericFormat) {
        const customFormat = new CustomNumericFormat(customNumericFormat);
        return value.toLocaleString(locale, {
          minimumFractionDigits: customFormat.minimumFractionDigits,
          maximumFractionDigits: customFormat.maximumFractionDigits,
          useGrouping: customFormat.useThousandsSeparator,
        });
      } else {
        return value.toLocaleString(locale);
      }
    },
    locale,
    customNumericFormat,
    value
  );
}

export function getCurrentDecimalSeparator() {
  return getSeparator("decimal");
}

export function getCurrentGroupSeparator() {
  return getSeparator("group");
}

function getSeparator(separatorType: string) {
  const locale = getLocaleFromCookie();
  const numberWithDecimalSeparator = 1000.1;
  return Intl.NumberFormat(locale)
    .formatToParts(numberWithDecimalSeparator)
    .find((part) => part.type === separatorType)!.value;
}

export function formatNumber(
  customNumericFormat: string | undefined,
  dataType: string,
  value: number
) {
  if(isNaN(value)){
    return "";
  }
  const locale = getLocaleFromCookie();
  if (customNumericFormat) {
    return formatNumberWithLocale(customNumericFormat, value, locale);
  }
  if (dataType === "Currency" || dataType === "Float") {
    return formatNumberWithLocale("### ###.00##########", value, locale);
  } else {
    return formatNumberWithLocale("### ###", value, locale);
  }
}

class CustomNumericFormat {
  private format: string;
  private fractionFormat: string | undefined;

  constructor(format: string) {
    const containsAllowedChar = RegExp("^[# 0.]+$").test(format);
    if (!containsAllowedChar) {
      throw new Error('Custom format contains unimplemented characters: "' + format + '"');
    }
    this.format = format.trim();
    this.fractionFormat = this.getFractionFormat(this.format);
  }

  getFractionFormat(format: string) {
    const splitByPoint = format.split(".");
    return splitByPoint.length < 2 ? undefined : splitByPoint[1];
  }

  get maximumFractionDigits() {
    return this.fractionFormat ? this.fractionFormat.length : undefined;
  }

  get minimumFractionDigits() {
    if (!this.fractionFormat) {
      return undefined;
    }
    const minDigits = this.fractionFormat.split("0").length - 1;
    return minDigits === 0 ? undefined : minDigits;
  }

  get useThousandsSeparator() {
    return this.format.includes(" ");
  }
}
