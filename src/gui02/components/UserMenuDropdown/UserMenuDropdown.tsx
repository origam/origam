import React from "react";
import {Dropdown} from "../Dropdown/Dropdown";

export const UserMenuDropdown: React.FC = props => (
  <Dropdown>
    {props.children}
  </Dropdown>
);