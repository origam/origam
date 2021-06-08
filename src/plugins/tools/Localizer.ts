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
    this.activeLocalization =  this.getLocalization(this.locale);
    this.defaultLocalization =  this.getLocalization(defaultLocale);
    this.messageFormat = new MessageFormat(this.locale);
    this.defaultMessageFormat = this.locale === defaultLocale
      ? this.messageFormat
      : new MessageFormat(this.locale);
  }

  getLocalization(locale: string){
    let localization = this.localizations.find(localization => localization.locale === locale);
    if(!localization){
      localization = this.localizations.find(localization => localization.locale === locale.split("-")[0]);
    }
    if(!localization){
      throw new Error(`Plugin localization for locale: "${locale}" was not found`)
    }
    return localization
  }

  translate(key: string, parameters?: {[key: string]: any}) {
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
