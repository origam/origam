#region license
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
#endregion

using System.Collections.Generic;

namespace Origam.Security.Identity;

public class InternalIdentityResult
{
    public bool Succeeded { get; }
    public IEnumerable<string> Errors { get; }

    public static InternalIdentityResult Success { get; } =
        new InternalIdentityResult(true);

    public static InternalIdentityResult Failed(params string[] errors)
    {
            return new InternalIdentityResult(errors);
        }
        
    public InternalIdentityResult(params string[] errors)
        : this((IEnumerable<string>) errors)
    {
        }

    public InternalIdentityResult(IEnumerable<string> errors)
    {
            Succeeded = false;
            Errors = errors ?? new []{"Unknown Error"};
        }

    private InternalIdentityResult(bool success)
    {
            Succeeded = success;
            Errors = new string[0];
        }
}