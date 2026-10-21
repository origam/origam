#!/usr/bin/env bash
# Prints the standard ORIGAM license header with the current year.
set -euo pipefail

if [[ $# -ne 1 ]]; then
    printf 'Usage: bash %s <cs|ts|tsx|css|scss|cshtml>\n' "$0" >&2
    exit 1
fi

case "$1" in
    cs) opening=$'#region license\n\n/*'; closing=$'*/\n\n#endregion' ;;
    cshtml) opening='@*'; closing='*@' ;;
    ts|tsx|css|scss) opening='/*'; closing='*/' ;;
    *) printf 'Unsupported extension: %s\n' "$1" >&2; exit 1 ;;
esac

cat <<EOF
$opening
Copyright 2005 - $(date +%Y) Advantage Solutions, s. r. o.

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
$closing
EOF
