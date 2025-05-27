/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

import axios from 'axios';
import { observable } from 'mobx';
import { getLocaleFromCookie, setLocaleToCookie } from 'src/utils/cookie.ts';

const debugShowTranslations = window.localStorage.getItem('debugShowTranslations') === 'true';

export class TranslationsStore {
  @observable accessor locale: string = 'en-US';
  @observable.ref private accessor translations = {} as { [k: string]: string };

  setLocale(locale: string) {
    return function* (this: TranslationsStore) {
      this.locale = locale;
      yield this.translationsInit(locale);
      setLocaleToCookie(locale);
    }.bind(this);
  }

  async translationsInit(locale: string) {
    try {
      const result = await axios.get(`locale/localization_${locale}.json`, {});
      this.translations = result.data;
    } catch (error: any) {
      console.error(error);
    }
  }

  T(defaultContent: any, translKey: string, ...p: any[]) {
    let result;
    let showingDefault = false;
    if (Object.prototype.hasOwnProperty.call(this.translations, translKey)) {
      result = this.translations[translKey];
    } else {
      result = defaultContent;
      showingDefault = true;
    }
    for (let i = 0; i < p.length; i++) {
      result = result.replace(`{${i}}`, p[i]);
    }
    if (debugShowTranslations) {
      if (this.locale === 'en-US' && result !== defaultContent) {
        console.error(
          'default translation for translKey ' +
            translKey +
            ' does not match translation for en-US!',
        );
      }
      if (showingDefault) {
        console.error(
          `Could not find translation for: "${translKey}", showing default: "${result}"`,
        );
        return (
          <span title={translKey} style={{ backgroundColor: 'red' }}>
            {result}
          </span>
        );
      } else {
        return (
          <span title={translKey} style={{ backgroundColor: 'green' }}>
            {result}
          </span>
        );
      }
    } else {
      return result;
    }
  }

  *updateLocale(): Generator<Promise<any>, void, any> {
    const cookieLocale = getLocaleFromCookie();
    if (cookieLocale !== this.locale) {
      yield* this.setLocale(cookieLocale)();
    }
  }
}
