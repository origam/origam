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

import { T } from '@/main';
import Button from '@components/Button/Button';
import { useState } from 'react';
import { VscSymbolKey } from 'react-icons/vsc';

const COPIED_FEEDBACK_MS = 1500;

const GuidGeneratorButton = () => {
  const [copied, setCopied] = useState(false);

  const handleOnClick = async () => {
    const guid = crypto.randomUUID();
    try {
      await navigator.clipboard.writeText(guid);
      setCopied(true);
      window.setTimeout(() => setCopied(false), COPIED_FEEDBACK_MS);
    } catch {
      window.prompt(T('Copy GUID', 'guid_button_copy_prompt'), guid);
    }
  };

  return (
    <Button
      type="secondary"
      title={
        copied
          ? T('Copied!', 'guid_button_copied_label')
          : T('New GUID', 'guid_button_label')
      }
      prefix={<VscSymbolKey />}
      onClick={handleOnClick}
      isAnimated
    />
  );
};

export default GuidGeneratorButton;
