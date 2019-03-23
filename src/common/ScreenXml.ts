import xmlJs from 'xml-js';
import { IScreenXml } from './types/IScreenXml';

export function parseScreenXml(xmlString: any): IScreenXml {
  return xmlJs.xml2js(xmlString, {
    compact: false,
    alwaysChildren: true,
    addParent: true,
    alwaysArray: true
  }) as IScreenXml;
}