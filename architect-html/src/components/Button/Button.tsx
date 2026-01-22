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

import S from '@components/button/Button.module.scss';
import cn from 'classnames';

const Button = ({
  title,
  type,
  prefix,
  isDisabled,
  isAnimated,
  onClick,
}: {
  title: string | React.ReactNode;
  type: 'primary' | 'secondary';
  prefix?: React.ReactNode;
  isDisabled?: boolean;
  isAnimated?: boolean;
  onClick: () => void;
}) => {
  return (
    <div
      className={cn(S.root, {
        [S.primary]: type === 'primary',
        [S.secondary]: type === 'secondary',
        [S.disabled]: isDisabled,
        [S.animate]: isAnimated,
      })}
      onClick={onClick}
    >
      {prefix}
      <span>{title}</span>
    </div>
  );
};

export default Button;
