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
﻿using System.Web;
using Origam.ServerCommon.Pages;

namespace Origam.Server.Pages
{
    internal class FxHttpContextWrapper : IHttpContextWrapper
    {
        private readonly HttpContext context;

        public FxHttpContextWrapper(HttpContext context)
        {
            this.context = context;
            Response = new FxHttpResponseWrapper(context.Response);
            Request=new FxHttpRequestWrapper(context.Request);
        }

        public IResponseWrapper Response { get; }
        public IRequestWrapper Request { get; }
    }
}