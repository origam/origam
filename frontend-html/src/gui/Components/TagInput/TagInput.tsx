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
import S from "./TagInput.module.css";

export const TagInput: React.FC<{ className?: string }> = (props) => {
  return (
    <div className={S.tagInputContainer + (props.className ? ` ${props.className}` : "")}>
      {props.children}
    </div>
  );
};

export const TagInputAdd: React.FC<{
  domRef?: any;
  className?: string;
  onClick: (event: any) => void;
  onMouseDown?: (event: any) => void;
}> = (props) => {

  return (
    <div
      className={S.tagInputAdd + (props.className ? ` ${props.className}` : "")}
      onClick={(event) => props.onClick(event)}
      onMouseDown={props.onMouseDown}
      ref={props.domRef}
    >
      <i className="fas fa-plus"/>
    </div>
  );
};

export const TagInputItemDelete: React.FC<{
  onClick?: (event: any) => void;
  className?: string;
}> = (props) => {
  return (
    <div
      className={S.tagInputItemDelete + (props.className ? ` ${props.className}` : "")}
      onClick={props.onClick}
    >
      <i className="fas fa-times"/>
    </div>
  );
};

export const TagInputItem: React.FC<{ className?: string }> = (props) => {
  return (
    <div className={S.tagInputItem + (props.className ? ` ${props.className}` : "")}>
      {props.children}
    </div>
  );
};
