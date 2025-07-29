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
import { DropdownColumnDrivers, DropdownDataTable } from "modules/Editors/DropdownEditor/DropdownTableModel";
import { IDriverState } from "modules/Editors/DropdownEditor/Cells/IDriverState";
import { findStopping } from "xmlInterpreters/xmlUtils";
import { getMomentFormat } from "xmlInterpreters/getMomentFormat";
import { TextCellDriver } from "modules/Editors/DropdownEditor/Cells/TextCellDriver";
import { NumberCellDriver } from "modules/Editors/DropdownEditor/Cells/NumberCellDriver";
import { BooleanCellDriver } from "modules/Editors/DropdownEditor/Cells/BooleanCellDriver";
import { DateCellDriver } from "modules/Editors/DropdownEditor/Cells/DateCellDriver";
import { DefaultHeaderCellDriver } from "modules/Editors/DropdownEditor/Cells/HeaderCell";

export class DropdownEditorSetup {
  constructor(
    public propertyId: string,
    public lookupId: string,
    public columnNames: string[],
    public visibleColumnNames: string[],
    public columnNameToIndex: Map<string, number>,
    public showUniqueValues: boolean,
    public identifier: string,
    public identifierIndex: number,
    public parameters: { [key: string]: any },
    public dropdownType: string,
    public cached: boolean,
    public searchByFirstColumnOnly: boolean,
    public columnDrivers: DropdownColumnDrivers,
    public isLink?: boolean,
  ) {
  }
}

export function DropdownEditorSetupFromXml(
  xmlNode: any,
  dropdownEditorDataTable: DropdownDataTable,
  dropdownEditorBehavior: IDriverState,
  customStyle: {[key: string]: string} | undefined,
  isLink: boolean | undefined
): DropdownEditorSetup {
  const attributes = xmlNode.attributes;
  const lookupId = attributes.LookupId;
  const propertyId = attributes.Id;
  const showUniqueValues = attributes.DropDownShowUniqueValues === "true";
  const identifier = attributes.Identifier;
  let identifierIndex = 0;
  const dropdownType = attributes.DropDownType;
  const cached = attributes.Cached === "true";
  const searchByFirstColumnOnly = attributes.SearchByFirstColumnOnly === "true";

  const columnNames: string[] = [identifier];
  const visibleColumnNames: string[] = [];
  const columnNameToIndex = new Map<string, number>([[identifier, identifierIndex]]);
  let index = 0;
  const drivers = new DropdownColumnDrivers();
  drivers.customFieldStyle = customStyle;
  if (attributes.SuppressEmptyColumns === "true") {
    drivers.driversFilter = (driver) => {
      return dropdownEditorDataTable.columnIdsWithNoData.indexOf(driver.columnId) < 0;
    };
  }
  for (let propertyXml of findStopping(xmlNode, (n) => n.name === "Property")) {
    index++;
    const attributes = propertyXml.attributes;
    const id = attributes.Id;
    columnNames.push(id);
    columnNameToIndex.set(id, index);

    const formatterPattern = attributes.FormatterPattern
      ? getMomentFormat(propertyXml)
      : "";
    visibleColumnNames.push(id);
    const name = attributes.Name;
    const column = attributes.Column;

    let bodyCellDriver;
    switch (column) {
      case "Text":
        bodyCellDriver = new TextCellDriver(
          index,
          dropdownEditorDataTable,
          dropdownEditorBehavior,
          index == 1 ? customStyle : undefined
        );
        break;
      case "Number":
        bodyCellDriver = new NumberCellDriver(
          index,
          dropdownEditorDataTable,
          dropdownEditorBehavior
        );
        break;
      case "CheckBox":
        bodyCellDriver = new BooleanCellDriver(
          index,
          dropdownEditorDataTable,
          dropdownEditorBehavior
        );
        break;
      case "Date":
        bodyCellDriver = new DateCellDriver(
          index,
          dropdownEditorDataTable,
          dropdownEditorBehavior,
          formatterPattern
        );
        break;
      default:
        throw new Error("Unknown column type " + column);
    }

    drivers.allDrivers.push({
      columnId: id,
      headerCellDriver: new DefaultHeaderCellDriver(name, index == 1 ? customStyle : undefined),
      bodyCellDriver,
    });
  }
  const parameters: { [k: string]: any } = {};

  for (let ddp of findStopping(xmlNode, (n) => n.name === "ComboBoxParameterMapping")) {
    const pat = ddp.attributes;
    parameters[pat.ParameterName] = pat.FieldName;
  }
  return new DropdownEditorSetup(
    propertyId,
    lookupId,
    columnNames,
    visibleColumnNames,
    columnNameToIndex,
    showUniqueValues,
    identifier,
    identifierIndex,
    parameters,
    dropdownType,
    cached,
    searchByFirstColumnOnly,
    drivers,
    isLink
  );
}

