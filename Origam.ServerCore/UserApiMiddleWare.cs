#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Origam.ServerCommon.Pages;

namespace Origam.ServerCore
{
    public class UserApiMiddleWare
    {
        public UserApiMiddleWare(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context)
        {
            UserApiProcessor userApiProcessor = new UserApiProcessor();
            var contextWrapper = new StandardHttpContextWrapper(context);
            userApiProcessor.Process(contextWrapper);
        }
    }
}