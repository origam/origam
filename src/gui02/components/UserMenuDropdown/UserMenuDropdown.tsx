import React from "react";
import S from "./UserMenuDropdown.module.scss";
import { Dropdown } from "../Dropdown/Dropdown";
import { UserMenuBlock } from "./UserMenuBlock";

export const UserMenuDropdown: React.FC = props => (
  <Dropdown>
    {props.children}
  </Dropdown>
);