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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.Server.Attributes;

namespace Origam.Server.Controller;

[Controller]
[Route("internalApi/[controller]")]
public class SearchController : AbstractController
{
    private readonly SearchHandler searchHandler;

    public SearchController(
        ILogger<AbstractController> log,
        SessionObjects sessionObjects,
        SearchHandler searchHandler,
        IWebHostEnvironment environment
    )
        : base(log, sessionObjects, environment)
    {
        this.searchHandler = searchHandler;
    }

    // .NetCore 3.1 cannot stream data, looks like this will be possible in .Net 5.0.0
    // https://github.com/dotnet/runtime/issues/1570
    [HttpGet("{*searchTerm}")]
    [DecodeQueryParameter("searchTerm")]
    public IActionResult Get(string searchTerm)
    {
        return RunWithErrorHandler(() => Ok(searchHandler.Search(searchTerm)));
    }
}
