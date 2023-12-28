/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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

import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { action, observable } from "mobx";
import * as React from "react";
import { NumberEditorProps } from "gui/Components/ScreenElements/Editors/NumberEditor";
import { getCurrentDecimalSeparator, getCurrentGroupSeparator } from "model/entities/NumberFormating";

export interface INumberEditorModel {
  value: string;
  inputRef: React.RefObject<HTMLInputElement>
  getInputType(): string;
  getInputMode(): "numeric" | "decimal" | "text" | "search" | "none" | "tel" | "url" | "email" | undefined;
  handleChange(event: any): void;
  updateTextOverflowState(): void;
}

export function createNumberEditorModel(args:{props: NumberEditorProps, initValue: string}): INumberEditorModel{
  return isMobileLayoutActive(args.props.property)
    ? new MobileNumberEditorModel(args.props, args.initValue)
    : new DesktopNumberEditorModel(args.props, args.initValue);
}

class DesktopNumberEditorModel implements INumberEditorModel {
  @observable
  value: string = "";
  inputRef: React.RefObject<HTMLInputElement> = React.createRef<HTMLInputElement>();

  constructor(protected props: NumberEditorProps, initValue: string ) {
    this.value = initValue;
  }

  getInputType(): string {
    return this.props.isPassword ? "password" : "text";
  }

  getInputMode():  "numeric" | "decimal" | "text" | "search" | "none" | "tel" | "url" | "email" | undefined {
    return undefined;
  }

  @action.bound handleChange(event: any) {
    const {cleanValue, invalidCharactersBeforeCursor} = getValidCharacters(event, this.props.property.isInteger);

    let cursorPosition = 0;
    if (isValidNumber(cleanValue)) {
      this.value = cleanValue;
      cursorPosition = event.target.selectionStart - invalidCharactersBeforeCursor
    } else {
      cursorPosition = event.target.selectionStart - 1;
    }
    if (this.inputRef.current) {
      this.inputRef.current.selectionStart = cursorPosition;
      this.inputRef.current.selectionEnd = cursorPosition;
    }

    this.updateTextOverflowState();
  }

  updateTextOverflowState() {
    if (!this.inputRef.current) {
      return;
    }
    const textOverflow = this.inputRef.current.offsetWidth < this.inputRef.current.scrollWidth
    this.props.onTextOverflowChanged?.(textOverflow ? this.value : undefined);
  }
}

class MobileNumberEditorModel extends DesktopNumberEditorModel {

  getInputType() {
    return this.props.isPassword ? "password" : "numeric";
  }

  getInputMode(): "numeric" | "decimal" | "text" | "search" | "none" | "tel" | "url" | "email" | undefined  {
    return "decimal";
  }

  @action.bound handleChange(event: any) {
    const {cleanValue, invalidCharactersBeforeCursor} = getValidCharacters(event, this.props.property.isInteger);
    if (isValidNumber(cleanValue)) {
      this.value = cleanValue;
    }
    this.updateTextOverflowState();
  }
}

function getValidCharacters(event: any, isInteger: boolean){
  const cleanChars = [];
  let invalidCharactersBeforeCursor = 0;
  for (let i = 0; i < event.target.value.length; i++) {
    const char = event.target.value[i];
    if(isValidCharacter(char, isInteger)){
      cleanChars.push(char)
    }else{
      if(i < event.target.selectionStart){
        invalidCharactersBeforeCursor++;
      }
    }
  }
  return {
    cleanValue:cleanChars.join(""),
    invalidCharactersBeforeCursor: invalidCharactersBeforeCursor
  };
}

function isValidNumber(value: string){
  let formattedValue = value
    .replaceAll(getCurrentGroupSeparator(), "")
    .replaceAll(getCurrentDecimalSeparator(), ".");
  if(value.trim() === "-"){
    return true;
  }
  return !isNaN(Number(formattedValue))
}

function isValidCharacter(char: string, isInteger: boolean){
  if(
    (char === getCurrentDecimalSeparator() && !isInteger) ||
    char === getCurrentGroupSeparator() ||
    char === "-"
  ) {
    return true;
  }
  return !isNaN(parseInt(char, 10))
}