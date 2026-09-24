/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

const OrigamAiIcon = () => {
  return (
    <svg
      viewBox="0 0 16 16"
      width="1em"
      height="1em"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      aria-hidden="true"
      focusable="false"
    >
      <circle cx="8" cy="1.1" r="1.1" fill="currentColor" />
      <path d="M8 1.6v1.8" stroke="currentColor" strokeWidth="1.3" />
      <rect
        x="1.6"
        y="3.4"
        width="12.8"
        height="10"
        rx="3"
        stroke="currentColor"
        strokeWidth="1.3"
      />
      <circle cx="5.6" cy="8.2" r="1.35" fill="currentColor" />
      <circle cx="10.4" cy="8.2" r="1.35" fill="currentColor" />
    </svg>
  );
};

export default OrigamAiIcon;
